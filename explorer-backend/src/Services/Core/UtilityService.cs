using System.Text.RegularExpressions;

namespace explorer_backend.Services.Core;

public class UtilityService : IUtilityService
{
    private static Regex hexRegex = new("^[A-Fa-f0-9]+$", RegexOptions.Compiled);
    public bool VerifyHex(string? hex) => hexRegex.IsMatch(hex ?? "");
}