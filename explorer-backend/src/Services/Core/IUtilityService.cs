namespace explorer_backend.Services.Core;

public interface IUtilityService
{
    bool VerifyHex(string? hex);
    bool IsNumeric(string? val);
    bool VerifyAddress(string? val);
    string CleanupAddress(string? val);
}