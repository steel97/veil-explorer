using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using ExplorerBackend.Configs;
using ExplorerBackend.VeilStructs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Models.API;
using ExplorerBackend.Models.Node;
using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services.Workers;

public class BlockchainStatsWorker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ChaininfoSingleton _chainInfoSingleton;
    private JsonSerializerOptions options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public BlockchainStatsWorker(ILogger<BlockchainStatsWorker> logger, IOptionsMonitor<ExplorerConfig> explorerConfig, IHttpClientFactory httpClientFactory, ChaininfoSingleton chaininfoSingleton)
    {
        _logger = logger;
        _explorerConfig = explorerConfig;
        _httpClientFactory = httpClientFactory;
        _chainInfoSingleton = chaininfoSingleton;
    }

    protected override async Task ExecuteAsync(CancellationToken stopToken)
    {
        using var httpClient = _httpClientFactory.CreateClient();

        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Url);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Authorization);

        httpClient.BaseAddress = new Uri(_explorerConfig.CurrentValue.Node.Url);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _explorerConfig.CurrentValue.Node.Authorization);

        var targetBlocksPerDay = 24 * 60 * 60 / Constants.BLOCK_TIME;

        await Task.Delay(TimeSpan.FromSeconds(5)); // let other services warm up, this allows us not to wait PullBlockchainStatsDelay if some data is unavailable at beginning

        while (!stopToken.IsCancellationRequested)
        {
            try
            {

                var txStatsDay = await GetTxStats(httpClient, targetBlocksPerDay / 4, -144);
                var txStatsWeek = await GetTxStats(httpClient, targetBlocksPerDay / 4, -144 * 7);
                var txStatsMonth = await GetTxStats(httpClient, targetBlocksPerDay / 4, -144 * 30);
                var txStatsOverall = await GetTxStats(httpClient, _explorerConfig.CurrentValue.StatsPointsCount, 0);

                var finalDict = new Dictionary<string, TxStatsEntry>();
                finalDict.Add("day", txStatsDay);
                finalDict.Add("week", txStatsWeek);
                finalDict.Add("month", txStatsMonth);
                finalDict.Add("overall", txStatsOverall);

                _chainInfoSingleton.CurrentChainStats = new TxStatsComposite
                {
                    TxStats = finalDict
                };

                // TimeSpan not reuired here since we use milliseconds, still put it there to change in future if required
                await Task.Delay(TimeSpan.FromMilliseconds(_explorerConfig.CurrentValue.PullBlockchainStatsDelay));
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't handle blockchain stats info");
            }
        }
    }



    private async Task<GetChainTxStatsResult> GetChainTxStats(HttpClient? httpClient, long ctxInterval)
    {
        if (httpClient == null) throw new Exception();
        // get blockchain info
        var getChainTxStatsRequest = new JsonRPCRequest
        {
            Id = 1,
            Method = "getchaintxstats",
            Params = new List<object>(new object[] { ctxInterval })
        };
        var getChainTxStatsResponse = await httpClient.PostAsJsonAsync<JsonRPCRequest>("", getChainTxStatsRequest, options);
        var chainTxStats = await getChainTxStatsResponse.Content.ReadFromJsonAsync<GetChainTxStats>(options);

        if (chainTxStats == null || chainTxStats.Result == null) throw new Exception();
        return chainTxStats.Result;
    }

    private async Task<TxStatsEntry> GetTxStats(HttpClient? httpClient, int points, int offset)
    {
        var count = (int)(_chainInfoSingleton?.CurrentChainInfo?.Blocks ?? 1);
        if (offset > count)
            throw new Exception("offset > count");

        if (offset < 0)
            offset += count;

        var txStatsEntry = new TxStatsEntry
        {
            TxCounts = new List<TxStatsDataPoint>(),
            TxRates = new List<TxStatsDataPoint>(),
            Labels = new List<string>()
        };

        var chainTxStatsIntervals = new List<int>();
        for (var i = 0; i < points; i++)
        {
            var target = (int)Math.Max(10.0, (double)count - (double)offset - (double)i * (double)(count - offset) / (double)(points - 1.0d) - 1.0d);
            chainTxStatsIntervals.Add(target);
        }

        for (var i = chainTxStatsIntervals.Count() - 1; i >= 0; i--)
        {
            var res = await GetChainTxStats(httpClient, chainTxStatsIntervals[i]);

            if (res.window_tx_count == 0) continue;

            txStatsEntry.TxCounts.Add(new TxStatsDataPoint
            {
                X = count - res.window_block_count,
                Y = res.txcount - res.window_tx_count
            });
            txStatsEntry.TxRates.Add(new TxStatsDataPoint
            {
                X = count - res.window_block_count,
                Y = res.txrate
            });
            txStatsEntry.Labels.Add(i.ToString());
        }

        return txStatsEntry;
    }
}