namespace ExplorerBackend.Models.Node;

public class JsonRPCResponse
{
    public object? Id { get; set; }
    public JsonRPCError? Error { get; set; }
}

public class JsonRPCError
{
    public int Code { get; set; }
    public string? Message { get; set; }
}