using SmartPOS.Services.Dtos;
namespace SmartPOS.Services;
public interface IOrderService
{
    Task<OrderReceipt> CreateOrderAsync(CreateOrderRequest request);
}
