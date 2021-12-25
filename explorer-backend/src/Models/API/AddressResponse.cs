namespace explorer_backend.Models.API;

public class AddressResponse
{
    public bool IsValid { get; set; }
    public bool Fetched { get; set; }
    public Address? Address { get; set; }
}

public class Address
{

}
