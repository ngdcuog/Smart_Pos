namespace SmartPOS.Services;
public interface ICurrentUserService
{
    int CurrentEmployeeId { get; }
    string DisplayName { get; }
    string Role { get; }
}
