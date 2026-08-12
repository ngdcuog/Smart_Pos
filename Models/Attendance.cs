using SmartPOS.Models.Enums;
namespace SmartPOS.Models;
public class Attendance { public int AttendanceId { get; set; } public int EmployeeId { get; set; } public DateTime CheckInTime { get; set; } public DateTime? CheckOutTime { get; set; } public DateTime Date { get; set; } public AttendanceStatus Status { get; set; } public Employee Employee { get; set; } = null!; }
