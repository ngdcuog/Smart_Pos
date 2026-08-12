namespace SmartPOS.Services.Dtos;
public sealed record RevenuePointDto(DateTime Date, decimal Revenue, int OrderCount);
public sealed record TopProductDto(string ProductName, int QuantitySold, decimal Revenue);
public sealed record LowStockDto(string ProductName, int StockQuantity, int MinStockAlert, string Status);
public sealed record RecentOrderDto(int OrderId, DateTime OrderDate, string CashierName, decimal FinalAmount, string PaymentMethod);
public sealed record CategorySalesDto(string CategoryName, int QuantitySold, decimal Revenue);
public sealed record DashboardDataDto(decimal RevenueToday, int OrdersToday, int LowStockCount, int AttendanceToday, IReadOnlyList<RevenuePointDto> RevenuePoints, IReadOnlyList<TopProductDto> TopProducts, IReadOnlyList<LowStockDto> LowStockProducts, IReadOnlyList<RecentOrderDto> RecentOrders);
public sealed record SalesReportDto(decimal TotalRevenue, int TotalOrders, decimal AverageOrderValue, int UnitsSold, IReadOnlyList<RevenuePointDto> RevenuePoints, IReadOnlyList<TopProductDto> TopProducts, IReadOnlyList<CategorySalesDto> CategorySales);
