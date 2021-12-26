using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using explorer_backend.Core;
using explorer_backend.Models.Node.Response;
using explorer_backend.Configs;
using explorer_backend.Services.Queues;
using explorer_backend.Services.Core;
using explorer_backend.Services.Caching;

namespace explorer_backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class GetAddressBalanceController : ControllerBase
{

    private readonly ILogger _logger;
    private readonly IOptions<APIConfig> _apiConfig;
    private readonly IUtilityService _utilityService;
    private readonly ScanTxOutsetBackgroundTaskQueue _scanTxOutsetBackgroundTaskQueue;
    private readonly INodeRequester _nodeRequester;
    private readonly NodeApiCacheSingleton _nodeApiCacheSingleton;

    public GetAddressBalanceController(ILogger<GetAddressBalanceController> logger, IOptions<APIConfig> apiConfig, IUtilityService utilityService,
        ScanTxOutsetBackgroundTaskQueue scanTxOutsetBackgroundTaskQueue,
        INodeRequester nodeRequester, NodeApiCacheSingleton nodeApiCacheSingleton)
    {
        _logger = logger;
        _apiConfig = apiConfig;
        _utilityService = utilityService;
        _scanTxOutsetBackgroundTaskQueue = scanTxOutsetBackgroundTaskQueue;
        _nodeRequester = nodeRequester;
        _nodeApiCacheSingleton = nodeApiCacheSingleton;
    }

    [HttpGet("{address}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(double?), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(string address, CancellationToken cancellationToken)
    {
        double? response = null;

        if (_utilityService.VerifyAddress(address))
        {
            var reqAddr = _utilityService.CleanupAddress(address);

            var scanTxOutsetRes = _nodeApiCacheSingleton.GetApiCache<ScanTxOutset>($"scantxoutset-{reqAddr}");

            if (scanTxOutsetRes == null)
            {
                try
                {
                    // try get balance
                    var scanTxOutsetFlag = new AsyncFlag
                    {
                        State = false
                    };
                    await _scanTxOutsetBackgroundTaskQueue.QueueBackgroundWorkItemAsync(async token =>
                    {
                        await _nodeRequester.ScanTxOutsetAndCacheAsync(reqAddr, token);
                        if (scanTxOutsetFlag != null)
                            scanTxOutsetFlag.State = true;
                    });
                    await AsyncUtils.WaitUntilAsync(cancellationToken, () => scanTxOutsetFlag.State, _apiConfig.Value.ApiQueueSpinDelay, _apiConfig.Value.ApiQueueWaitTimeout);
                    scanTxOutsetRes = _nodeApiCacheSingleton.GetApiCache<ScanTxOutset>($"scantxoutset-{reqAddr}");
                }
                catch (TimeoutException)
                {

                }
            }

            if (scanTxOutsetRes != null && scanTxOutsetRes.Result != null)
                response = scanTxOutsetRes.Result.total_amount;
        }

        return Ok(response);
    }
}