namespace ExplorerBackend.Models.API;

public class UnconfirmedTxResponse
{
    public int TxnCount { get; set; }
    public List<TransactionSimpleDecoded>? Transactions { get; set; }
}