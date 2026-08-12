namespace SmartPOS.Services.Dtos;

public sealed record AttendanceActionResult(string EmployeeCode, string EmployeeName, DateTime OccurredAt, string Message);
