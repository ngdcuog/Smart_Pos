using Microsoft.EntityFrameworkCore;
using SmartPOS.Data;
using SmartPOS.Models.Enums;
using SmartPOS.Services;
using SmartPOS.Services.Dtos;

namespace SmartPOS.Tests;

public sealed class ReportServiceTests : IAsyncLifetime
{
    private const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=SmartPOSPhase8Tests;Trusted_Connection=True;TrustServerCertificate=True";
    private readonly DbContextOptions<AppDbContext> _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(ConnectionString).Options;

    public async Task InitializeAsync()
    {
        await using var context = new AppDbContext(_options);
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var context = new AppDbContext(_options);
        await context.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task Dashboard_Returns_Todays_Revenue_And_Seven_Day_Points()
    {
        var orders = new OrderService(new Factory(_options));
        await orders.CreateOrderAsync(new CreateOrderRequest(2, 0, PaymentMethod.Cash, [new CreateOrderItemRequest(1, 2)]));

        var dashboard = await new ReportService(new Factory(_options)).GetDashboardAsync();

        Assert.Equal(14_000m, dashboard.RevenueToday);
        Assert.Equal(1, dashboard.OrdersToday);
        Assert.Equal(7, dashboard.RevenuePoints.Count);
        Assert.Contains(dashboard.RevenuePoints, point => point.Date.Date == DateTime.Today && point.Revenue == 14_000m);
    }

    private sealed class Factory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);
        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(CreateDbContext());
    }
}
