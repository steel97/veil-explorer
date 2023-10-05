using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Models.Node;
using ExplorerBackend.Models.Node.Response;

namespace ExplorerBackend.Services.Core;

public class NodeRequester
{
    private Uri? _uri;
    private AuthenticationHeaderValue? _authHeader;
    private int _usernameHash;
    private int _passHash;
    private readonly string? _nodeFailureError ;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<ExplorerConfig> _explorerConfig;
    private readonly NodeApiCacheSingleton _nodeApiCacheSingleton;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public NodeRequester(IHttpClientFactory httpClientFactory, IOptionsMonitor<ExplorerConfig> explorerConfig, NodeApiCacheSingleton nodeApiCacheSingleton)
    {
        (_explorerConfig, _httpClientFactory, _nodeApiCacheSingleton) = (explorerConfig, httpClientFactory, nodeApiCacheSingleton);
        _nodeFailureError = JsonSerializer.Serialize(new GenericResult
        {
            Result = null,
            Error = new()
            {
                Code = -32603,
                Message = "Node failure"
            }
        }, _serializerOptions);
    }

    public async Task<string> NodeRequest(string? method, List<object>? parameters, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();

            if(_passHash !=_explorerConfig.CurrentValue.Node!.Password!.GetHashCode() || _usernameHash !=_explorerConfig.CurrentValue.Node!.Username!.GetHashCode())        
                ConfigureHttpClient();
                    
            httpClient.BaseAddress = _uri;
            httpClient.DefaultRequestHeaders.Authorization = _authHeader;

            var request = new JsonRPCRequest
            {
                Id = 1,
                Method = method,
                Params = parameters
            };
            var response = await httpClient.PostAsJsonAsync("", request, _serializerOptions, cancellationToken);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch { }

        return _nodeFailureError!; // RPC_INTERNAL_ERROR -32603
    }

    public async ValueTask ScanTxOutsetAndCacheAsync(string target, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient();

            if(_passHash !=_explorerConfig.CurrentValue.Node!.Password!.GetHashCode() || _usernameHash !=_explorerConfig.CurrentValue.Node!.Username!.GetHashCode())        
                ConfigureHttpClient();
                    
            httpClient.BaseAddress = _uri;
            httpClient.DefaultRequestHeaders.Authorization = _authHeader;

            var request = new JsonRPCRequest
            {
                Id = 1,
                Method = "scantxoutset",
                Params = new List<object>(new object[] { "start", new object[] { $"addr({target})" } })
            };
            var response = await httpClient.PostAsJsonAsync("", request, _serializerOptions, cancellationToken);
            var data = await response.Content.ReadFromJsonAsync<ScanTxOutset>(_serializerOptions, cancellationToken);
            if (data != null && data.Result != null)
                _nodeApiCacheSingleton.SetApiCache($"scantxoutset-{target}", data);
        }
        catch { }
    }
    
    public async Task<GetBlockchainInfo?> GetBlockChainInfo(HttpClient httpClient, CancellationToken cancellationToken)
    {
        JsonRPCRequest getBlockchainInfoRequest = new()
        {
            Id = 1,
            Method = "getblockchaininfo",
            Params = new List<object>(Array.Empty<object>())
        };
        var getBlockchainInfoResponse = await httpClient.PostAsJsonAsync("", getBlockchainInfoRequest, _serializerOptions, cancellationToken);
        return await getBlockchainInfoResponse.Content.ReadFromJsonAsync<GetBlockchainInfo>(_serializerOptions, cancellationToken);
    }

    public async Task<GetBlockHash?> GetBlockHash(uint height, HttpClient httpClient, CancellationToken cancellationToken)
    {
        var getBlockHashRequest = new JsonRPCRequest
        {
            Id = 1,
            Method = "getblockhash",
            Params = new List<object>(new object[] { height })
        };
        var getBlockHashResponse = await httpClient.PostAsJsonAsync("", getBlockHashRequest, _serializerOptions, cancellationToken);
        return await getBlockHashResponse.Content.ReadFromJsonAsync<GetBlockHash>(_serializerOptions, cancellationToken);
    }

    public async Task<GetBlock?> GetBlock(string hash, HttpClient httpClient, CancellationToken cancellationToken, int simplifiedTxInfo = 1)
    {
        var getBlockRequest = new JsonRPCRequest
        {
            Id = simplifiedTxInfo,
            Method = "getblock",
            Params = new List<object>(new object[] { hash })
        };
        var getBlockResponse = await httpClient.PostAsJsonAsync("", getBlockRequest, _serializerOptions, cancellationToken);
        return await getBlockResponse.Content.ReadFromJsonAsync<GetBlock>(_serializerOptions, cancellationToken);
    }

    public async Task<GetChainalgoStats?> GetChainAlgoStats(HttpClient httpClient, CancellationToken cancellationToken)
    {
        var getChainalgoStatsRequest = new JsonRPCRequest
        {
            Id = 1,
            Method = "getchainalgostats",
            Params = new List<object>(Array.Empty<object>())
        };
        var getChainalgoStatsResponse = await httpClient.PostAsJsonAsync("", getChainalgoStatsRequest, _serializerOptions, cancellationToken);
        return await getChainalgoStatsResponse.Content.ReadFromJsonAsync<GetChainalgoStats>(_serializerOptions, cancellationToken);
    }

    public async Task<GetRawMempool?> GetRawMempool(HttpClient httpClient, CancellationToken cancellationToken, bool isSerialized = false)
    {
        var getRawMempoolRequest = new JsonRPCRequest
        {
            Id = 1,
            Method = "getrawmempool",
            Params = new List<object>(new object[] { isSerialized })
        };
        var getRawMempoolResult = await httpClient.PostAsJsonAsync("", getRawMempoolRequest, _serializerOptions, cancellationToken);
        return await getRawMempoolResult.Content.ReadFromJsonAsync<GetRawMempool>(_serializerOptions, cancellationToken);
    }

    public async Task<GetRawTransaction?> GetRawTransaction(string txId, HttpClient httpClient, CancellationToken cancellationToken)
    {
        var getRawTxRequest = new JsonRPCRequest
        {
            Id = 1,
            Method = "getrawtransaction",
            Params = new List<object>(new object[] { txId, true })
        };
        var getRawTxResponse = await httpClient.PostAsJsonAsync("", getRawTxRequest, _serializerOptions, cancellationToken);
        return await getRawTxResponse.Content.ReadFromJsonAsync<GetRawTransaction>(_serializerOptions, cancellationToken);
    }

    public async Task<GetBlock?> GetLatestBlock(HttpClient httpClient, CancellationToken cancellationToken, bool isOrphanFix = false)
    {
        byte failedRequests = 0;
    // get blockchain info
    repeatBlockInfoRequest:
        GetBlockchainInfo? blockInfo = await GetBlockChainInfo(httpClient, cancellationToken);

        if (blockInfo is null || blockInfo.Result is null)
        {
            failedRequests++;
            if (failedRequests >= 2)
                throw new ArgumentNullException();

            await Task.Delay(_explorerConfig.CurrentValue.NodeWorkersPullDelay);
            goto repeatBlockInfoRequest;
        }
        failedRequests = 0;

    // get hash by height
    repeatBlockHashRequest:
        GetBlockHash? blockHash = await GetBlockHash(isOrphanFix ? (uint)(blockInfo.Result.Blocks - _explorerConfig.CurrentValue.BlocksOrphanCheck) : blockInfo.Result.Blocks, httpClient, cancellationToken);

        if (blockHash is null || blockHash.Result is null)
        {
            failedRequests++;
            if (failedRequests >= 2)
                throw new ArgumentNullException();

            await Task.Delay(_explorerConfig.CurrentValue.NodeWorkersPullDelay);
            goto repeatBlockHashRequest;
        }
        failedRequests = 0;

    // get block info by hash
    repeatBlockRequest:
        GetBlock? block = await GetBlock(blockHash.Result, httpClient, cancellationToken, simplifiedTxInfo: 2);

        if (block is null || block.Result is null)
        {
            failedRequests++;
            if (failedRequests >= 2)
                throw new ArgumentNullException();

            await Task.Delay(_explorerConfig.CurrentValue.NodeWorkersPullDelay);
            goto repeatBlockRequest;
        }
        return block;
    }
    private void ConfigureHttpClient()
    {
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Url);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Username);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.Node.Password);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.NodeWorkersPullDelay);
        ArgumentNullException.ThrowIfNull(_explorerConfig.CurrentValue.BlocksOrphanCheck);

        _uri = new Uri(_explorerConfig.CurrentValue.Node.Url);
        _authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_explorerConfig.CurrentValue.Node.Username}:{_explorerConfig.CurrentValue.Node.Password}")));
    }
}

// https://github.com/Veil-Project/veil/blob/f43af6ad895423711423b311819bbfd7de3264b0/src/rpc/blockchain.cpp#L2491C38-L2491C38
/*
    Method name: "getblockchaininfo"
    Returns an object containing various state info regarding blockchain processing
    
    Result:
    {
        "chain": "xxxx",                    (string) current network name as defined in BIP70 (main, test, regtest)
        "blocks": xxxxxx,                   (numeric) the current number of blocks processed in the server
        "moneysupply": xxxxx,               (numeric) the current total coin supply (in satoshis)
        "zerocoinsupply":                   (object) the current zerocoin supply
        [
            {
                "denom" : d,                (string) current denomination, or total
                "amount": n,                (numeric) supply in denominated zerocoins
                "percent": p                (numeric) percent of denominated zerocoins over total supply
            },
            ...
        ],
        "headers": xxxxxx,                  (numeric) the current number of headers we have validated
        "bestblockhash": "...",             (string) the hash of the currently best block
        "difficulty_pow": xxxxxx,           (numeric) the current X16RT difficulty
        "difficulty_randomx": xxxxxx,       (numeric) the current RandomX PoW difficulty
        "difficulty_progpow": xxxxxx,       (numeric) the current ProgPow difficulty
        "difficulty_sha256d": xxxxxx,       (numeric) the current SHA256D difficulty
        "difficulty_pos": xxxxxx,           (numeric) the current PoS difficulty
        "mediantime": xxxxxx,               (numeric) median time for the current best block
        "verificationprogress": xxxx,       (numeric) estimate of verification progress [0..1]
        "initialblockdownload": xxxx,       (bool) (debug information) estimate of whether this node is in Initial Block Download mode.
        "chainwork": "xxxx"                 (string) total amount of work in active chain, in hexadecimal
        "chainpow": "xxxx"                  (string) total amount of PoW work in active chain, in hexadecimal
        "size_on_disk": xxxxxx,             (numeric) the estimated size of the block and undo files on disk
        "pruned": xx,                       (boolean) if the blocks are subject to pruning
        "pruneheight": xxxxxx,              (numeric) lowest-height complete block stored (only present if pruning is enabled)
        "automatic_pruning": xx,            (boolean) whether automatic pruning is enabled (only present if pruning is enabled)
        "prune_target_size": xxxxxx,        (numeric) the target size used by pruning (only present if automatic pruning is enabled)
        "softforks":                        (array) status of softforks in progress
        [                
            {
                "id": "xxxx",               (string) name of softfork
                "version": xx,              (numeric) block version
                "reject":                   (object) progress toward rejecting pre-softfork blocks
                {             
                    "status": xx,           (boolean) true if threshold reached
                },
            }, 
            ...
        ],
        "bip9_softforks":                   (object) status of BIP9 softforks in progress
        {           
            "xxxx" :                        (string) name of the softfork
            {                 
                "status": "xxxx",           (string) one of "defined", "started", "locked_in", "active", "failed"
                "bit": xx,                  (numeric) the bit (0-28) in the block version field used to signal this softfork (only for "started" status)
                "startTime": xx,            (numeric) the minimum median time past of a block at which the bit gains its meaning
                "timeout": xx,              (numeric) the median time past of a block at which the deployment is considered failed if not yet locked in
                "since": xx,                (numeric) height of the first block to which the status applies
                "statistics":               (object) numeric statistics about BIP9 signalling for a softfork (only for "started" status)
                {         
                    "period": xx,           (numeric) the length in blocks of the BIP9 signalling period 
                    "threshold": xx,        (numeric) the number of blocks with the version bit set required to activate the feature 
                    "elapsed": xx,          (numeric) the number of blocks elapsed since the beginning of the current period 
                    "count": xx,            (numeric) the number of blocks with the version bit set in the current period 
                    "possible": xx          (boolean) returns false if there are not enough blocks left in this period to pass activation threshold 
                }
            }
        }
        "warnings" : "...",                 (string) any network and blockchain warnings.
    }
*/


/*
    Method name: "getblockhash"
    by height

    Returns hash of block in best-block-chain at height provided.

    Arguments:
    1. height                               (numeric, required) The height index

    Result:
    "hash"                                  (string) The block hash
*/


/*
    Method name: "getblock"
    by blockhash ( verbosity )

    If verbosity is 0, returns a string that is serialized, hex-encoded data for block 'hash'.
    If verbosity is 1, returns an Object with information about block <hash>.
    If verbosity is 2, returns an Object with information about block <hash> and information about each transaction. 

    Arguments:
    1. "blockhash"                   (string, required) The block hash
    2. "verbosity"                   (numeric, optional, default = 1) 0 for hex encoded data, 1 for a json object, and 2 for json object with transaction data
    
    Result (for verbosity = 0):
    "data"                           (string) A string that is serialized, hex-encoded data for block 'hash'.

    Result (for verbosity = 1):
    {
      "hash" : "hash",               (string) the block hash (same as provided)
      "confirmations" : n,           (numeric) The number of confirmations, or -1 if the block is not on the main chain
      "size" : n,                    (numeric) The block size
      "strippedsize" : n,            (numeric) The block size excluding witness data
      "weight" : n                   (numeric) The block weight as defined in BIP 141
      "height" : n,                  (numeric) The block height or index
      "version" : n,                 (numeric) The block version
      "versionHex" : "00000000",     (string) The block version formatted in hexadecimal
      "merkleroot" : "xxxx",         (string) The merkle root
      "tx" :                         (array of string) The transaction ids
      [
        "transactionid",             (string) The transaction id
        ...
      ],"
      "time" : ttt,                  (numeric) The block time in seconds since epoch (Jan 1 1970 GMT)
      "mediantime" : ttt,            (numeric) The median block time in seconds since epoch (Jan 1 1970 GMT)
      "nonce" : n,                   (numeric) The nonce
      "bits" : "1d00ffff",           (string) The bits
      "difficulty" : x.xxx,          (numeric) The difficulty
      "chainwork" : "xxxx",          (string) Expected number of hashes required to produce the chain up to this block (in hex)
      "nTx" : n,                     (numeric) The number of transactions in the block.
      "previousblockhash" : "hash",  (string) The hash of the previous block
      "nextblockhash" : "hash"       (string) The hash of the next block
    }

    Result (for verbosity = 2):"
    {
      ...,                            Same output as verbosity = 1
      "tx" :                          (array of Objects) The transactions in the format of the getrawtransaction RPC. Different from verbosity = 1 "tx" result.
      [                       
        "..."
      ],
      ,...                            Same output as verbosity = 1
    }
*/

/*
    Method name: "getrawtransaction"

    NOTE: By default this function only works for mempool transactions. If the -txindex option is
    enabled, it also works for blockchain transactions. If the block which contains the transaction
    is known, its hash can be provided even for nodes without -txindex. Note that if a blockhash is
    provided, only that block will be searched and if the transaction is in the mempool or other
    blocks, or if this node does not have the given block available, the transaction will not be found.
    DEPRECATED: for now, it also works for transactions with unspent outputs.

    Return the raw transaction data.
    If verbose is 'true', returns an Object with information about 'txid'.
    If verbose is 'false' or omitted, returns a string that is serialized, hex-encoded data for 'txid'.

    Arguments:
    1. "txid"                                   (string, required) The transaction id
    2.  verbose                                 (bool, optional, default = false) If false, return a string, otherwise return a json object
    3. "blockhash"                              (string, optional) The block in which to look for the transaction

    Result (if verbose is not set or set to false):
    data"                                       (string) The serialized, hex-encoded data for 'txid'

    Result (if verbose is set to true):
    {
        "in_active_chain":                      (bool) Whether specified block is in the active chain or not (only present with explicit "blockhash" argument)
        "hex" : "data",                         (string) The serialized, hex-encoded data for 'txid'
        "txid" : "id",                          (string) The transaction id (same as provided)
        "hash" : "id",                          (string) The transaction hash (differs from txid for witness transactions)
        "size" : n,                             (numeric) The serialized transaction size
        "vsize" : n,                            (numeric) The virtual transaction size (differs from size for witness transactions)
        "weight" : n,                           (numeric) The transaction's weight (between vsize*4-3 and vsize*4)
        "version" : n,                          (numeric) The version
        "locktime" : ttt,                       (numeric) The lock time
        "vin" :                                 (array of json objects)
        [                       
            {
                "txid": "id",                   (string) The transaction id
                "vout": n,                      (numeric) 
                "scriptSig":                    (json object) The script
                    {
                        "asm": "asm",           (string) asm
                        "hex": "hex"            (string) hex
                    },
                "sequence": n                   (numeric) The script sequence number
                "txinwitness": ["hex", ...]     (array of string) hex-encoded witness data (if any)
            }
            ,...
        ],
        "vout" :                                 (array of json objects)
        [
            {
                "value" : x.xxx,                 (numeric) The value in " + CURRENCY_UNIT + "
                "valueSat" : x.xxx,              (numeric) The sat value in " + CURRENCY_UNIT + "
                "vout.n" : n,                    (numeric) index
                "scriptPubKey" :                 (json object)
                {
                    "asm" : "asm",               (string) the asm
                    "hex" : "hex",               (string) the hex
                    "reqSigs" : n,               (numeric) The required sigs
                    "type" : "pubkeyhash",       (string) The type, eg 'pubkeyhash'
                    "addresses" :                (json array of string)
                    [
                        "address"                (string) veil address
                        ,...
                    ]
                }
            }
            ,...
        ],
        "blockhash" : "hash",                    (string) The block hash
        "confirmations" : n,                     (numeric) The confirmations
        "time" : ttt,                            (numeric) The transaction time in seconds since epoch (Jan 1 1970 GMT)
        "blocktime" : ttt                        (numeric) The block time in seconds since epoch (Jan 1 1970 GMT)
    }
*/