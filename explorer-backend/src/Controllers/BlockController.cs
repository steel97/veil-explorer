using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ExplorerBackend.Models.API;
using ExplorerBackend.Models.System;
using ExplorerBackend.Configs;
using ExplorerBackend.Models.Data;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Persistence.Repositories;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class BlockController : ControllerBase
{

    private readonly ILogger _logger;
    private readonly IOptions<APIConfig> _apiConfig;
    private readonly IBlocksRepository _blocksRepository;
    private readonly ITransactionsRepository _transactionsRepository;
    private readonly ITransactionDecoder _transactionDecoder;
    private readonly IUtilityService _utilityService;

    public BlockController(ILogger<BlockController> logger, IOptions<APIConfig> apiConfig, IBlocksRepository blocksRepository, ITransactionsRepository transactionsRepository, ITransactionDecoder transactionDecoder, IUtilityService utilityService)
    {
        _logger = logger;
        _apiConfig = apiConfig;
        _blocksRepository = blocksRepository;
        _transactionsRepository = transactionsRepository;
        _transactionDecoder = transactionDecoder;
        _utilityService = utilityService;
    }

    [HttpPost(Name = "GetBlock")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(List<BlockResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(BlockRequest body)
    {
        if (body.Offset < 0)
            return Problem("offset should be higher or equal to zero", statusCode: 400);
        if (body.Count < 1)
            return Problem("count should be more or equal to one", statusCode: 400);
        if (body.Count > _apiConfig.Value.MaxTransactionsPullCount)
            return Problem($"count should be less or equal than {_apiConfig.Value.MaxBlocksPullCount}", statusCode: 400);

        var response = new BlockResponse
        {
            Found = false
        };

        Block? block = null;
        if (body.Hash != null && _utilityService.VerifyHex(body.Hash))
            block = await _blocksRepository.GetBlockByHashAsync(body.Hash);
        else if (body.Height != null)
            block = await _blocksRepository.GetBlockByHeightAsync(body.Height.Value);
        else
            return Problem($"count should be less or equal than {_apiConfig.Value.MaxBlocksPullCount}", statusCode: 400);

        if (block != null)
        {
            var nextBlockHash = await _blocksRepository.ProbeHashByHeight(block.height + 1);
            var prevBlockHash = await _blocksRepository.ProbeHashByHeight(block.height - 1);

            response.Found = true;
            response.Block = block;
            response.TxnCount = block.txnCount;

            var verHex = BitConverter.GetBytes(block.version);
            verHex = verHex.Reverse().ToArray();
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


            var rtxs = await _transactionsRepository.GetTransactionsForBlockAsync(block.height, body.Offset, body.Count);
            if (rtxs != null)
            {
                var txTargets = new List<TxDecodeTarget>();

                rtxs.ForEach(rtx => txTargets.Add(new TxDecodeTarget
                {
                    TxId = rtx.txid_hex!,
                    Data = rtx.data
                }));

                response.Transactions = await _transactionDecoder.DecodeTransactions(txTargets, block.height);
            }
        }

        return Ok(response);
    }
}
