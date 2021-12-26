using explorer_backend.Models.Node.Response;

namespace explorer_backend.Models.API;

public class MoneySupplyResponse
{
    public double total_supply { get; set; }
    public double circulating_supply { get; set; }
    public double team_budget { get; set; }
    public double foundation_budget { get; set; }
    public string? budget_address { get; set; }
    public string? foundation_address { get; set; }
}