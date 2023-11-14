using Microsoft.Extensions.Options;
using ExplorerBackend.Hubs;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Services.Workers;
using ExplorerBackend.Services.Queues;
using ExplorerBackend.Persistence.Repositories;
using ExplorerBackend.Core;
using ExplorerBackend.Services;
using Serilog;
using Serilog.Events;
using ExplorerBackend.Services.Workers.Patches;
using ExplorerBackend.Services.Data;

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .WriteTo.Async(a => a.Console())
            .WriteTo.File(new Serilog.Formatting.Compact.CompactJsonFormatter(), "")
            .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext());

// Configuration
builder.Services.Configure<APIConfig>(builder.Configuration.GetSection("API"));
builder.Services.Configure<ExplorerConfig>(builder.Configuration.GetSection("Explorer"));
builder.Services.Configure<ServerConfig>(builder.Configuration.GetSection("Server"));

bool rpcMode = builder.Configuration.GetSection("Explorer:RPCMode").Get<bool>();

// Add services to the container.
if(rpcMode)
    builder.Services.AddStackExchangeRedisCache(redisOpt =>
    {
        redisOpt.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "";
        redisOpt.InstanceName = "Redis";
    });
else
    builder.Services.AddNpgsqlDataSource(builder.Configuration.GetConnectionString("DefaultConnection") ?? "");
    
builder.Services.AddSingleton<ChaininfoSingleton>();
builder.Services.AddSingleton<InternalSingleton>();
builder.Services.AddSingleton<NodeApiCacheSingleton>();
builder.Services.AddSingleton<ScanTxOutsetBackgroundTaskQueue>();
builder.Services.AddSingleton<NodeRequester>();
builder.Services.AddSingleton<IUtilityService, UtilityService>();
builder.Services.AddSingleton<IBlocksService, BlocksService>();

builder.Services.AddHostedService<BlocksWorker>();
builder.Services.AddHostedService<BlockchainWorker>();
builder.Services.AddHostedService<BlockchainStatsWorker>();
builder.Services.AddHostedService<HubBackgroundWorker>();
builder.Services.AddHostedService<ScanTxOutsetWorker>();
builder.Services.AddHostedService<SupplyWorker>();
builder.Services.AddHostedService<MempoolWorker>();
builder.Services.AddHostedService<ScanTxOutsetWorker>();
if (args.Length > 1 && args[0] == "--fixorphans" && !rpcMode)
{
    OrphanFixWorker.StartingBlock = int.Parse(args[1]);
    builder.Services.AddHostedService<OrphanFixWorker>();
}

if(rpcMode)
{
    builder.Services.AddSingleton<BlocksCacheSingleton>();
    builder.Services.AddSingleton<SimplifiedBlocksCacheSingleton>();
    
    builder.Services.AddTransient<IBlocksDataService, RealtimeBlocksDataService>();
    builder.Services.AddTransient<ITransactionsDataService, RealtimeTransactionsDataService>();
    builder.Services.AddTransient<IRawTransactionsDataService, RealtimeRawTransactionsDataService>();
}
else
{
    builder.Services.AddTransient<IBlocksDataService, DbBlocksDataService>();
    builder.Services.AddTransient<ITransactionsDataService, DbTransactionsDataService>();
    builder.Services.AddTransient<IRawTransactionsDataService, DbRawTransactionsDataService>();

    builder.Services.AddTransient<IBlocksRepository, BlocksRepository>();
    builder.Services.AddTransient<ITransactionsRepository, TransactionsRepository>();
    builder.Services.AddTransient<IRawTxsRepository, RawTxsRepository>();
}

builder.Services.AddTransient<ITransactionDecoder, TransactionsDecoder>();

builder.Services.AddHttpClient();
builder.Services.AddSignalR(options => { });
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var corsOrigins = builder.Configuration.GetSection("Server:CorsOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CORSPolicies.BaseCorsPolicy, corsBuilder =>
    {
        corsOrigins?.ToList().ForEach(entry => corsBuilder.WithOrigins(entry).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
    });
    options.AddPolicy(CORSPolicies.NodeProxyPolicy, corsBuilder => corsBuilder
                 .AllowAnyOrigin()
                 .AllowAnyMethod()
                 .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
var swaggerConfig = app.Services.GetRequiredService<IOptions<ServerConfig>>().Value;
if (swaggerConfig.Swagger?.Enabled ?? false)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = swaggerConfig.Swagger.RoutePrefix;
    });
}

app.UseRouting();
app.UseCors(CORSPolicies.BaseCorsPolicy);
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHub<EventsHub>("/api/events");
app.MapHub<InternalHub>("/api/internal");

app.Run();
