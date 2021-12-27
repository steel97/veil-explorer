namespace ExplorerBackend.Models.API;

public class BlockRequest
{
    public string? Hash { get; set; }
    public int? Height { get; set; }
    public int Offset { get; set; }
    public int Count { get; set; }
}