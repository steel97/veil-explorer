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
    private Uri? _uri;
    private AuthenticationHeaderValue? _authHeader;
    private int _usernameHash;
    private int _passHash;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly NodeApiCacheSingleton _nodeApiCacheSingleton;
    private readonly JsonSerializerOptions serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public NodeRequester(IHttpClientFactory httpClientFactory, IOptionsMonitor<ExplorerConfig> explorerConfig, NodeApiCacheSingleton nodeApiCacheSingleton) =>
        (_explorerConfig, _httpClientFactory, _nodeApiCacheSingleton) = (explorerConfig, httpClientFactory, nodeApiCacheSingleton);

    public async Task<string> NodeRequest(string? method, List<object>? parameters, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();

            if(_passHash !=_explorerConfig.CurrentValue.Node!.Password!.GetHashCode() || _usernameHash !=_explorerConfig.CurrentValue.Node!.Username!.GetHashCode())        
                ConfigureHttpClient();
                    
            httpClient.BaseAddress = _uri;
            httpClient.DefaultRequestHeaders.Authorization = _authHeader;

            var request = new JsonRPCRequest
            {
                Id = 1,
                Method = method,
                Params = parameters
            };
            var response = await httpClient.PostAsJsonAsync<JsonRPCRequest>("", request, serializerOptions, cancellationToken);
            return await response.Content.ReadAsStringAsync(cancellationToken);

        }
        catch
        {

        }

        var error = new GenericResult
        {
            Result = null,
            Error = new()
            {
                Code = -32603,
                Message = "Node failure"
            }
        };
        return JsonSerializer.Serialize<GenericResult>(error, serializerOptions); // RPC_INTERNAL_ERROR -32603
    }

    public async ValueTask ScanTxOutsetAndCacheAsync(string target, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();

            if(_passHash !=_explorerConfig.CurrentValue.Node!.Password!.GetHashCode() || _usernameHash !=_explorerConfig.CurrentValue.Node!.Username!.GetHashCode())        
                ConfigureHttpClient();
                    
            httpClient.BaseAddress = _uri;
            httpClient.DefaultRequestHeaders.Authorization = _authHeader;

            var request = new JsonRPCRequest
            {
                Id = 1,
                Method = "scantxoutset",
                Params = new List<object>(new object[] { "start", new object[] { $"addr({target})" } })
            };
            var response = await httpClient.PostAsJsonAsync<JsonRPCRequest>("", request, serializerOptions, cancellationToken);
            var data = await response.Content.ReadFromJsonAsync<ScanTxOutset>(serializerOptions, cancellationToken);
            if (data != null && data.Result != null)
                _nodeApiCacheSingleton.SetApiCache($"scantxoutset-{target}", data);
        }
        catch
        {

        }
    }
    private void ConfigureHttpClient()
    {
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Url);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Username);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Password);

        _uri = new Uri(_explorerConfig.CurrentValue.Node.Url);
        _authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_explorerConfig.CurrentValue.Node.Username}:{_explorerConfig.CurrentValue.Node.Password}")));
    }
}