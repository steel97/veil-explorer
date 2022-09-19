namespace ExplorerBackend.Models.Node.Response;

public class GenericResult : JsonRPCResponse
{
    public object? Result { get; set; }
}