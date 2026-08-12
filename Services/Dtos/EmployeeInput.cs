using SmartPOS.Models.Enums;

namespace SmartPOS.Services.Dtos;

public sealed record EmployeeInput(int? EmployeeId, string EmployeeCode, string FullName, string Email, string? Phone, EmployeeRole Role);
