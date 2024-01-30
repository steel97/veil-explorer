# Currently unreleased features
# Backend:
- API-internal/FetchExportedTxs, BackendState
- NodeProxyController (accept post requests on root url and forward it to node (sandboxed))

    Allowed methods:
    1) importlightwalletaddress
    2) getwatchonlystatus
    3) getwatchonlytxes
    4) checkkeyimage
    5) checkkeyimages
    6) getanonoutputs
    7) sendrawtransaction
    
    
- appsettings.json
    1) Added: Server/InternalAccessKey for API-Internal requests
    2) Added: Server/Swagger/RedirectFromHomepage bool that indicate if GET request on root url should be redirected to swagger generated API docs
    3) Added ConnectionString/Redis string for redis configuration (should be also set in MemoryCache/Host-Port)
    4) Added Explorer/RPCMode bool to indicate if backend launched in RPC mode
    5) Moved MemoryCache out of Explorer section + added new configurations (not yet covered in docs)
    

- added RPC mode and Redis/Inmemory caching

# Frontend:
- src/public/fetchtxs.html