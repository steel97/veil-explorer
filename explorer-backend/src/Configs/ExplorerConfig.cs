namespace explorer_backend.Configs;

public class ExplorerConfig
{
    public int HubNotifyDelay { get; set; }
    public int PullBlocksDelay { get; set; }
    public int PullBlockchainInfoDelay { get; set; }
    public int BlocksPerBatch { get; set; }
    public Node? Node { get; set; }
}

public class Node
{
    public string? Url { get; set; }
    public string? Authorization { get; set; }
}