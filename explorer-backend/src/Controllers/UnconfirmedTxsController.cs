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
public class UnconfirmedTransactions(IOptions<APIConfig> apiConfig, ChaininfoSingleton chaininfoSingleton, ITransactionDecoder transactionDecoder, IUtilityService utilityService)
    : ControllerBase
{
    private readonly IOptions<APIConfig> _apiConfig = apiConfig;
    private readonly ChaininfoSingleton _chaininfoSingleton = chaininfoSingleton;
    private readonly ITransactionDecoder _transactionDecoder = transactionDecoder;
    private readonly IUtilityService _utilityService = utilityService;

    [HttpGet(Name = "UnconfirmedTransactions")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(UnconfirmedTxResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(int offset, int count, CancellationToken cancellationToken)
    {
        if (offset < 0)
            return Problem("offset should be higher or equal to zero", statusCode: 400);
        if (count < 1 || count > _apiConfig.Value.MaxTransactionsPullCount)
            return Problem($"count should be between 1 and {_apiConfig.Value.MaxBlocksPullCount}", statusCode: 400);

        var txncount = _chaininfoSingleton.UnconfirmedTxs?.Count ?? 0;

        List<TransactionSimpleDecoded>? txs = null;

        if (_chaininfoSingleton.UnconfirmedTxs != null)
        {
            if (_chaininfoSingleton.UnconfirmedTxs.Count > offset)
            {
                txs = [];
                var rtxs = _chaininfoSingleton.UnconfirmedTxs.Skip(offset).Take(count);

                List<TxDecodeTarget> txTargets = [];

                rtxs.ToList().ForEach(rtx => txTargets.Add(new TxDecodeTarget
                {
                    TxId = rtx.txid!,
                    Data = _utilityService.HexToByteArray(rtx.hex!)
                }));

                txs = await _transactionDecoder.DecodeTransactionsAsync(txTargets, (int)((_chaininfoSingleton.CurrentChainInfo?.Blocks ?? 0) + 1), cancellationToken);
            }
        }

        return Ok(new UnconfirmedTxResponse
        {
            TxnCount = txncount,
            Transactions = txs
        });
    }
}
