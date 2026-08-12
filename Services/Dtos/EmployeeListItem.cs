using SmartPOS.Models.Enums;

namespace SmartPOS.Services.Dtos;

public sealed record EmployeeListItem(int EmployeeId, string EmployeeCode, string FullName, string Email, string? Phone,
    EmployeeRole Role, bool IsActive, bool HasFaceSamples)
{
    public string StatusText => IsActive ? "Đang hoạt động" : "Đã vô hiệu hóa";
    public string StatusAction => IsActive ? "Vô hiệu hóa" : "Kích hoạt";
    public string FaceStatusText => HasFaceSamples ? "Đã đăng ký" : "Chưa đăng ký";
    public string FaceActionText => HasFaceSamples ? "Đăng ký lại" : "Đăng ký khuôn mặt";
}
