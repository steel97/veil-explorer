namespace ExplorerBackend.Services.Core;

public interface IUtilityService
{
    bool VerifyHex(string? hex);
    bool IsNumeric(string? val);
    bool VerifyAddress(string? val);
    string CleanupAddress(string? val);
    byte[] HexToByteArray(string hex);
    string ToHex(byte[] val);
    string ToHexReversed(byte[] val);
}