using SmartPOS.Services.Dtos;

namespace SmartPOS.Services;

public interface IAttendanceService
{
    Task<AttendanceEmployee> ResolveEmployeeAsync(string rawEmployeeCode);
    Task<AttendanceActionResult> CheckInAsync(string rawEmployeeCode);
    Task<AttendanceActionResult> CheckOutAsync(string rawEmployeeCode);
    Task<IReadOnlyList<AttendanceRecordItem>> GetAttendanceAsync(DateTime? fromDate, DateTime? toDate, string? search);
}
