# Backend configuration
## Configuring process overview
Backend configured in **appsettings.json**

## Available parameters
It's recommended to use appsettings.json.tpl as configuration template, configuration below contains commentaries prefixed with #, which is incorrect in **json** and added only for information.
```bash
{
  "ConnectionStrings": {
    # change user and password, host (if needed, by default it connects to db via unix socket, recommended and fastest way)
    "DefaultConnection": "Host=/var/run/postgresql/;Port=5432;Database=veilexplorer;Username=<USER>;Password=<PASSWORD>;Pooling=true;Maximum Pool Size=100;Tcp Keepalive=true;Keepalive=60;No Reset On Close=true;Client Encoding=UTF8",
    # standard redis connection string
    # see https://github.com/StackExchange/StackExchange.Redis/blob/main/docs/Basics.md
    "Redis" : "localhost:6379,serviceName=Redis"
  },
  "Server": {
    # secret key to access internal APIs
    "InternalAccessKey": "",
    "CorsOrigins": [
      # frontend and backend usually use different hostname:port pair, so it's important to allow xhr requests from frontend host:port
      "http://localhost:3000"
    ],
    "Swagger": {
      # show swagger ui
      "Enabled": true,
      # empty means that swagger ui hosted directly on main api page
      "RoutePrefix": ""
    }
  },
  "API": {
    # how many blocks can be retrivied form api per request
    "MaxBlocksPullCount": 30,    
    # how many transactions can be retrivied form api per request
    "MaxTransactionsPullCount": 15,
    # how long backend can hold request to wait response from veil-node in ms
    "ApiQueueWaitTimeout": 300,
    # overall wait timeout from veil node, after which request canceled in ms
    "ApiQueueSystemWaitTimeout": 5000,
    # queue processing delay in ms
    "ApiQueueSpinDelay": 20
  },
  "Explorer": {
    # specifies what will be used to store data. true is for in-memory caching, false - database
    "RPCMode" : true,
    # database transaction timeout in ms
    "TxScopeTimeout": 600,
    # realtime notification interval in ms
    "HubNotifyDelay": 5000,
    # delay beween blocks pulling batch in ms
    "PullBlocksDelay": 500,
    # delay between blockchain info pulling in ms
    "PullBlockchainInfoDelay": 500,
    # delay between blockchain stats pulling in ms
    "PullBlockchainStatsDelay": 1800000,
    # delay for queues workers in ms
    "NodeWorkersPullDelay": 20,
    # delay between supply pulling in ms
    "SupplyPullDelay": 60000,
    # delay between mempool pulling in ms
    "PullMempoolDelay": 1000,
    # amount of stats point for transaction statistics
    "StatsPointsCount": 50,
    # blocks pulled per batch
    "BlocksPerBatch": 10,
    # amount of simplified oldest blocks that will be cached
    "OldestSimplifiedBlocksCacheCount": 20010,
    # amount of simplified newest blocks that will be cached
    "SimplifiedBlocksCacheCount": 200010,
    # address of veil budget
    "BudgetAddress": "35uS99ZnfaYB293sJ8ptUEXkUTQXH8WnDe",
    # address of veil foundation
    "FoundationAddress": "38J8RGLetRUNEXycBMPg8oZqLt4bB9hCbt",
    "MemoryCache": {
      # redis standard port
      "Port" : "6379",
      # redis standard host
      "Host" : "localhost",
      # limit of redis memory usage in bytes
      "RedisMaxMemoryUsage": 524288000,
      # delay between expiration scan process
      "ExpirationScanFrequency": 10000,
      # cache expiration absolute time
      "ExpirationApiAbsoluteTime": 3600000,
      # time after block data will expires in days
      "ServerAbsExpCacheTimeDays" : 7,
      # expiration time of user blocks data cache (if it isn't cached by the server)
      "UserAbsExpCacheTimeSec": 30
    },
    "Node": {
      # veil node api address
      "Url": "http://127.0.0.1:5050/",
      # veil node rpc credentials
      "Username": "[noderpc_username]",
      "Password": "[noderpc_password]"
    },
    "ScanTxOutsetQueue": {
      # capacity of balances queue
      "Capacity": 50,
      # see https://docs.microsoft.com/en-us/dotnet/api/system.threading.channels.boundedchannelfullmode?view=net-6.0
      "Mode": 2
    }
  },
  # configuration below is kestrel server configuration
  # see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-6.0
  "AllowedHosts": "*",
  "Kestrel": {
    "EndPointDefaults": {
      "Protocols": "Http1AndHttp2"
    },
    "Endpoints": {
      "HTTP": {
        "Url": "http://*:5000"
      }
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "System.Net.Http.HttpClient": "None"
    }
  }
}
```