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

# Frontend:
- src/public/fetchtxs.html