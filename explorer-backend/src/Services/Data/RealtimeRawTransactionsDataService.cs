using ExplorerBackend.Models.Node.Response;
using ExplorerBackend.Services.Core;

namespace ExplorerBackend.Services.Data;

public class RealtimeRawTransactionsDataService(NodeRequester nodeRequester, IUtilityService utilityService) : IRawTransactionsDataService
{
    private readonly static byte[] EMPTY_ARRAY = [];
    private readonly NodeRequester _nodeRequester = nodeRequester;
    private readonly IUtilityService _utilityService = utilityService;
    public async Task<byte[]?> GetTransactionByIdAsync(string txid, CancellationToken cancellationToken = default)
    {
        GetRawTransaction? tx = await _nodeRequester.GetRawTransaction(txid, cancellationToken);
        if (tx is null || tx.Result is null) return null;
        return _utilityService.HexToByteArray(tx.Result.hex!);
    }

    public async Task<Dictionary<string, byte[]>?> GetTransactionsByIdsAsync(List<string> txids, CancellationToken cancellationToken = default)
    {
        List<Task<GetRawTransaction?>> list = [];
        Dictionary<string, byte[]> result = [];

        foreach (var txid in txids)
        {
            list.Add(_nodeRequester.GetRawTransaction(txid, cancellationToken));
        }
        await Task.WhenAll(list);

        for (int i = 0; i < txids.Count; i++)
        {
            if (list[i] is null || list[i].Result is null || list[i].Result!.Result is null)
            {
                result.Add(txids[i], EMPTY_ARRAY);
                continue;
            }
            result.TryAdd(txids[i], _utilityService.HexToByteArray(list[i].Result!.Result!.hex!));
        }
        return result;
    }
}