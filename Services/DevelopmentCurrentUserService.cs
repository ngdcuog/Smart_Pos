namespace SmartPOS.Services;

// Temporary development identity. Authentication will replace this implementation.
public sealed class DevelopmentCurrentUserService : ICurrentUserService
{
    public int CurrentEmployeeId => 2;
    public string DisplayName => "Trần Quốc Bảo";
    public string Role => "Cashier";
}
