namespace SmartPOS.Models;

public class Customer
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int LoyaltyPoints { get; set; }
}
