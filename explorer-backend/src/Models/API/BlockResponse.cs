using ExplorerBackend.Models.Data;

namespace ExplorerBackend.Models.API;

public class BlockResponse
{
    public bool Found { get; set; }
    public BlockBasicData? NextBlock { get; set; }
    public BlockBasicData? PrevBlock { get; set; }
    public string? VersionHex { get; set; }
    public Block? Block { get; set; }
    public List<TransactionSimpleDecoded>? Transactions { get; set; }
}

public class BlockBasicData
{
    public string? Hash { get; set; }
    public int Height { get; set; }
}

public class TransactionSimpleDecoded
{

}