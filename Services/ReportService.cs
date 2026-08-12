using Microsoft.EntityFrameworkCore;
using SmartPOS.Data;
using SmartPOS.Models.Enums;
using SmartPOS.Services.Dtos;

namespace SmartPOS.Services;

public sealed class ReportService(IDbContextFactory<AppDbContext> factory) : IReportService
{
    public async Task<DashboardDataDto> GetDashboardAsync()
    {
        var today = DateTime.Today;
        var sales = await GetSalesReportAsync(today.AddDays(-6), today);

        await using var context = await factory.CreateDbContextAsync();
        var start = today;
        var end = today.AddDays(1);
        var todayOrders = context.Orders.AsNoTracking().Where(order => order.OrderDate >= start && order.OrderDate < end);

        var lowStock = await context.Products.AsNoTracking()
            .Where(product => product.StockQuantity <= product.MinStockAlert)
            .OrderBy(product => product.StockQuantity)
            .Take(6)
            .Select(product => new LowStockDto(
                product.ProductName,
                product.StockQuantity,
                product.MinStockAlert,
                product.StockQuantity == 0 ? "Hết hàng" : "Sắp hết"))
            .ToListAsync();

        var recentOrders = await context.Orders.AsNoTracking()
            .OrderByDescending(order => order.OrderDate)
            .Take(6)
            .Select(order => new RecentOrderDto(
                order.OrderId,
                order.OrderDate,
                order.Employee.FullName,
                order.FinalAmount,
                order.PaymentMethod == PaymentMethod.Cash ? "Tiền mặt" : "Chuyển khoản"))
            .ToListAsync();

        return new DashboardDataDto(
            await todayOrders.SumAsync(order => (decimal?)order.FinalAmount) ?? 0,
            await todayOrders.CountAsync(),
            await context.Products.CountAsync(product => product.StockQuantity <= product.MinStockAlert),
            await context.Attendances.CountAsync(attendance => attendance.Date == today),
            sales.RevenuePoints,
            sales.TopProducts.Take(5).ToList(),
            lowStock,
            recentOrders);
    }

    public async Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to)
    {
        if (from.Date > to.Date)
            throw new ArgumentException("Ngày bắt đầu không được lớn hơn ngày kết thúc.");

        await using var context = await factory.CreateDbContextAsync();
        var start = from.Date;
        var end = to.Date.AddDays(1);
        var orders = context.Orders.AsNoTracking().Where(order => order.OrderDate >= start && order.OrderDate < end);

        var dailyRows = await orders
            .GroupBy(order => order.OrderDate.Date)
            .Select(group => new { Date = group.Key, Revenue = group.Sum(order => order.FinalAmount), OrderCount = group.Count() })
            .ToListAsync();
        var dailyMap = dailyRows.ToDictionary(row => row.Date);
        var revenuePoints = Enumerable.Range(0, (to.Date - from.Date).Days + 1)
            .Select(day =>
            {
                var date = start.AddDays(day);
                return dailyMap.TryGetValue(date, out var row)
                    ? new RevenuePointDto(row.Date, row.Revenue, row.OrderCount)
                    : new RevenuePointDto(date, 0, 0);
            })
            .ToList();

        var details = context.OrderDetails.AsNoTracking()
            .Where(detail => detail.Order.OrderDate >= start && detail.Order.OrderDate < end);

        // Materialize aggregate rows first: EF Core cannot order by a DTO constructor projection.
        var productRows = await details
            .GroupBy(detail => detail.Product.ProductName)
            .Select(group => new { ProductName = group.Key, Quantity = group.Sum(detail => detail.Quantity), Revenue = group.Sum(detail => detail.LineTotal) })
            .ToListAsync();
        var topProducts = productRows
            .OrderByDescending(row => row.Quantity)
            .ThenByDescending(row => row.Revenue)
            .Take(10)
            .Select(row => new TopProductDto(row.ProductName, row.Quantity, row.Revenue))
            .ToList();

        var categoryRows = await details
            .GroupBy(detail => detail.Product.Category.CategoryName)
            .Select(group => new { CategoryName = group.Key, Quantity = group.Sum(detail => detail.Quantity), Revenue = group.Sum(detail => detail.LineTotal) })
            .ToListAsync();
        var categorySales = categoryRows
            .OrderByDescending(row => row.Revenue)
            .Select(row => new CategorySalesDto(row.CategoryName, row.Quantity, row.Revenue))
            .ToList();

        var totalRevenue = await orders.SumAsync(order => (decimal?)order.FinalAmount) ?? 0;
        var totalOrders = await orders.CountAsync();
        var unitsSold = await details.SumAsync(detail => (int?)detail.Quantity) ?? 0;

        return new SalesReportDto(
            totalRevenue,
            totalOrders,
            totalOrders == 0 ? 0 : totalRevenue / totalOrders,
            unitsSold,
            revenuePoints,
            topProducts,
            categorySales);
    }
}
