using SmartPOS.Models.Enums;
namespace SmartPOS.Services.Dtos;
public sealed record CreateOrderRequest(int EmployeeId, decimal DiscountAmount, PaymentMethod PaymentMethod, IReadOnlyList<CreateOrderItemRequest> Items);
