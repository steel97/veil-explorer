namespace explorer_backend.Services.Core;

public interface INodeRequester
{
    ValueTask ValidateAddressAndCacheAsync(string target, CancellationToken token);
    ValueTask ScanTxOutsetAndCacheAsync(string target, CancellationToken token);
}