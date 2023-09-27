using ExplorerBackend.Persistence.Repositories;

namespace ExplorerBackend.Services.Data;

public class DbRawTransactionsDataService : IRawTransactionsDataService
{
    private readonly IRawTxsRepository _rawTxsRepository;
    public DbRawTransactionsDataService(IRawTxsRepository rawTxsRepository) => _rawTxsRepository = rawTxsRepository;
    public Task<byte[]?> GetTransactionByIdAsync(string txid, CancellationToken cancellationToken = default) =>
        _rawTxsRepository.GetTransactionByIdAsync(txid, cancellationToken);

    public Task<Dictionary<string, byte[]>?> GetTransactionsByIdsAsync(List<string> txids, CancellationToken cancellationToken = default) =>
        _rawTxsRepository.GetTransactionsByIdsAsync(txids, cancellationToken);
}