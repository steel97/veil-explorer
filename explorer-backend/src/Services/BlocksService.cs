using System.Text.Json;
using System.Transactions;
using System.Net.Http.Headers;
using ExplorerBackend.Configs;
using ExplorerBackend.Models.Data;
using ExplorerBackend.Models.Node;
using ExplorerBackend.Models.Node.Response;
using ExplorerBackend.Persistence.Repositories;
using Microsoft.Extensions.Options;
using System.Text;
using ExplorerBackend.Services.Core;

namespace ExplorerBackend.Services;

public class BlocksService : IBlocksService
{
    private Uri? _uri;
    private AuthenticationHeaderValue? _authHeader;
    private int _usernameHash;
    private int _passHash;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly NodeRequester _nodeRequester;

    public BlocksService(ILogger<IBlocksService> logger, IServiceProvider serviceProvider, IOptionsMonitor<ExplorerConfig> explorerConfig,
        IHttpClientFactory httpClientFactory, NodeRequester nodeRequester)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _explorerConfig = explorerConfig;
        _httpClientFactory = httpClientFactory;
        _nodeRequester = nodeRequester;
        ConfigSetup();
    }

    public Block RPCBlockToDb(GetBlockResult block) => new()
    {
        anon_index = block.Anon_index,
        bits_hex = block.Bits,
        chainwork_hex = block.Chainwork,
        difficulty = block.Difficulty,
        epoch_number = block.epoch_number,
        hash_hex = block.Hash,
        height = block.Height,
        mediantime = block.Mediantime,
        merkleroot_hex = block.Merkleroot,
        mixhash_hex = block.Mixhash,
        nonce = block.Nonce,
        nonce64 = block.Nonce64,
        prog_header_hash_hex = block.prog_header_hash,
        prog_header_hex = block.prog_header,
        progpowmixhash_hex = block.progpowmixhash,
        progproofofworkhash_hex = block.progproofofworkhash,
        proofofstakehash_hex = block.Proofofstakehash,
        proofofworkhash_hex = block.proofofworkhash,
        randomxproofofworkhash_hex = block.randomxproofofworkhash,
        sha256dproofofworkhash_hex = block.sha256dproofofworkhash,
        size = block.Size,
        strippedsize = block.Strippedsize,
        time = block.Time,
        proof_type = block.Proof_type switch
        {
            "Proof-of-Work (X16RT)" => BlockType.POW_X16RT,
            "Proof-of-work (ProgPow)" => BlockType.POW_ProgPow,
            "Proof-of-work (RandomX)" => BlockType.POW_RandomX,
            "Proof-of-work (Sha256D)" => BlockType.POW_Sha256D,
            "Proof-of-Stake" => BlockType.POS,
            _ => BlockType.UNKNOWN
        },
        veil_data_hash_hex = block.Veil_data_hash,
        version = block.Version,
        weight = block.Weight,
        synced = false
    };

    public async Task<bool> UpdateDbBlockAsync(int height, string validBlockHash, CancellationToken cancellationToken)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        using var scope = _serviceProvider.CreateAsyncScope();

       if(_passHash !=_explorerConfig.CurrentValue.Node!.Password!.GetHashCode() || _usernameHash !=_explorerConfig.CurrentValue.Node!.Username!.GetHashCode())        
            ConfigSetup();
                    
        httpClient.BaseAddress = _uri;
        httpClient.DefaultRequestHeaders.Authorization = _authHeader;
        
        var validBlock = await _nodeRequester.GetBlock(validBlockHash, httpClient, cancellationToken);

        if (validBlock == null || validBlock.Result == null)
        {
            _logger.LogInformation("Can't pull block (orphan fix) for {blockHeight}", height);
            return false;
        }

        var transactionsRepository = scope.ServiceProvider.GetRequiredService<ITransactionsRepository>();
        var blocksRepository = scope.ServiceProvider.GetRequiredService<IBlocksRepository>();

        await transactionsRepository.RemoveTransactionsForBlockAsync(height, cancellationToken);
        await blocksRepository.UpdateBlockAsync(height, RPCBlockToDb(validBlock.Result), cancellationToken);
        return !await InsertTransactionsAsync(height, validBlock.Result.Txs!, cancellationToken);
    }

    public async Task<bool> InsertTransactionsAsync(int blockId, List<string>? txIds, CancellationToken cancellationToken)
    {
        using var httpClient = _httpClientFactory.CreateClient();
        using var scope = _serviceProvider.CreateAsyncScope();

       if(_passHash !=_explorerConfig.CurrentValue.Node!.Password!.GetHashCode() || _usernameHash !=_explorerConfig.CurrentValue.Node!.Username!.GetHashCode())        
            ConfigSetup();
                    
        httpClient.BaseAddress = _uri;
        httpClient.DefaultRequestHeaders.Authorization = _authHeader;

        var transactionsRepository = scope.ServiceProvider.GetRequiredService<ITransactionsRepository>();
        var rawTxsRepository = scope.ServiceProvider.GetRequiredService<IRawTxsRepository>();

        var pulledTxs = new List<GetRawTransactionResult>();
        if (txIds != null)
            foreach (var txId in txIds)
            {
                var tx = await _nodeRequester.GetRawTransaction(txId, httpClient, cancellationToken);

                if (tx == null || tx.Result == null)
                {
                    _logger.LogInformation("Can't pull transaction {txId} for block #{blockNumber}", txId, blockId);
                    return false; // was break
                }

                pulledTxs.Add(tx.Result);
            }

        var isTxFailed = false;
        foreach (var tx in pulledTxs)
        {
            ArgumentNullException.ThrowIfNull(tx);
            ArgumentNullException.ThrowIfNull(tx.txid);
            ArgumentNullException.ThrowIfNull(tx.hex);

            var targetTx = await transactionsRepository.GetTransactionByIdAsync(tx.txid, cancellationToken);
            if (targetTx != null) continue;

            try
            {
                using var txscope = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromSeconds(_explorerConfig.CurrentValue.TxScopeTimeout), TransactionScopeAsyncFlowOption.Enabled);
                targetTx = new Models.Data.Transaction
                {
                    txid_hex = tx.txid,
                    hash_hex = tx.hash,
                    version = tx.version,
                    size = tx.size,
                    vsize = tx.vsize,
                    weight = tx.weight,
                    locktime = tx.locktime,
                    block_height = blockId
                };

                var txCompleted = await transactionsRepository.InsertTransactionAsync(targetTx, cancellationToken);
                var txRawCompleted = await rawTxsRepository.InsertTransactionAsync(tx.txid, tx.hex, cancellationToken);

                if (txCompleted && txRawCompleted)
                    txscope.Complete();
                else
                    isTxFailed = true;
            }

            catch (TransactionAbortedException txex)
            {
                _logger.LogError(txex, "Can't save transaction {txId} (insert) for block #{blockNumber}", tx.txid, blockId);
                isTxFailed = true;
                break;
            }
        }

        return isTxFailed;
    }

    // if something doesn't work correctly, then use the method above + check GetBlock model
    public async Task<bool> InsertTransactionsAsync(int height, List<GetRawTransactionResult> txs, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateAsyncScope();

        var transactionsRepository = scope.ServiceProvider.GetRequiredService<ITransactionsRepository>();
        var rawTxsRepository = scope.ServiceProvider.GetRequiredService<IRawTxsRepository>();

        var isTxFailed = false;
        foreach (var tx in txs)
        {
            ArgumentNullException.ThrowIfNull(tx);
            ArgumentNullException.ThrowIfNull(tx.txid);
            ArgumentNullException.ThrowIfNull(tx.hex);

            var targetTx = await transactionsRepository.GetTransactionByIdAsync(tx.txid, cancellationToken);
            if (targetTx != null) continue;

            try
            {
                using var txscope = new TransactionScope(TransactionScopeOption.Required, TimeSpan.FromSeconds(_explorerConfig.CurrentValue.TxScopeTimeout), TransactionScopeAsyncFlowOption.Enabled);
                targetTx = new Models.Data.Transaction
                {
                    txid_hex = tx.txid,
                    hash_hex = tx.hash,
                    version = tx.version,
                    size = tx.size,
                    vsize = tx.vsize,
                    weight = tx.weight,
                    locktime = tx.locktime,
                    block_height = height
                };

                var txCompleted = await transactionsRepository.InsertTransactionAsync(targetTx, cancellationToken);
                var txRawCompleted = await rawTxsRepository.InsertTransactionAsync(tx.txid, tx.hex, cancellationToken);

                if (txCompleted && txRawCompleted)
                    txscope.Complete();
                else
                    isTxFailed = true;
            }

            catch (TransactionAbortedException txex)
            {
                _logger.LogError(txex, "Can't save transaction {txId} (insert) for block #{blockNumber}", tx.txid, height);
                isTxFailed = true;
                break;
            }
        }

        return isTxFailed;
    }
    private void ConfigSetup()
    {
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Url);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Username);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Password);

        _authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_explorerConfig.CurrentValue.Node!.Username}:{_explorerConfig.CurrentValue.Node.Password}")));
        _uri = new Uri(_explorerConfig.CurrentValue.Node!.Url!);
        _usernameHash = _explorerConfig.CurrentValue.Node.Password!.GetHashCode();
        _passHash = _explorerConfig.CurrentValue.Node!.Username!.GetHashCode();
    }
}