using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ExplorerBackend.Models.API;
using ExplorerBackend.Models.System;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Services.Caching;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Produces("application/json")]
public class UnconfirmedTransactions : ControllerBase
{

    private readonly ILogger _logger;
    private readonly IOptions<APIConfig> _apiConfig;
    private readonly ChaininfoSingleton _chaininfoSingleton;
    private readonly ITransactionDecoder _transactionDecoder;
    private readonly IUtilityService _utilityService;

    public UnconfirmedTransactions(ILogger<UnconfirmedTransactions> logger, IOptions<APIConfig> apiConfig, ChaininfoSingleton chaininfoSingleton, ITransactionDecoder transactionDecoder, IUtilityService utilityService)
    {
        _logger = logger;
        _apiConfig = apiConfig;
        _chaininfoSingleton = chaininfoSingleton;
        _transactionDecoder = transactionDecoder;
        _utilityService = utilityService;
    }

    [HttpGet(Name = "UnconfirmedTransactions")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(UnconfirmedTxResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int offset, int count)
    {
        if (offset < 0)
            return Problem("offset should be higher or equal to zero", statusCode: 400);
        if (count < 1)
            return Problem("count should be more or equal to one", statusCode: 400);
        if (count > _apiConfig.Value.MaxTransactionsPullCount)
            return Problem($"count should be less or equal than {_apiConfig.Value.MaxBlocksPullCount}", statusCode: 400);

        var txncount = _chaininfoSingleton.UnconfirmedTxs?.Count() ?? 0;

        List<TransactionSimpleDecoded>? txs = null;

        if (_chaininfoSingleton.UnconfirmedTxs != null)
        {
            if (_chaininfoSingleton.UnconfirmedTxs.Count() > offset)
            {
                txs = new List<TransactionSimpleDecoded>();
                var rtxs = _chaininfoSingleton.UnconfirmedTxs.Skip(offset).Take(count);

                var txTargets = new List<TxDecodeTarget>();

                rtxs.ToList().ForEach(rtx => txTargets.Add(new TxDecodeTarget
                {
                    TxId = rtx.txid!,
                    Data = _utilityService.HexToByteArray(rtx.hex!)
                }));

                txs = await _transactionDecoder.DecodeTransactions(txTargets, (int)((_chaininfoSingleton.CurrentChainInfo?.Blocks ?? 0) + 1));

            }
        }

        return Ok(new UnconfirmedTxResponse
        {
            TxnCount = txncount,
            Transactions = txs
        });
    }
}
