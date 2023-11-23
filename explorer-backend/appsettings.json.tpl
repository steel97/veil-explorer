{
  "ConnectionStrings": {
    "DefaultConnection": "Host=/var/run/postgresql/;Port=5432;Database=veilexplorer;Username=<USER>;Password=<PASSWORD>;Pooling=true;Maximum Pool Size=100;Tcp Keepalive=true;Keepalive=60;No Reset On Close=true;Client Encoding=UTF8",
    "Redis" : "localhost:6379"
  },
  "Server": {
    "InternalAccessKey": "",
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
    "RPCMode": true,
    "TxScopeTimeout": 600,
    "HubNotifyDelay": 5000,
    "PullBlocksDelay": 500,
    "PullBlockchainInfoDelay": 500,
    "PullBlockchainStatsDelay": 1800000,
    "NodeWorkersPullDelay": 20,
    "SupplyPullDelay": 60000,
    "PullMempoolDelay": 1000,
    "StatsPointsCount": 50,
    "BlocksPerBatch": 10,
    "BlocksOrphanCheck": 12,
    "OldestSimplifiedBlocksCacheCount": 20010,
    "SimplifiedBlocksCacheCount": 200010,
    "BudgetAddress": "35uS99ZnfaYB293sJ8ptUEXkUTQXH8WnDe",
    "FoundationAddress": "38J8RGLetRUNEXycBMPg8oZqLt4bB9hCbt",
    "MemoryCache": {
      "ExpirationScanFrequency": 10000,
      "ExpirationApiAbsoluteTime": 3600000,
      "ServerAbsoluteExpirationCacheTimeMin" : 10080,
      "UserAbsoluteExpirationCacheTimeSec": 30
    },
    "Node": {
      "Url": "http://127.0.0.1:5050/",
      "Username": "[noderpc_username]",
      "Password": "[noderpc_password]"
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
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "System.Net.Http.HttpClient": "Error"
      }
    },
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithThreadId"
    ],
    "Destructure": [
      {
        "Name": "ToMaximumDepth",
        "Args": {
          "maximumDestructuringDepth": 4
        }
      },
      {
        "Name": "ToMaximumStringLength",
        "Args": {
          "maximumStringLength": 100
        }
      },
      {
        "Name": "ToMaximumCollectionCount",
        "Args": {
          "maximumCollectionCount": 10
        }
      }
    ],
    "Properties": {
      "Application": "VeilExplorerBackend"
    },
    "WriteTo": [
      {
        "Name": "Async",
        "Args": {
          "configure": [
            {
              "Name": "Console"
            }
          ]
        }
      },
      {
        "Name": "Async",
        "Args": {
          "configure": [
            {
              "Name": "File",
              "Path": "./logs/log-.txt",
              "BufferSize": 4096,
              "BlockWhenFull": true
            }
          ]
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "./logs/log-.log",
          "rollingInterval": "Day",
          "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
        }
      }
    ]
  }
}