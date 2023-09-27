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
public class BlockController : ControllerBase
{
    private readonly IOptions<APIConfig> _apiConfig;
    private readonly IBlocksDataService _blocksDataService; // switched to the new layer
    private readonly ITransactionsDataService _transactionsDataService; // switched to the new layer
    private readonly ITransactionDecoder _transactionDecoder;
    private readonly IUtilityService _utilityService;

    public BlockController(IOptions<APIConfig> apiConfig, IBlocksDataService blocksDataService, ITransactionsDataService transactionsDataService, ITransactionDecoder transactionDecoder, IUtilityService utilityService)
    {
        _apiConfig = apiConfig;
        _blocksDataService = blocksDataService;
        _transactionsDataService = transactionsDataService;
        _transactionDecoder = transactionDecoder;
        _utilityService = utilityService;
    }

    [HttpPost(Name = "GetBlock")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(List<BlockResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(BlockRequest body, CancellationToken cancellationToken)
    {
        if (body.Offset < 0)
            return Problem("offset should be higher or equal to zero", statusCode: 400);
        if (body.Count > _apiConfig.Value.MaxTransactionsPullCount || body.Count < 1)
            return Problem($"count should be between 1 and {_apiConfig.Value.MaxBlocksPullCount} ", statusCode: 400);

        var response = new BlockResponse
        {
            Found = false
        };

        Block? block = null;
        if (body.Hash != null && body.Hash.Length == 64 && _utilityService.VerifyHex(body.Hash))
            block = await _blocksDataService.GetBlockByHashAsync(body.Hash, cancellationToken);        
        else if (body.Height != null)
            block = await _blocksDataService.GetBlockByHeightAsync(body.Height.Value, cancellationToken);
        else
            return Problem($"a problem has occured, try again", statusCode: 400);

        if (block != null)
        {
            var nextBlockHash = await _blocksDataService.ProbeHashByHeightAsync(block.height + 1, cancellationToken);
            var prevBlockHash = await _blocksDataService.ProbeHashByHeightAsync(block.height - 1, cancellationToken);

            response.Found = true;
            response.Block = block;
            response.TxnCount = block.txnCount;

            var verHex = BitConverter.GetBytes(block.version).Reverse().ToArray();
            response.VersionHex = _utilityService.ToHex(verHex);

            if (nextBlockHash != null)
                response.NextBlock = new BlockBasicData
                {
                    Hash = nextBlockHash,
                    Height = block.height + 1
                };

            if (prevBlockHash != null)
                response.PrevBlock = new BlockBasicData
                {
                    Hash = prevBlockHash,
                    Height = block.height - 1
                };


            var rtxs = await _transactionsDataService.GetTransactionsForBlockAsync(block.height, body.Offset, body.Count, false, cancellationToken);
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
