namespace ExplorerBackend.Services.Core;

public interface INodeRequester
{
    ValueTask ScanTxOutsetAndCacheAsync(string target, CancellationToken token);
}