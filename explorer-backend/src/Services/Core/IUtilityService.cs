namespace explorer_backend.Services.Core;

public interface IUtilityService
{
    bool VerifyHex(string? hex);
    bool IsNumeric(string? val);
}