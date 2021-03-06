using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Models.Node;
using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services.Core;

public class NodeRequester : INodeRequester
{
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly NodeApiCacheSingleton _nodeApiCacheSingleton;

    public NodeRequester(ILogger<NodeRequester> logger, IHttpClientFactory httpClientFactory, IOptionsMonitor<ExplorerConfig> explorerConfig, NodeApiCacheSingleton nodeApiCacheSingleton) =>
        (_logger, _explorerConfig, _httpClientFactory, _nodeApiCacheSingleton) = (logger, explorerConfig, httpClientFactory, nodeApiCacheSingleton);


    private void ConfigureHttpClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Url);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Username);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Password);

        httpClient.BaseAddress = new Uri(_explorerConfig.CurrentValue.Node.Url);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_explorerConfig.CurrentValue.Node.Username}:{_explorerConfig.CurrentValue.Node.Password}")));
    }

    public async ValueTask ScanTxOutsetAndCacheAsync(string target, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            ConfigureHttpClient(httpClient);

            var request = new JsonRPCRequest
            {
                Id = 1,
                Method = "scantxoutset",
                Params = new List<object>(new object[] { "start", new object[] { $"addr({target})" } })
            };
            var response = await httpClient.PostAsJsonAsync<JsonRPCRequest>("", request, options, cancellationToken);
            var data = await response.Content.ReadFromJsonAsync<ScanTxOutset>(options, cancellationToken);
            if (data != null && data.Result != null)
                _nodeApiCacheSingleton.SetApiCache($"scantxoutset-{target}", data);
        }
        catch
        {

        }
    }
}