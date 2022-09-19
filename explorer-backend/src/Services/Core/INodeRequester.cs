namespace ExplorerBackend.Services.Core;

public interface INodeRequester
{
    Task<string> NodeRequest(string? method, List<object>? parameters, CancellationToken cancellationToken);
    ValueTask ScanTxOutsetAndCacheAsync(string target, CancellationToken token);
}