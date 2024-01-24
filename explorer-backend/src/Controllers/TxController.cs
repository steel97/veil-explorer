using Microsoft.AspNetCore.Mvc;
using ExplorerBackend.Models.API;
using ExplorerBackend.Models.System;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Services.Data;
using ExplorerBackend.Models.Data;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class TxController(IBlocksDataService blocksDataService, ITransactionsDataService transactionsDataService, ITransactionDecoder transactionDecoder,
    IUtilityService utilityService, ChaininfoSingleton chaininfoSingleton)
    : ControllerBase
{
    private readonly IBlocksDataService _blocksDataService = blocksDataService;
    private readonly ITransactionsDataService _transactionsDataService = transactionsDataService;
    private readonly ITransactionDecoder _transactionDecoder = transactionDecoder;
    private readonly IUtilityService _utilityService = utilityService;
    private readonly ChaininfoSingleton _chaininfoSingleton = chaininfoSingleton;

    [HttpPost(Name = "GetTx")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(List<TxResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(TxRequest body, CancellationToken cancellationToken)
    {
        if (body.Hash == null) return Problem("hash can't be null", statusCode: 400);
        if (!_utilityService.VerifyHex(body.Hash)) return Problem("hash is not valid hex string", statusCode: 400);

        List<TxDecodeTarget> txTargets = [];

        var probeTx = _chaininfoSingleton.UnconfirmedTxs?.Where(tx => tx.txid == body.Hash).FirstOrDefault();

        TxResponse response = new();

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
            var tx = await _transactionsDataService.GetTransactionFullByIdAsync(body.Hash, cancellationToken);
            if (tx == null) return Problem("tx not found", statusCode: 400);

            Block? block;
            if (tx.block_height == 0)
            {
                block = await _blocksDataService.GetBlockAsync(tx.blockhash!, 1, cancellationToken);
                tx.block_height = block!.height;
            }
            else
                block = await _blocksDataService.GetBlockAsync(tx.block_height, 1, cancellationToken);

            txTargets.Add(new TxDecodeTarget
            {
                TxId = tx.txid_hex!,
                Data = tx.data
            });

            response.TxId = tx.txid_hex!;
            response.Confirmed = true;
            response.BlockHeight = tx.block_height;
            response.Timestamp = block?.time ?? 0;
            response.Version = tx.version;
            response.Size = tx.size;
            response.VSize = tx.vsize;
            response.Locktime = tx.locktime;
        }

        response.Transaction = (await _transactionDecoder.DecodeTransactionsAsync(txTargets, response.BlockHeight, cancellationToken))![0];

        return Ok(response);
    }
}
