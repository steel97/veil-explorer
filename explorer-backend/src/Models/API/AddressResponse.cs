using explorer_backend.Models.Node.Response;

namespace explorer_backend.Models.API;

public class AddressResponse
{
    public bool Fetched { get; set; }
    public bool IsValid { get; set; }
    public ValidateAddreesResult? Address { get; set; }
    public bool AmountFetched { get; set; }
    public double Amount { get; set; }
}