using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ExplorerBackend.Models.Node;
using ExplorerBackend.Models.Node.Response;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Core;

namespace ExplorerBackend.Controllers;

[ApiController]
[Route("/")]
public class NodeProxyController : ControllerBase
{
    private static List<string> NODE_ALLOWED_METHODS = new(new string[] {
        "importlightwalletaddress", "getwatchonlystatus", "getwatchonlytxes", "checkkeyimage", "checkkeyimages", "getanonoutputs",
        "sendrawtransaction"
    });
    private static JsonSerializerOptions serializeOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };


    private readonly ILogger _logger;
    private readonly IOptions<ServerConfig> _serverConfig;
    private readonly IUtilityService _utilityService;
    private readonly INodeRequester _nodeRequester;

    public NodeProxyController(ILogger<NodeProxyController> logger, IOptions<ServerConfig> serverConfig, IUtilityService utilityService, INodeRequester nodeRequester)
    {
        _logger = logger;
        _serverConfig = serverConfig;
        _utilityService = utilityService;
        _nodeRequester = nodeRequester;
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
        {
            var error = new GenericResult
            {
                Result = null,
                Error = new()
                {
                    Code = -2,
                    Message = "Forbidden by safe mode" // RPC_FORBIDDEN_BY_SAFE_MODE
                }
            };
            return Content(JsonSerializer.Serialize<GenericResult>(error, serializeOptions), "application/json");
        }

        var res = await _nodeRequester.NodeRequest(model.Method, model.Params, cancellationToken);
        return Content(res, "application/json");
    }
}