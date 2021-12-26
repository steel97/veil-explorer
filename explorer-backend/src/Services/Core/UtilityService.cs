using System.Text.RegularExpressions;

namespace ExplorerBackend.Services.Core;

public class UtilityService : IUtilityService
{
    private static Regex hexRegex = new("^[A-Fa-f0-9]+$", RegexOptions.Compiled);
    private static Regex numericRegex = new("^(0|[1-9][0-9]*)$", RegexOptions.Compiled);
    private static Regex addressCleanupRegex = new("/[^a-zA-Z0-9=&]/g", RegexOptions.Compiled);

    public bool VerifyHex(string? hex) => hexRegex.IsMatch(hex ?? "");
    public bool IsNumeric(string? val) => numericRegex.IsMatch(val ?? "");
    public bool VerifyAddress(string? val) => val != null ? val.Length == 34 || val.Length == 42 : false;
    public string CleanupAddress(string? val) => addressCleanupRegex.Replace(val ?? "", "");
}