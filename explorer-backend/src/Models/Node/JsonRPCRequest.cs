using System.Text.Json.Serialization;

namespace ExplorerBackend.Models.Node;

public class JsonRPCRequest
{
    [JsonPropertyName("jsonrpc")]
    public string Jsonrpc
    {
        get
        {
            return "1.0";
        }
    }
    [JsonPropertyName("id")]
    public object? Id { get; set; }
    [JsonPropertyName("method")]
    public string? Method { get; set; }
    [JsonPropertyName("params")]
    public List<object>? Params { get; set; }
}