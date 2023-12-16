using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NBitcoin.DataEncoders;
using ExplorerBackend.Core;
using ExplorerBackend.VeilStructs;
using ExplorerBackend.Models.API;
using ExplorerBackend.Models.System;
using ExplorerBackend.Models.Node.Response;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Queues;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Services.Caching;
using System.Buffers;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class AddressController : ControllerBase
{
    private readonly IOptions<APIConfig> _apiConfig;
    private readonly IUtilityService _utilityService;
    private readonly ScanTxOutsetBackgroundTaskQueue _scanTxOutsetBackgroundTaskQueue;
    private readonly NodeApiCacheSingleton _nodeApiCacheSingleton;

    public AddressController(IOptions<APIConfig> apiConfig, IUtilityService utilityService,
        ScanTxOutsetBackgroundTaskQueue scanTxOutsetBackgroundTaskQueue, NodeApiCacheSingleton nodeApiCacheSingleton)
    {
        _apiConfig = apiConfig;
        _utilityService = utilityService;
        _scanTxOutsetBackgroundTaskQueue = scanTxOutsetBackgroundTaskQueue;
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

        if (body.Address != null && _utilityService.VerifyAddress(body.Address))
        {
            string reqAddr = _utilityService.CleanupAddress(body.Address);

            var validateRes = VeilAddress.ValidateAddress(body.Address);// decode internal
            var scanTxOutsetRes = _nodeApiCacheSingleton.GetApiCache<ScanTxOutset>($"scantxoutset-{reqAddr}");


            if ((scanTxOutsetRes == null || body.ForceScanAmount) && (
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

            if (validateRes != null)
            {
                response.Fetched = true;

                response.IsValid = validateRes.isvalid;
                response.Address = validateRes;

                if (response.IsValid)
                {
                    if (validateRes.scriptPubKey != null && _utilityService.VerifyHex(validateRes.scriptPubKey))
                    {
                        byte[] ch = ArrayPool<byte>.Shared.Rent(validateRes.scriptPubKey.Length / 2);
                        
                        ch = SHA256.HashData(_utilityService.HexToByteArray(validateRes.scriptPubKey));

                        response.ScriptHash = new string(_utilityService.ToHex(ch).Reverse().ToArray());                        

                        ArrayPool<byte>.Shared.Return(ch);   
                    }

                    byte[] b58Data = [];
                    bool isDecoded = true;
                   
                    var b58enc = new Base58Encoder();
                    try
                    {
                        b58Data = b58enc.DecodeData(reqAddr);
                    }
                    catch
                    {
                        isDecoded = false;
                    }
                    //var b58Data = Base58Encoding.Decode(reqAddr);
                    if(isDecoded)
                    {
                        byte[] hash = ArrayPool<byte>.Shared.Rent(b58Data.Length - 1);
                        hash = b58Data.Skip(1).ToArray();

                        response.Version = b58Data[0];
                        response.Hash = _utilityService.ToHex(hash);
                        response.Hash = response.Hash[0..^8];
                        ArrayPool<byte>.Shared.Return(hash);
                    }
                    else
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

        return Ok(response);
    }
}
