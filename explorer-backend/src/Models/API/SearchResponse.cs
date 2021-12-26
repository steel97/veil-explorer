namespace ExplorerBackend.Models.API;

public class SearchResponse
{
    public bool Found { get; set; }
    public string? Query { get; set; }
    public EntityType Type { get; set; }
}

public enum EntityType : short
{
    UNKNOWN = 0,
    BLOCK_HEIGHT = 1,
    BLOCK_HASH = 2,
    TRANSACTION_HASH = 3,
    ADDRESS = 4
}