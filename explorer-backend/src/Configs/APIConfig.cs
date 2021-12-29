namespace ExplorerBackend.Configs;

public class APIConfig
{
    public int MaxBlocksPullCount { get; set; }
    public int MaxTransactionsPullCount { get; set; }
    public int ApiQueueWaitTimeout { get; set; }
    public int ApiQueueSystemWaitTimeout { get; set; }
    public int ApiQueueSpinDelay { get; set; }
}