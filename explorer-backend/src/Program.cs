using Microsoft.Extensions.Options;
using ExplorerBackend.Hubs;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Services.Workers;
using ExplorerBackend.Services.Queues;
using ExplorerBackend.Persistence.Repositories;

var baseCorsPolicty = "_baseCorsPolicy";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<APIConfig>(builder.Configuration.GetSection("API"));
builder.Services.Configure<ExplorerConfig>(builder.Configuration.GetSection("Explorer"));
builder.Services.Configure<ServerConfig>(builder.Configuration.GetSection("Server"));

builder.Services.AddSingleton<ChaininfoSingleton>();
builder.Services.AddSingleton<NodeApiCacheSingleton>();
builder.Services.AddSingleton<IUtilityService, UtilityService>();
builder.Services.AddSingleton<INodeRequester, NodeRequester>();

builder.Services.AddHostedService<BlocksWorker>();
builder.Services.AddHostedService<BlockchainWorker>();
builder.Services.AddHostedService<HubBackgroundWorker>();
builder.Services.AddHostedService<ScanTxOutsetWorker>();
builder.Services.AddHostedService<SupplyWorker>();
builder.Services.AddHostedService<MempoolWorker>();

builder.Services.AddSingleton<ScanTxOutsetBackgroundTaskQueue>();

builder.Services.AddHostedService<ScanTxOutsetWorker>();

builder.Services.AddTransient<IBlocksRepository, BlocksRepository>();
builder.Services.AddTransient<ITransactionsRepository, TransactionsRepository>();
builder.Services.AddTransient<IRawTxsRepository, RawTxsRepository>();

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
    options.AddPolicy(baseCorsPolicty, corsBuilder =>
    {
        corsOrigins.ToList().ForEach(entry => corsBuilder.WithOrigins(entry).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
    });
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

app.UseCors(baseCorsPolicty);

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<EventsHub>("/api/events");
});

app.Run();
