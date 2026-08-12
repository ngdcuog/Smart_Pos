using Microsoft.EntityFrameworkCore;
using SmartPOS.Data;
using SmartPOS.Models;
using SmartPOS.Models.Enums;
using SmartPOS.Services.Dtos;

namespace SmartPOS.Services;

public sealed class EmployeeService(IDbContextFactory<AppDbContext> contextFactory) : IEmployeeService
{
    public async Task<IReadOnlyList<EmployeeListItem>> GetEmployeesAsync(string? search, EmployeeRole? role, bool? isActive)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var query = context.Employees.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => EF.Functions.Like(x.FullName, $"%{term}%") || EF.Functions.Like(x.EmployeeCode, $"%{term}%") || EF.Functions.Like(x.Email, $"%{term}%"));
        }
        if (role.HasValue) query = query.Where(x => x.Role == role.Value);
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        return await query.OrderBy(x => x.EmployeeCode).Select(x => new EmployeeListItem(x.EmployeeId, x.EmployeeCode, x.FullName,
            x.Email, x.Phone, x.Role, x.IsActive, x.FaceSamples.Any())).ToListAsync();
    }

    public async Task CreateEmployeeAsync(EmployeeInput input)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await ValidateAsync(context, input, null, true);
        context.Employees.Add(new Employee { EmployeeCode = NormalizeCode(input.EmployeeCode), FullName = input.FullName.Trim(),
            Email = input.Email.Trim(), Phone = NormalizeOptional(input.Phone), Role = input.Role, IsActive = true });
        await context.SaveChangesAsync();
    }

    public async Task UpdateEmployeeAsync(EmployeeInput input)
    {
        if (!input.EmployeeId.HasValue) throw new EmployeeServiceException("Không tìm thấy nhân viên cần cập nhật.");
        await using var context = await contextFactory.CreateDbContextAsync();
        var employee = await context.Employees.FindAsync(input.EmployeeId.Value) ?? throw new EmployeeServiceException("Không tìm thấy nhân viên cần cập nhật.");
        await ValidateAsync(context, input, employee.EmployeeId, false);
        if (!string.Equals(employee.EmployeeCode, NormalizeCode(input.EmployeeCode), StringComparison.Ordinal))
            throw new EmployeeServiceException("Mã nhân viên không được thay đổi sau khi tạo.");
        employee.FullName = input.FullName.Trim(); employee.Email = input.Email.Trim(); employee.Phone = NormalizeOptional(input.Phone); employee.Role = input.Role;
        await context.SaveChangesAsync();
    }

    public async Task SetEmployeeActiveStateAsync(int employeeId, bool isActive)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var employee = await context.Employees.FindAsync(employeeId) ?? throw new EmployeeServiceException("Không tìm thấy nhân viên.");
        employee.IsActive = isActive;
        await context.SaveChangesAsync();
    }

    private static async Task ValidateAsync(AppDbContext context, EmployeeInput input, int? existingId, bool isNew)
    {
        var code = NormalizeCode(input.EmployeeCode);
        if (string.IsNullOrWhiteSpace(code)) throw new EmployeeServiceException("Mã nhân viên là bắt buộc.");
        if (code.Length > 20) throw new EmployeeServiceException("Mã nhân viên không được vượt quá 20 ký tự.");
        if (string.IsNullOrWhiteSpace(input.FullName)) throw new EmployeeServiceException("Họ tên nhân viên là bắt buộc.");
        if (string.IsNullOrWhiteSpace(input.Email) || !input.Email.Contains('@')) throw new EmployeeServiceException("Email không hợp lệ.");
        if (isNew && await context.Employees.AnyAsync(x => x.EmployeeCode == code)) throw new EmployeeServiceException("Mã nhân viên đã tồn tại.");
        if (await context.Employees.AnyAsync(x => x.Email == input.Email.Trim() && x.EmployeeId != existingId)) throw new EmployeeServiceException("Email này đã được sử dụng.");
    }

    internal static string NormalizeCode(string? value) => (value ?? string.Empty).Trim().ToUpperInvariant();
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
