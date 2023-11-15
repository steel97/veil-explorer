using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ExplorerBackend.Models.Node;
using ExplorerBackend.Models.Node.Response;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Services.Core;
using Microsoft.AspNetCore.Cors;
using ExplorerBackend.Core;
using System.Collections.Frozen;

namespace ExplorerBackend.Controllers;

[ApiController]
[EnableCors(CORSPolicies.NodeProxyPolicy)]
[Route("/")]
public class NodeProxyController : ControllerBase
{
    private readonly FrozenSet<string> NODE_ALLOWED_METHODS;
    
    private readonly string _invalidOperation;
    private readonly static List<string> emptyList = [];
    private readonly IOptions<ServerConfig> _serverConfig;
    private readonly NodeRequester _nodeRequester;
    private readonly ChaininfoSingleton _chainInfoSingleton;

    public NodeProxyController(IOptions<ServerConfig> serverConfig, NodeRequester nodeRequester, ChaininfoSingleton chainInfoSingleton)
    {
        List<string> set = ["importlightwalletaddress", "getwatchonlystatus", "getwatchonlytxes", "checkkeyimages", "getanonoutputs",
        "sendrawtransaction", "getblockchaininfo", "getrawmempool"];
        NODE_ALLOWED_METHODS = set.ToFrozenSet();
        _serverConfig = serverConfig;
        _nodeRequester = nodeRequester;
        _chainInfoSingleton = chainInfoSingleton;
        _invalidOperation = JsonSerializer.Serialize(new GenericResult
        {
            Result = null,
            Id = null,
            Error = new()
            {
                Code = -2,
                Message = "Forbidden by safe mode or invalid method name" // RPC_FORBIDDEN_BY_SAFE_MODE
            }
        });
    }

    [HttpGet]
    public IActionResult Get()
    {
        if (_serverConfig.Value.Swagger?.RedirectFromHomepage ?? false)
            return Redirect(_serverConfig.Value.Swagger?.RoutePrefix ?? "");

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Post(JsonRPCRequest model, CancellationToken cancellationToken)
    {
        // verify method (and parameters?)
        if (!NODE_ALLOWED_METHODS.Contains(model.Method ?? ""))
            return Content(_invalidOperation, "application/json");
        

        if ((model.Method ?? "") == "getblockchaininfo")
        {
            var res1 = new GetBlockchainInfo
            {
                Id = model.Id,
                Result = _chainInfoSingleton.CurrentChainInfo
            };
            return Ok(res1);
        }

        if ((model.Method ?? "") == "getrawmempool")
        {
            var res1 = new GetRawMempool
            {
                Id = model.Id,
                Result = _chainInfoSingleton.UnconfirmedTxs?.Select(a => a.txid ?? "").ToList() ?? emptyList
            };
            return Ok(res1);
        }


        var res = await _nodeRequester.NodeRequest(model.Method, model.Params, cancellationToken);
        return Content(res, "application/json");
    }
}