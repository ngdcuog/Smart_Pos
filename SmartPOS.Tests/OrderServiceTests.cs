using Microsoft.EntityFrameworkCore;
using SmartPOS.Data;
using SmartPOS.Models.Enums;
using SmartPOS.Services;
using SmartPOS.Services.Dtos;

namespace SmartPOS.Tests;

public sealed class OrderServiceTests : IAsyncLifetime
{
    private const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=SmartPOSPhase5Tests;Trusted_Connection=True;TrustServerCertificate=True";
    private readonly DbContextOptions<AppDbContext> _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(ConnectionString).Options;
    public async Task InitializeAsync() { await using var c = new AppDbContext(_options); await c.Database.EnsureDeletedAsync(); await c.Database.MigrateAsync(); }
    public async Task DisposeAsync() { await using var c = new AppDbContext(_options); await c.Database.EnsureDeletedAsync(); }

    [Fact]
    public async Task Checkout_CreatesOrder_DeductsStock_AndCreatesExport()
    {
        var service = CreateService(); var before = await StockAsync(1);
        var receipt = await service.CreateOrderAsync(new(2, 0, PaymentMethod.Cash, [new(1, 2)]));
        await using var c = new AppDbContext(_options);
        Assert.Equal(before - 2, await StockAsync(1)); Assert.Equal(1, await c.Orders.CountAsync()); Assert.Equal(1, await c.OrderDetails.CountAsync());
        Assert.Equal(2, await c.StockTransactions.Where(x => x.Type == StockTransactionType.Export).Select(x => x.Quantity).SingleAsync()); Assert.Equal(14000m, receipt.FinalAmount);
    }

    [Fact]
    public async Task Checkout_MultipleProducts_AndDuplicateItems_AreMerged()
    {
        var service = CreateService(); var a = await StockAsync(1); var b = await StockAsync(2);
        await service.CreateOrderAsync(new(2, 0, PaymentMethod.BankTransfer, [new(1, 1), new(1, 1), new(2, 3)]));
        await using var c = new AppDbContext(_options);
        Assert.Equal(a - 2, await StockAsync(1)); Assert.Equal(b - 3, await StockAsync(2)); Assert.Equal(2, await c.OrderDetails.CountAsync()); Assert.Equal(2, await c.StockTransactions.CountAsync(x => x.Type == StockTransactionType.Export));
    }

    [Fact]
    public async Task Checkout_InsufficientStock_RollsBackEverything()
    {
        var service = CreateService(); var a = await StockAsync(1); var b = await StockAsync(8);
        await Assert.ThrowsAsync<OrderServiceException>(() => service.CreateOrderAsync(new(2, 0, PaymentMethod.Cash, [new(1, 1), new(8, b + 1)])));
        await using var c = new AppDbContext(_options);
        Assert.Equal(a, await StockAsync(1)); Assert.Equal(b, await StockAsync(8)); Assert.Equal(0, await c.Orders.CountAsync()); Assert.Equal(0, await c.OrderDetails.CountAsync()); Assert.Equal(0, await c.StockTransactions.CountAsync());
    }

    [Fact]
    public async Task Checkout_UsesDatabasePrice_AndValidatesDiscount()
    {
        var service = CreateService();
        var receipt = await service.CreateOrderAsync(new(2, 2000m, PaymentMethod.Cash, [new(2, 2)]));
        await using var c = new AppDbContext(_options);
        var detail = await c.OrderDetails.SingleAsync(); Assert.Equal(12000m, detail.UnitPrice); Assert.Equal(24000m, detail.LineTotal); Assert.Equal(22000m, receipt.FinalAmount);
        await Assert.ThrowsAsync<OrderServiceException>(() => service.CreateOrderAsync(new(2, 999999m, PaymentMethod.Cash, [new(1, 1)])));
        await Assert.ThrowsAsync<OrderServiceException>(() => service.CreateOrderAsync(new(2, -1m, PaymentMethod.Cash, [new(1, 1)])));
    }

    private OrderService CreateService() => new(new Factory(_options));
    private async Task<int> StockAsync(int productId) { await using var c = new AppDbContext(_options); return await c.Products.Where(x => x.ProductId == productId).Select(x => x.StockQuantity).SingleAsync(); }
    private sealed class Factory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext> { public AppDbContext CreateDbContext() => new(options); public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(CreateDbContext()); }
}
