namespace explorer_backend.Models.Node;
public class JsonRPCRequest
{
    public string Jsonrpc
    {
        get
        {
            return "1.0";
        }
    }
    public object? Id { get; set; }
    public string? Method { get; set; }
    public List<object>? Params { get; set; }
}