# Backend configuration
## Configuring process overview
Backend configured in **appsettings.json**

## Available parameters
```bash
{
  "ConnectionStrings": {
    # change user and password, host (if needed, by default it connects to db via unix socket, recommended and fastest way)
    "DefaultConnection": "Host=/var/run/postgresql/;Port=5432;Database=veilexplorer;Username=<USER>;Password=<PASSWORD>;Pooling=true;Maximum Pool Size=100;Tcp Keepalive=true;Keepalive=60;No Reset On Close=true;Client Encoding=UTF8"
  },
  "Server": {
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
    # address of veil budget
    "BudgetAddress": "35uS99ZnfaYB293sJ8ptUEXkUTQXH8WnDe",
    # address of veil foundation
    "FoundationAddress": "38J8RGLetRUNEXycBMPg8oZqLt4bB9hCbt",
    "MemoryCache": {
      # delay between expiration scan process
      "ExpirationScanFrequency": 10000,
      # cache expiration absolute time
      "ExpirationApiAbsoluteTime": 3600000
    },
    "Node": {
      # veil node api address
      "Url": "http://127.0.0.1:5050/",
      # veil node basic-authorization password
      "Authorization": ""
    },
    "ScanTxOutsetQueue": {
      # capacity of balances cache
      "Capacity": 50,
      # see https://docs.microsoft.com/en-us/dotnet/api/system.threading.channels.boundedchannelfullmode?view=net-6.0
      "Mode": 2
    }
  },
  # configuration below is kestrel server configuration, described here: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-6.0
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