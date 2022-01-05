using ExplorerBackend.Models.Data;
using ExplorerBackend.VeilStructs;

namespace ExplorerBackend.Models.API;

public class BlockResponse
{
    public bool Found { get; set; }
    public BlockBasicData? NextBlock { get; set; }
    public BlockBasicData? PrevBlock { get; set; }
    public string? VersionHex { get; set; }
    public int TxnCount { get; set; }
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
    public string? TxId { get; set; }
    public List<TxVinSimpleDecoded>? Inputs { get; set; }
    public List<TxVoutSimpleDecoded>? Outputs { get; set; }
    public bool IsBasecoin { get; set; }
    public bool IsCoinStake { get; set; }
    public bool IsZerocoinMint { get; set; }
    public bool IsZerocoinSpend { get; set; }
}

public class TxVinSimpleDecoded
{
    public string? PrevOutTx { get; set; }
    public uint PrevOutNum { get; set; }
    public List<string>? PrevOutAddresses { get; set; }
    public long PrevOutAmount { get; set; }

    public TxInType Type { get; set; }
    public long ZerocoinSpend { get; set; }

    public List<RingCTInput>? AnonInputs { get; set; }
}

public class TxVoutSimpleDecoded
{
    public List<string>? Addresses { get; set; }
    public bool IsOpreturn { get; set; }
    public bool IsCoinBase { get; set; }
    public long Amount { get; set; }
    public OutputTypes Type { get; set; }
    public txnouttype ScriptPubKeyType { get; set; }
    public ulong? CTFee { get; set; }

}

public class RingCTInput
{
    public string? TxId { get; set; }
    public uint VoutN { get; set; }
}

public enum TxInType : int
{
    DEFAULT = 0,
    ZEROCOIN_SPEND = 1,
    ANON = 2
}