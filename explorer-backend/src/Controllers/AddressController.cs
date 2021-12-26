using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using explorer_backend.Core;
using explorer_backend.Models.API;
using explorer_backend.Models.Node.Response;
using explorer_backend.Configs;
using explorer_backend.Services.Queues;
using explorer_backend.Services.Core;
using explorer_backend.Services.Caching;
using explorer_backend.Persistence.Repositories;

namespace explorer_backend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class AddressController : ControllerBase
{

    private readonly ILogger _logger;
    private readonly IOptions<APIConfig> _apiConfig;
    private readonly IUtilityService _utilityService;
    private readonly ValidateAddressBackgroundTaskQueue _validateAddressBackgroundTaskQueue;
    private readonly ScanTxOutsetBackgroundTaskQueue _scanTxOutsetBackgroundTaskQueue;
    private readonly INodeRequester _nodeRequester;
    private readonly NodeApiCacheSingleton _nodeApiCacheSingleton;

    public AddressController(ILogger<AddressController> logger, IOptions<APIConfig> apiConfig, IUtilityService utilityService,
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

    [HttpPost(Name = "Address")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(AddressRequest body, CancellationToken cancellationToken)
    {
        var response = new AddressResponse
        {
            Fetched = false,
            IsValid = false
        };

        if (_utilityService.VerifyAddress(body.Address))
        {
            var reqAddr = _utilityService.CleanupAddress(body.Address);

            var validateRes = _nodeApiCacheSingleton.GetApiCache<ValidateAddrees>($"validateaddress-{reqAddr}");
            var scanTxOutsetRes = _nodeApiCacheSingleton.GetApiCache<ScanTxOutset>($"scantxoutset-{reqAddr}");

            if (validateRes == null)
            {
                try
                {
                    // validate address
                    var validateAddressFlag = new AsyncFlag
                    {
                        State = false
                    };
                    await _validateAddressBackgroundTaskQueue.QueueBackgroundWorkItemAsync(async token =>
                    {
                        await _nodeRequester.ValidateAddressAndCacheAsync(reqAddr, token);
                        if (validateAddressFlag != null)
                            validateAddressFlag.State = true;
                    });
                    await AsyncUtils.WaitUntilAsync(cancellationToken, () => validateAddressFlag.State, _apiConfig.Value.ApiQueueSpinDelay, _apiConfig.Value.ApiQueueWaitTimeout);

                    response.Fetched = true;
                    validateRes = _nodeApiCacheSingleton.GetApiCache<ValidateAddrees>($"validateaddress-{reqAddr}");
                }
                catch (TimeoutException)
                {

                }
            }

            if (scanTxOutsetRes == null || body.ForceScanAmount)
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

            if (validateRes != null)
            {
                response.Fetched = true;
                if (validateRes.Result != null)
                {
                    response.IsValid = validateRes.Result.isvalid;
                    response.Address = validateRes.Result;

                    // copy amount if it exists

                    if (scanTxOutsetRes != null && scanTxOutsetRes.Result != null)
                    {
                        response.AmountFetched = true;
                        response.Amount = scanTxOutsetRes.Result.total_amount;
                    }
                    else
                    {
                        response.AmountFetched = false;
                        response.Amount = 0;
                    }
                }
            }
        }

        return Ok(response);
    }
}
