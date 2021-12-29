using ExplorerBackend.Models.System;

namespace ExplorerBackend.Models.API;

public class AddressResponse
{
    public bool Fetched { get; set; }
    public bool IsValid { get; set; }
    public ValidateAddress? Address { get; set; }
    public bool AmountFetched { get; set; }
    public double Amount { get; set; }
    public int? Version { get; set; }
    public string? Hash { get; set; }
    public string? ScriptHash { get; set; }
}