using ExplorerBackend.Models.Data;
using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services.Data;

public interface ITransactionsDataService
{
    public Task<Transaction?> GetTransactionByIdAsync(string txid, CancellationToken cancellationToken = default);
    public Task<TransactionExtended?> GetTransactionFullByIdAsync(string txid, CancellationToken cancellationToken = default);
    public Task<List<TransactionExtended>?> GetTransactionsForBlockAsync(int blockHeight, int offset, int count, bool fetchAll, CancellationToken cancellationToken = default);
    public Task<string?> ProbeTransactionByHashAsync(string txid, CancellationToken cancellationToken = default);
    public List<TransactionExtended>? ToTransactionExtended(List<GetRawTransactionResult> tx, int height);
}