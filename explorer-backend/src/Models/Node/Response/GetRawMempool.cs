namespace ExplorerBackend.Models.Node.Response;

public class GetRawMempool : JsonRPCResponse
{
    public List<string>? Result { get; set; }
}
