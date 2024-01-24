using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ExplorerBackend.Models.API;
using ExplorerBackend.Models.System;
using ExplorerBackend.Configs;
using ExplorerBackend.Models.Data;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Services.Data;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class BlockController(IOptions<APIConfig> apiConfig, IBlocksDataService blocksDataService, ITransactionsDataService transactionsDataService, ITransactionDecoder transactionDecoder, IUtilityService utilityService) : ControllerBase
{
    private readonly IOptions<APIConfig> _apiConfig = apiConfig;
    private readonly IBlocksDataService _blocksDataService = blocksDataService;
    private readonly ITransactionsDataService _transactionsDataService = transactionsDataService;
    private readonly ITransactionDecoder _transactionDecoder = transactionDecoder;
    private readonly IUtilityService _utilityService = utilityService;

    [HttpPost(Name = "GetBlock")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(List<BlockResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(BlockRequest body, CancellationToken cancellationToken)
    {
        if (body.Offset < 0)
            return Problem("offset should be higher or equal to zero", statusCode: 400);
        if (body.Count > _apiConfig.Value.MaxTransactionsPullCount || body.Count < 1)
            return Problem($"count should be between 1 and {_apiConfig.Value.MaxBlocksPullCount}", statusCode: 400);

        var response = new BlockResponse
        {
            Found = false
        };

        Block? block;
        if (body.Height != null)
            block = await _blocksDataService.GetBlockAsync(body.Height.Value, 2, cancellationToken);
        else if (body.Hash != null && body.Hash.Length == 64 && _utilityService.VerifyHex(body.Hash))
            block = await _blocksDataService.GetBlockAsync(body.Hash, 2, cancellationToken);
        else
            return Problem($"a problem has occured, try again", statusCode: 400);

        if (block != null)
        {
            Task<string?>? nextBlockHash = null;
            Task<string?>? prevBlockHash = null;
            try
            {
                nextBlockHash = _blocksDataService.ProbeHashByHeightAsync(block.height + 1, cancellationToken);
                prevBlockHash = _blocksDataService.ProbeHashByHeightAsync(block.height - 1, cancellationToken);

                await Task.WhenAll(nextBlockHash, prevBlockHash);
            }
            catch { }

            response.Found = true;
            response.Block = block;
            response.TxnCount = block.txnCount;

            var verHex = BitConverter.GetBytes(block.version).Reverse().ToArray();
            response.VersionHex = _utilityService.ToHex(verHex);

            if (nextBlockHash != null && nextBlockHash.Result != null)
                response.NextBlock = new BlockBasicData
                {
                    Hash = nextBlockHash.Result,
                    Height = block.height + 1
                };

            if (prevBlockHash != null && prevBlockHash.Result != null)
                response.PrevBlock = new BlockBasicData
                {
                    Hash = prevBlockHash.Result,
                    Height = block.height - 1
                };

            List<TransactionExtended>? rtxs;

            if (block.tx is null)
                rtxs = await _transactionsDataService.GetTransactionsForBlockAsync(block.height, body.Offset, body.Count, false, cancellationToken);
            else
                rtxs = _transactionsDataService.ToTransactionExtended(block.tx, block.height);

            if (rtxs != null)
            {
                var txTargets = new List<TxDecodeTarget>();

                rtxs.ForEach(rtx => txTargets.Add(new TxDecodeTarget
                {
                    TxId = rtx.txid_hex!,
                    Data = rtx.data
                }));

                response.Transactions = await _transactionDecoder.DecodeTransactionsAsync(txTargets, block.height, cancellationToken);
            }
        }

        return Ok(response);
    }
}
