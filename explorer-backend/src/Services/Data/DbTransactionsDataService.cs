using ExplorerBackend.Models.Data;
using ExplorerBackend.Persistence.Repositories;

namespace ExplorerBackend.Services.Data;

public class DbTransactionsDataService : ITransactionsDataService
{
    private readonly ITransactionsRepository _transactionsRepository;
    public DbTransactionsDataService(ITransactionsRepository transactionsRepository) => _transactionsRepository = transactionsRepository;
    public Task<Transaction?> GetTransactionByIdAsync(string txid, CancellationToken cancellationToken = default) =>
        _transactionsRepository.GetTransactionByIdAsync(txid, cancellationToken);

    public Task<TransactionExtended?> GetTransactionFullByIdAsync(string txid, CancellationToken cancellationToken = default) =>
        _transactionsRepository.GetTransactionFullByIdAsync(txid, cancellationToken);

    public Task<List<TransactionExtended>?> GetTransactionsForBlockAsync(int blockHeight, int offset, int count, bool fetchAll, CancellationToken cancellationToken = default) =>
        _transactionsRepository.GetTransactionsForBlockAsync(blockHeight, offset, count, fetchAll, cancellationToken);

    public Task<string?> ProbeTransactionByHashAsync(string txid, CancellationToken cancellationToken = default) => 
        _transactionsRepository.ProbeTransactionByHashAsync(txid, cancellationToken);
}