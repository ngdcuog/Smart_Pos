using Microsoft.EntityFrameworkCore;
using SmartPOS.Data;
using SmartPOS.Models;
using SmartPOS.Models.Enums;
using SmartPOS.Services.Dtos;

namespace SmartPOS.Services;

public sealed class AttendanceService(IDbContextFactory<AppDbContext> contextFactory, IDateTimeProvider clock, AttendanceSettings settings) : IAttendanceService
{
    public async Task<AttendanceEmployee> ResolveEmployeeAsync(string rawEmployeeCode)
    {
        var code = NormalizeQrCompatibleCode(rawEmployeeCode);
        if (string.IsNullOrWhiteSpace(code)) throw new AttendanceServiceException("Vui lòng nhập hoặc quét mã nhân viên.");
        await using var context = await contextFactory.CreateDbContextAsync();
        var employee = await context.Employees.AsNoTracking().SingleOrDefaultAsync(x => x.EmployeeCode == code)
            ?? throw new AttendanceServiceException("Không tìm thấy nhân viên với mã này.");
        if (!employee.IsActive) throw new AttendanceServiceException("Tài khoản nhân viên đã bị vô hiệu hóa.");
        var today = clock.Now.Date;
        var attendance = await context.Attendances.AsNoTracking().SingleOrDefaultAsync(x => x.EmployeeId == employee.EmployeeId && x.Date == today);
        return new AttendanceEmployee(employee.EmployeeId, employee.EmployeeCode, employee.FullName, attendance is not null, attendance?.CheckOutTime is not null);
    }

    public async Task<AttendanceActionResult> CheckInAsync(string rawEmployeeCode)
    {
        var code = NormalizeQrCompatibleCode(rawEmployeeCode);
        var now = clock.Now;
        await using var context = await contextFactory.CreateDbContextAsync();
        var employee = await GetActiveEmployeeAsync(context, code);
        if (await context.Attendances.AnyAsync(x => x.EmployeeId == employee.EmployeeId && x.Date == now.Date))
            throw new AttendanceServiceException("Bạn đã chấm công vào hôm nay.");
        var status = now.TimeOfDay > settings.GetLateThreshold() ? AttendanceStatus.Late : AttendanceStatus.OnTime;
        context.Attendances.Add(new Attendance { EmployeeId = employee.EmployeeId, Date = now.Date, CheckInTime = now, Status = status });
        try { await context.SaveChangesAsync(); }
        catch (DbUpdateException) { throw new AttendanceServiceException("Bạn đã chấm công vào hôm nay."); }
        return new AttendanceActionResult(employee.EmployeeCode, employee.FullName, now, $"Check-in thành công lúc {now:HH:mm}.");
    }

    public async Task<AttendanceActionResult> CheckOutAsync(string rawEmployeeCode)
    {
        var code = NormalizeQrCompatibleCode(rawEmployeeCode);
        var now = clock.Now;
        await using var context = await contextFactory.CreateDbContextAsync();
        var employee = await GetActiveEmployeeAsync(context, code);
        var attendance = await context.Attendances.SingleOrDefaultAsync(x => x.EmployeeId == employee.EmployeeId && x.Date == now.Date)
            ?? throw new AttendanceServiceException("Bạn chưa check-in hôm nay.");
        if (attendance.CheckOutTime.HasValue) throw new AttendanceServiceException("Bạn đã check-out hôm nay.");
        attendance.CheckOutTime = now;
        await context.SaveChangesAsync();
        return new AttendanceActionResult(employee.EmployeeCode, employee.FullName, now, $"Check-out thành công lúc {now:HH:mm}.");
    }

    public async Task<IReadOnlyList<AttendanceRecordItem>> GetAttendanceAsync(DateTime? fromDate, DateTime? toDate, string? search)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var query = context.Attendances.AsNoTracking().Include(x => x.Employee).AsQueryable();
        if (fromDate.HasValue) query = query.Where(x => x.Date >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(x => x.Date <= toDate.Value.Date);
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim(); query = query.Where(x => EF.Functions.Like(x.Employee.FullName, $"%{term}%") || EF.Functions.Like(x.Employee.EmployeeCode, $"%{term}%")); }
        return await query.OrderByDescending(x => x.Date).ThenBy(x => x.Employee.EmployeeCode).Select(x => new AttendanceRecordItem(
            x.AttendanceId, x.EmployeeId, x.Employee.EmployeeCode, x.Employee.FullName, x.Date, x.CheckInTime, x.CheckOutTime,
            x.Status == AttendanceStatus.Late ? "Đi trễ" : "Đúng giờ")).ToListAsync();
    }

    private static async Task<Employee> GetActiveEmployeeAsync(AppDbContext context, string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new AttendanceServiceException("Vui lòng nhập hoặc quét mã nhân viên.");
        var employee = await context.Employees.SingleOrDefaultAsync(x => x.EmployeeCode == code) ?? throw new AttendanceServiceException("Không tìm thấy nhân viên với mã này.");
        if (!employee.IsActive) throw new AttendanceServiceException("Tài khoản nhân viên đã bị vô hiệu hóa.");
        return employee;
    }

    internal static string NormalizeQrCompatibleCode(string? input)
    {
        var value = (input ?? string.Empty).Trim();
        if (value.StartsWith("EMPLOYEE:", StringComparison.OrdinalIgnoreCase)) value = value["EMPLOYEE:".Length..];
        return EmployeeService.NormalizeCode(value);
    }
}
