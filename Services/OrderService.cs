using Microsoft.EntityFrameworkCore;
using SmartPOS.Data;
using SmartPOS.Models;
using SmartPOS.Models.Enums;
using SmartPOS.Services.Dtos;

namespace SmartPOS.Services;

public sealed class OrderService(IDbContextFactory<AppDbContext> contextFactory) : IOrderService
{
    public async Task<OrderReceipt> CreateOrderAsync(CreateOrderRequest request)
    {
        if (request.Items.Count == 0) throw new OrderServiceException("Giỏ hàng đang trống.");
        if (request.DiscountAmount < 0) throw new OrderServiceException("Giảm giá không được nhỏ hơn 0.");

        var normalizedItems = request.Items.GroupBy(x => x.ProductId).Select(x => new CreateOrderItemRequest(x.Key, x.Sum(i => i.Quantity))).ToList();
        if (normalizedItems.Any(x => x.Quantity <= 0)) throw new OrderServiceException("Số lượng sản phẩm phải lớn hơn 0.");

        await using var context = await contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            if (!await context.Employees.AnyAsync(x => x.EmployeeId == request.EmployeeId && x.IsActive))
                throw new OrderServiceException("Không xác định được nhân viên bán hàng.");

            var productIds = normalizedItems.Select(x => x.ProductId).ToList();
            var products = new Dictionary<int, Product>();
            foreach (var id in productIds)
            {
                var product = await context.Products.FirstOrDefaultAsync(x => x.ProductId == id);
                if (product != null) products[id] = product;
            }
            if (products.Count != productIds.Count) throw new OrderServiceException("Một hoặc nhiều sản phẩm không còn tồn tại.");

            foreach (var item in normalizedItems)
            {
                var product = products[item.ProductId];
                if (product.StockQuantity < item.Quantity)
                    throw new OrderServiceException($"Không đủ tồn kho cho \"{product.ProductName}\". Yêu cầu: {item.Quantity}. Hiện có: {product.StockQuantity}.");
            }

            var total = normalizedItems.Sum(item => products[item.ProductId].SellingPrice * item.Quantity);
            if (request.DiscountAmount > total) throw new OrderServiceException("Giảm giá không được lớn hơn tạm tính.");
            var now = DateTime.Now;
            var order = new Order { EmployeeId = request.EmployeeId, OrderDate = now, TotalAmount = total, DiscountAmount = request.DiscountAmount, FinalAmount = total - request.DiscountAmount, PaymentMethod = request.PaymentMethod };
            context.Orders.Add(order);

            foreach (var item in normalizedItems)
            {
                var product = products[item.ProductId];
                var lineTotal = product.SellingPrice * item.Quantity;
                product.StockQuantity -= item.Quantity;
                context.OrderDetails.Add(new OrderDetail { Order = order, ProductId = product.ProductId, Quantity = item.Quantity, UnitPrice = product.SellingPrice, LineTotal = lineTotal });
                context.StockTransactions.Add(new StockTransaction { ProductId = product.ProductId, Quantity = item.Quantity, Type = StockTransactionType.Export, TransactionDate = now });
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return new OrderReceipt(order.OrderId, order.TotalAmount, order.DiscountAmount, order.FinalAmount);
        }
        catch (OrderServiceException)
        {
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new OrderServiceException($"Không thể hoàn tất thanh toán do lỗi CSDL: {ex.Message}", ex);
        }
    }
}
