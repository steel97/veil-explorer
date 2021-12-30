namespace ExplorerBackend.Models.Node.Response;

public class GetChainalgoStats : JsonRPCResponse
{
    public GetChainalgoStatsResult? Result { get; set; }
}

public class GetChainalgoStatsResult
{
    public ulong Start { get; set; }
    public ulong Finish { get; set; }
    public int Period { get; set; }
    public uint Startblock { get; set; }
    public uint Endblock { get; set; }
    public int Pos { get; set; }
    public int Progpow { get; set; }
    public int Randomx { get; set; }
    public int Sha256d { get; set; }
    public int X16rt { get; set; } // for compatability
}