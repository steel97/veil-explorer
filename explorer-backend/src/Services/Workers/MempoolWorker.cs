using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Models.Node;
using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services.Workers;

public class MempoolWorker : BackgroundService
{
    private Uri? _uri;
    private AuthenticationHeaderValue? _authHeader;
    private int _usernameHash;
    private int _passHash;
    private JsonSerializerOptions _options;
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ChaininfoSingleton _chainInfoSingleton;

    public MempoolWorker(ILogger<MempoolWorker> logger, IOptionsMonitor<ExplorerConfig> explorerConfig, IHttpClientFactory httpClientFactory, ChaininfoSingleton chaininfoSingleton)
    {
        _logger = logger;
        _explorerConfig = explorerConfig;
        _httpClientFactory = httpClientFactory;
        _chainInfoSingleton = chaininfoSingleton;
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        ConfigSetup();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        if(_passHash !=_explorerConfig.CurrentValue.Node!.Password!.GetHashCode() || _usernameHash !=_explorerConfig.CurrentValue.Node!.Username!.GetHashCode())        
            ConfigSetup();
                    
        httpClient.BaseAddress = _uri;
        httpClient.DefaultRequestHeaders.Authorization = _authHeader;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // get blockchain info
                var getRawMempoolRequest = new JsonRPCRequest
                {
                    Id = 1,
                    Method = "getrawmempool",
                    Params = new List<object>(new object[] { false })
                };
                var getRawMempoolResult = await httpClient.PostAsJsonAsync<JsonRPCRequest>("", getRawMempoolRequest, _options, cancellationToken);
                var mempoolInfo = await getRawMempoolResult.Content.ReadFromJsonAsync<GetRawMempool>(_options, cancellationToken);
                // get chainalgo stats

                if (mempoolInfo != null && mempoolInfo.Result != null)
                {
                    var unconfirmedTxs = new List<GetRawTransactionResult>();
                    foreach (var txId in mempoolInfo.Result)
                    {
                        var getRawTransactionRequest = new JsonRPCRequest
                        {
                            Id = 1,
                            Method = "getrawtransaction",
                            Params = new List<object>(new object[] { txId, true })
                        };
                        var getRawTransactionResponse = await httpClient.PostAsJsonAsync<JsonRPCRequest>("", getRawTransactionRequest, _options, cancellationToken);
                        var rawTransaction = await getRawTransactionResponse.Content.ReadFromJsonAsync<GetRawTransaction>(_options, cancellationToken);
                        if (rawTransaction != null && rawTransaction.Result != null)
                            unconfirmedTxs.Add(rawTransaction.Result);
                    }

                    _chainInfoSingleton.UnconfirmedTxs = unconfirmedTxs;

                }

                // TimeSpan not reuired here since we use milliseconds, still put it there to change in future if required
                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.PullMempoolDelay), cancellationToken);
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't handle mempool info");
            }
        }
    }
    private void ConfigSetup()
    {
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Url);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Username);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Password);

        _authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_explorerConfig.CurrentValue.Node!.Username}:{_explorerConfig.CurrentValue.Node.Password}")));
        _uri = new Uri(_explorerConfig.CurrentValue.Node!.Url!);
        _usernameHash = _explorerConfig.CurrentValue.Node.Password!.GetHashCode();
        _passHash = _explorerConfig.CurrentValue.Node!.Username!.GetHashCode();
    }
}