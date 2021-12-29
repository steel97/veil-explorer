namespace ExplorerBackend.Configs;

public class ExplorerConfig
{
    public int TxScopeTimeout { get; set; }
    public int HubNotifyDelay { get; set; }
    public int PullBlocksDelay { get; set; }
    public int PullBlockchainInfoDelay { get; set; }
    public int PullBlockchainStatsDelay { get; set; }
    public int NodeWorkersPullDelay { get; set; }
    public int SupplyPullDelay { get; set; }
    public int PullMempoolDelay { get; set; }
    public int StatsPointsCount { get; set; }
    public int BlocksPerBatch { get; set; }
    public string? BudgetAddress { get; set; }
    public string? FoundationAddress { get; set; }
    public MemoryCache? MemoryCache { get; set; }
    public Node? Node { get; set; }
    public QueueConfig? ScanTxOutsetQueue { get; set; }
    public QueueConfig? ValidateAddressQueue { get; set; }
}

public class MemoryCache
{
    public int ExpirationScanFrequency { get; set; }
    public int ExpirationApiAbsoluteTime { get; set; }
}

public class Node
{
    public string? Url { get; set; }
    public string? Authorization { get; set; }
}

public class QueueConfig
{
    public int Capacity { get; set; }
    public int Mode { get; set; } // matches https://docs.microsoft.com/en-us/dotnet/api/system.threading.channels.boundedchannelfullmode?view=net-6.0
}