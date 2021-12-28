using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NBitcoin.DataEncoders;
using ExplorerBackend.Core;
using ExplorerBackend.Core.Node;
using ExplorerBackend.Models.API;
using ExplorerBackend.Models.Node.Response;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Queues;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Services.Caching;

namespace ExplorerBackend.Controllers;

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

                        response.Fetched = true;
                        validateRes = _nodeApiCacheSingleton.GetApiCache<ValidateAddress>($"validateaddress-{reqAddr}");
                    }
                }
                catch (TimeoutException)
                {

                }
            }

            if ((scanTxOutsetRes == null || body.ForceScanAmount) && (
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

            if (validateRes != null)
            {
                response.Fetched = true;
                if (validateRes.Result != null)
                {
                    response.IsValid = validateRes.Result.isvalid;
                    response.Address = validateRes.Result;

                    if (response.IsValid)
                    {
                        if (validateRes.Result.scriptPubKey != null && _utilityService.VerifyHex(validateRes.Result.scriptPubKey))
                        {
                            try
                            {
                                using var sha256 = SHA256.Create();
                                var ch = sha256.ComputeHash(_utilityService.HexToByteArray(validateRes.Result.scriptPubKey));
                                response.ScriptHash = new String(_utilityService.ToHex(ch).Reverse().ToArray());
                            }
                            catch
                            {

                            }
                        }
                        try
                        {
                            var b58enc = new Base58Encoder();
                            var b58Data = b58enc.DecodeData(reqAddr);
                            //var b58Data = Base58Encoding.Decode(reqAddr);
                            var version = b58Data[0];
                            var hash = b58Data.Skip(1).ToArray();

                            response.Version = version;
                            response.Hash = _utilityService.ToHex(hash);
                            response.Hash = response.Hash.Substring(0, response.Hash.Length - 8);
                        }
                        catch
                        {
                            try
                            {
                                var ver = (byte)0;
                                //var isP2PKH = (byte)0;
                                //var mainnet = false;

                                var bech = new Bech32Encoder(null);
                                var bechData = bech.Decode(reqAddr, out ver);

                                //var bechData = Bech32Converter.DecodeBech32(reqAddr, out ver, out isP2PKH, out mainnet);
                                response.Version = ver;
                            }
                            catch
                            {

                            }
                        }
                    }

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
