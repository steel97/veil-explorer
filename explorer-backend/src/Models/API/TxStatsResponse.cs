namespace ExplorerBackend.Models.API;

public class TxStatsComposite
{
    public Dictionary<string, TxStatsEntry>? TxStats { get; set; }
}

public class TxStatsEntry
{
    public List<string>? Labels { get; set; }
    public List<TxStatsDataPoint>? TxCounts { get; set; }
    public List<TxStatsDataPoint>? TxRates { get; set; }
}

public class TxStatsDataPoint
{
    public double X { get; set; }
    public double Y { get; set; }
}