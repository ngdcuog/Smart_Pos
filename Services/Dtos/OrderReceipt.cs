namespace SmartPOS.Services.Dtos;
public sealed record OrderReceipt(int OrderId, decimal TotalAmount, decimal DiscountAmount, decimal FinalAmount);
