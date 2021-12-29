using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ExplorerBackend.Models.API;
using ExplorerBackend.Models.System;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Persistence.Repositories;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class TxController : ControllerBase
{

    private readonly ILogger _logger;
    private readonly IOptions<APIConfig> _apiConfig;
    private readonly IBlocksRepository _blocksRepository;
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly ITransactionDecoder _transactionDecoder;
    private readonly ChaininfoSingleton _chaininfoSingleton;
    private readonly IUtilityService _utilityService;

    public TxController(ILogger<TxController> logger, IOptions<APIConfig> apiConfig, IBlocksRepository blocksRepository, ITransactionsRepository transactionsRepository, ITransactionDecoder transactionDecoder, ChaininfoSingleton chaininfoSingleton, IUtilityService utilityService)
    {
        _logger = logger;
        _apiConfig = apiConfig;
        _blocksRepository = blocksRepository;
        _transactionsRepository = transactionsRepository;
        _transactionDecoder = transactionDecoder;
        _chaininfoSingleton = chaininfoSingleton;
        _utilityService = utilityService;
    }

    [HttpPost(Name = "GetTx")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(List<TxResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(TxRequest body)
    {
        if (body.Hash == null) return Problem("hash can't be null", statusCode: 400);
        if (!_utilityService.VerifyHex(body.Hash)) return Problem("hash is not valid hex string", statusCode: 400);

        var txTargets = new List<TxDecodeTarget>();

        var probeTx = _chaininfoSingleton.UnconfirmedTxs?.Where(tx => tx.txid == body.Hash).FirstOrDefault();

        var response = new TxResponse();

        if (probeTx != null)
        {
            txTargets.Add(new TxDecodeTarget
            {
                TxId = probeTx.txid!,
                Data = _utilityService.HexToByteArray(probeTx.hex!)
            });

            response.TxId = probeTx.txid!;
            response.Confirmed = false;
            response.BlockHeight = (int)((_chaininfoSingleton.CurrentChainInfo?.Blocks ?? 0) + 1);
            response.Timestamp = probeTx.time;
            response.Version = probeTx.version;
            response.Size = probeTx.size;
            response.VSize = probeTx.vsize;
            response.Locktime = probeTx.locktime;
        }
        else
        {
            var dbtx = await _transactionsRepository.GetTransactionFullByIdAsync(body.Hash);
            if (dbtx == null) return Problem("can't find tx", statusCode: 400);

            var block = await _blocksRepository.GetBlockByHeightAsync(dbtx.block_height);

            txTargets.Add(new TxDecodeTarget
            {
                TxId = dbtx.txid_hex!,
                Data = dbtx.data
            });

            response.TxId = dbtx.txid_hex!;
            response.Confirmed = true;
            response.BlockHeight = dbtx.block_height;
            response.Timestamp = block?.time ?? 0;
            response.Version = dbtx.version;
            response.Size = dbtx.size;
            response.VSize = dbtx.vsize;
            response.Locktime = dbtx.locktime;
        }



        response.Transaction = (await _transactionDecoder.DecodeTransactions(txTargets, response.BlockHeight))![0];



        return Ok(response);
    }
}
