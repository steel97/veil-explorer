{
  "ConnectionStrings": {
    "DefaultConnection": "Host=/var/run/postgresql/;Port=5432;Database=veilexplorer;Username=<USER>;Password=<PASSWORD>;Pooling=true;Maximum Pool Size=100;Tcp Keepalive=true;Keepalive=60;No Reset On Close=true;Client Encoding=UTF8"
  },
  "Server": {
    "CorsOrigins": [
      "http://localhost:3000"
    ],
    "Swagger": {
      "Enabled": true,
      "RoutePrefix": ""
    }
  },
  "API": {
    "MaxBlocksPullCount": 30,
    "MaxTransactionsPullCount": 15,
    "ApiQueueWaitTimeout": 300,
    "ApiQueueSystemWaitTimeout": 5000,
    "ApiQueueSpinDelay": 20
  },
  "Explorer": {
    "TxScopeTimeout": 600,
    "HubNotifyDelay": 1000,
    "PullBlocksDelay": 500,
    "PullBlockchainInfoDelay": 500,
    "PullBlockchainStatsDelay": 1800000,
    "NodeWorkersPullDelay": 20,
    "SupplyPullDelay": 60000,
    "PullMempoolDelay": 1000,
    "StatsPointsCount": 50,
    "BlocksPerBatch": 10,
    "BudgetAddress": "35uS99ZnfaYB293sJ8ptUEXkUTQXH8WnDe",
    "FoundationAddress": "38J8RGLetRUNEXycBMPg8oZqLt4bB9hCbt",
    "MemoryCache": {
      "ExpirationScanFrequency": 10000,
      "ExpirationApiAbsoluteTime": 3600000
    },
    "Node": {
      "Url": "http://127.0.0.1:5050/",
      "Authorization": ""
    },
    "ScanTxOutsetQueue": {
      "Capacity": 50,
      "Mode": 2
    }
  },
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