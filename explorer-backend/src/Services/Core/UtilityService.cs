using System.Text.RegularExpressions;

namespace ExplorerBackend.Services.Core;

public partial class UtilityService : IUtilityService
{
    private readonly static Regex hexRegex = HexRegex();
    private readonly static Regex numericRegex = NumericRegex();
    private readonly static Regex addressCleanupRegex = AddressCleanupRegex();

    public bool VerifyHex(string? hex) => hexRegex.IsMatch(hex ?? "") && hex?.Length % 2 == 0;
    public bool IsNumeric(string? val) => numericRegex.IsMatch(val ?? "");
    //sv1qqpqrgc5rgd9xd4pmm05wrya44kfg50gpr9nhuxrnq9qczyt4jvxn8qpqwpdhshlhrx3y7yp2m9ehc4ljkfa42kvym2vlyj38fzahg25rgwcsqqqglz679
    public bool VerifyAddress(string? val) => val != null && (val.Length == 34 || val.Length == 42 || val.Length == 121 || val.Length == 103);
    public string CleanupAddress(string? val) => addressCleanupRegex.Replace(val ?? "", "");

    public byte[] HexToByteArray(string hex) => Convert.FromHexString(hex);
    public string ToHex(byte[] val) => BitConverter.ToString(val).Replace("-", "").ToLowerInvariant();
    public string ToHexReversed(byte[] val) => ToHex(val.Reverse().ToArray());
    [GeneratedRegex("/[^a-zA-Z0-9=&]/g", RegexOptions.Compiled)]
    private static partial Regex AddressCleanupRegex();
    [GeneratedRegex("^(0|[1-9][0-9]*)$", RegexOptions.Compiled)]
    private static partial Regex NumericRegex();
    [GeneratedRegex("^[A-Fa-f0-9]+$", RegexOptions.Compiled)]
    private static partial Regex HexRegex();
}