using System.Text.RegularExpressions;

namespace explorer_backend.Services.Core;

public class UtilityService : IUtilityService
{
    private static Regex hexRegex = new("^[A-Fa-f0-9]+$", RegexOptions.Compiled);
    private static Regex numericRegex = new("^(0|[1-9][0-9]*)$", RegexOptions.Compiled);

    public bool VerifyHex(string? hex) => hexRegex.IsMatch(hex ?? "");
    public bool IsNumeric(string? val) => numericRegex.IsMatch(val ?? "");
}