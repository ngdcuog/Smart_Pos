namespace SmartPOS.Services.Dtos;

public sealed record AttendanceRecordItem(int AttendanceId, int EmployeeId, string EmployeeCode, string EmployeeName,
    DateTime Date, DateTime CheckInTime, DateTime? CheckOutTime, string Status);
