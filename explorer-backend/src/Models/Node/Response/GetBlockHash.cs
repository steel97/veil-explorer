namespace explorer_backend.Models.Node.Response;

public class GetBlockHash : JsonRPCResponse
{
    public string? Result { get; set; }
}