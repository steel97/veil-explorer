using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ExplorerBackend.Core;
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

    private readonly ILogger _logger;
    private readonly IOptions<APIConfig> _apiConfig;
    private readonly IUtilityService _utilityService;
    private readonly ValidateAddressBackgroundTaskQueue _validateAddressBackgroundTaskQueue;
    private readonly ScanTxOutsetBackgroundTaskQueue _scanTxOutsetBackgroundTaskQueue;
    private readonly INodeRequester _nodeRequester;
    private readonly NodeApiCacheSingleton _nodeApiCacheSingleton;

    public GetAddressBalanceController(ILogger<GetAddressBalanceController> logger, IOptions<APIConfig> apiConfig, IUtilityService utilityService,
        ValidateAddressBackgroundTaskQueue validateAddressBackgroundTaskQueue, ScanTxOutsetBackgroundTaskQueue scanTxOutsetBackgroundTaskQueue,
        INodeRequester nodeRequester, NodeApiCacheSingleton nodeApiCacheSingleton)
    {
        _logger = logger;
        _apiConfig = apiConfig;
        _utilityService = utilityService;
        _validateAddressBackgroundTaskQueue = validateAddressBackgroundTaskQueue;
        _scanTxOutsetBackgroundTaskQueue = scanTxOutsetBackgroundTaskQueue;
        _nodeRequester = nodeRequester;
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

            var validateRes = _nodeApiCacheSingleton.GetApiCache<ValidateAddress>($"validateaddress-{reqAddr}");
            var scanTxOutsetRes = _nodeApiCacheSingleton.GetApiCache<ScanTxOutset>($"scantxoutset-{reqAddr}");

            if (validateRes == null && !_nodeApiCacheSingleton.IsInQueue($"validateaddress-{reqAddr}"))
            {
                try
                {
                    if (await _nodeApiCacheSingleton.PutInQueueAsync($"validateaddress-{reqAddr}"))
                    {
                        // validate address
                        var validateAddressFlag = new AsyncFlag
                        {
                            State = false
                        };
                        await _validateAddressBackgroundTaskQueue.QueueBackgroundWorkItemAsync(async token =>
                        {
                            try
                            {
                                await _nodeRequester.ValidateAddressAndCacheAsync(reqAddr, token);
                            }
                            catch
                            {
                            }

                            await _nodeApiCacheSingleton.RemoveFromQueueAsync($"validateaddress-{reqAddr}");

                            if (validateAddressFlag != null)
                                validateAddressFlag.State = true;
                        });
                        await AsyncUtils.WaitUntilAsync(cancellationToken, () => validateAddressFlag.State, _apiConfig.Value.ApiQueueSpinDelay, _apiConfig.Value.ApiQueueWaitTimeout);

                        validateRes = _nodeApiCacheSingleton.GetApiCache<ValidateAddress>($"validateaddress-{reqAddr}");
                    }
                }
                catch (TimeoutException)
                {

                }
            }

            if (
              validateRes == null ||
              validateRes.Result == null ||
              !validateRes.Result.isvalid ||
              (validateRes.Result.isstealthaddress ?? false)
            )
                return StatusCode(400, response);


            if (scanTxOutsetRes == null && (
                 validateRes != null &&
                 validateRes.Result != null &&
                 validateRes.Result.isvalid &&
                 !(validateRes.Result.isstealthaddress ?? false)
             ) && !_nodeApiCacheSingleton.IsInQueue($"scantxoutset-{reqAddr}"))
            {
                try
                {
                    if (await _nodeApiCacheSingleton.PutInQueueAsync($"scantxoutset-{reqAddr}"))
                    {
                        // try get balance
                        var scanTxOutsetFlag = new AsyncFlag
                        {
                            State = false
                        };
                        await _scanTxOutsetBackgroundTaskQueue.QueueBackgroundWorkItemAsync(async token =>
                        {
                            try
                            {
                                await _nodeRequester.ScanTxOutsetAndCacheAsync(reqAddr, token);
                            }
                            catch
                            {

                            }

                            await _nodeApiCacheSingleton.RemoveFromQueueAsync($"scantxoutset-{reqAddr}");

                            if (scanTxOutsetFlag != null)
                                scanTxOutsetFlag.State = true;
                        });
                        await AsyncUtils.WaitUntilAsync(cancellationToken, () => scanTxOutsetFlag.State, _apiConfig.Value.ApiQueueSpinDelay, _apiConfig.Value.ApiQueueWaitTimeout);
                        scanTxOutsetRes = _nodeApiCacheSingleton.GetApiCache<ScanTxOutset>($"scantxoutset-{reqAddr}");
                    }
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