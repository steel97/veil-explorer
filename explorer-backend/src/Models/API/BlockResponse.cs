using ExplorerBackend.Models.Data;

namespace ExplorerBackend.Models.API;

public class BlockResponse
{
    public bool Found { get; set; }
    public Block? Block { get; set; }
    public List<TransactionExtended>? Transactions { get; set; }
}

public class DecodedTransaction
{

}