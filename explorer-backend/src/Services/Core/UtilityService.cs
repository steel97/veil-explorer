using System.Text.RegularExpressions;

namespace ExplorerBackend.Services.Core;

public class UtilityService : IUtilityService
{
    private static Regex hexRegex = new("^[A-Fa-f0-9]+$", RegexOptions.Compiled);
    private static Regex numericRegex = new("^(0|[1-9][0-9]*)$", RegexOptions.Compiled);
    private static Regex addressCleanupRegex = new("/[^a-zA-Z0-9=&]/g", RegexOptions.Compiled);

    public bool VerifyHex(string? hex) => hexRegex.IsMatch(hex ?? "");
    public bool IsNumeric(string? val) => numericRegex.IsMatch(val ?? "");
    //sv1qqpqrgc5rgd9xd4pmm05wrya44kfg50gpr9nhuxrnq9qczyt4jvxn8qpqwpdhshlhrx3y7yp2m9ehc4ljkfa42kvym2vlyj38fzahg25rgwcsqqqglz679
    public bool VerifyAddress(string? val) => val != null ? val.Length == 34 || val.Length == 42 || val.Length == 121 || val.Length == 103 : false;
    public string CleanupAddress(string? val) => addressCleanupRegex.Replace(val ?? "", "");

    // https://stackoverflow.com/a/321404
    public byte[] HexToByteArray(string hex) => Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();

    public string ToHex(byte[] val) => BitConverter.ToString(val).Replace("-", "").ToLowerInvariant();
}