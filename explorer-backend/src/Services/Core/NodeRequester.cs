using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using explorer_backend.Configs;
using explorer_backend.Services.Caching;
using explorer_backend.Models.Node;
using explorer_backend.Models.Node.Response;

namespace explorer_backend.Services.Core;

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
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Authorization);

        httpClient.BaseAddress = new Uri(_explorerConfig.CurrentValue.Node.Url);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _explorerConfig.CurrentValue.Node.Authorization);
    }

    public async ValueTask ValidateAddressAndCacheAsync(string target, CancellationToken token)
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
            Method = "validateaddress",
            Params = new List<object>(new object[] { target })
        };
        var response = await httpClient.PostAsJsonAsync<JsonRPCRequest>("", request, options);
        var data = await response.Content.ReadFromJsonAsync<ValidateAddrees>(options);
        if (data != null)
            _nodeApiCacheSingleton.SetApiCache($"validateaddress-{target}", data);
    }

    public async ValueTask ScanTxOutsetAndCacheAsync(string target, CancellationToken token)
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
        var response = await httpClient.PostAsJsonAsync<JsonRPCRequest>("", request, options);
        var data = await response.Content.ReadFromJsonAsync<ScanTxOutset>(options);
        if (data != null)
            _nodeApiCacheSingleton.SetApiCache($"scantxoutset-{target}", data);
    }
}