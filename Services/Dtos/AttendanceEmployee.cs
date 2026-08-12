namespace SmartPOS.Services.Dtos;

public sealed record AttendanceEmployee(int EmployeeId, string EmployeeCode, string FullName, bool HasCheckedInToday, bool HasCheckedOutToday);
