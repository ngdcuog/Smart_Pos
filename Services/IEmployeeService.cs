using SmartPOS.Models.Enums;
using SmartPOS.Services.Dtos;

namespace SmartPOS.Services;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeListItem>> GetEmployeesAsync(string? search, EmployeeRole? role, bool? isActive);
    Task CreateEmployeeAsync(EmployeeInput input);
    Task UpdateEmployeeAsync(EmployeeInput input);
    Task SetEmployeeActiveStateAsync(int employeeId, bool isActive);
}
