using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ExplorerBackend.Core;
using ExplorerBackend.VeilStructs;
using ExplorerBackend.Models.System;
using ExplorerBackend.Models.Node.Response;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Queues;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Services.Caching;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class GetAddressBalanceController : ControllerBase
{
    private readonly IOptions<APIConfig> _apiConfig;
    private readonly IUtilityService _utilityService;
    private readonly ScanTxOutsetBackgroundTaskQueue _scanTxOutsetBackgroundTaskQueue;
    private readonly NodeApiCacheSingleton _nodeApiCacheSingleton;

    public GetAddressBalanceController(IOptions<APIConfig> apiConfig, IUtilityService utilityService,
        ScanTxOutsetBackgroundTaskQueue scanTxOutsetBackgroundTaskQueue, NodeApiCacheSingleton nodeApiCacheSingleton)
    {
        _apiConfig = apiConfig;
        _utilityService = utilityService;
        _scanTxOutsetBackgroundTaskQueue = scanTxOutsetBackgroundTaskQueue;
        _nodeApiCacheSingleton = nodeApiCacheSingleton;
    }

    [HttpGet("{address}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(double?), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(string address, CancellationToken cancellationToken)
    {
        double? response = null;

        if (_utilityService.VerifyAddress(address))
        {
            var reqAddr = _utilityService.CleanupAddress(address);

            var validateRes = VeilAddress.ValidateAddress(address);// decode internal
            var scanTxOutsetRes = _nodeApiCacheSingleton.GetApiCache<ScanTxOutset>($"scantxoutset-{reqAddr}");



            if (
              validateRes == null ||
              !validateRes.isvalid ||
              (validateRes.isstealthaddress ?? false)
            )
                return StatusCode(400, response);


            if (scanTxOutsetRes == null && (
                 validateRes != null &&
                 validateRes.isvalid &&
                 !(validateRes.isstealthaddress ?? false)
             ) && !_nodeApiCacheSingleton.IsInQueue($"scantxoutset-{reqAddr}"))
            {
                try
                {
                    // try get balance
                    var scanTxOutsetFlag = new AsyncFlag
                    {
                        State = false
                    };
                    await _scanTxOutsetBackgroundTaskQueue.QueueBackgroundWorkItemAsync(async (input, token) =>
                    {
                        var bridge = (ScanTxOutsetBridge)input;
                        if (bridge == null || bridge.NodeApiCacheLink == null || bridge.NodeRequesterLink == null) return;

                        if (await bridge.NodeApiCacheLink.PutInQueueAsync($"scantxoutset-{reqAddr}"))
                        {
                            await bridge.NodeRequesterLink.ScanTxOutsetAndCacheAsync(reqAddr, token);
                            await bridge.NodeApiCacheLink.RemoveFromQueueAsync($"scantxoutset-{reqAddr}");
                        }

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
            {
                response = scanTxOutsetRes.Result.total_amount;
                return Ok(response);
            }
        }
        else
            return StatusCode(400, response);

        return StatusCode(202, response);
    }
}