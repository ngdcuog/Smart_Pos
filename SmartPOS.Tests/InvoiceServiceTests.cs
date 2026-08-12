using Microsoft.EntityFrameworkCore;
using SmartPOS.Data;
using SmartPOS.Models.Enums;
using SmartPOS.Services;
using SmartPOS.Services.Dtos;

namespace SmartPOS.Tests;

public sealed class InvoiceServiceTests : IAsyncLifetime
{
    private const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=SmartPOSInvoiceTests;Trusted_Connection=True;TrustServerCertificate=True";
    private readonly DbContextOptions<AppDbContext> _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(ConnectionString).Options;
    private readonly string _outputDirectory = Path.Combine(Path.GetTempPath(), "SmartPOSTests", Guid.NewGuid().ToString("N"));

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
        if (Directory.Exists(_outputDirectory))
            Directory.Delete(_outputDirectory, recursive: true);
    }

    [Fact]
    public async Task GeneratePdfAsync_CreatesPdfForPersistedOrder()
    {
        var orderService = new OrderService(new Factory(_options));
        var receipt = await orderService.CreateOrderAsync(new CreateOrderRequest(2, 1_000m, PaymentMethod.Cash, [new CreateOrderItemRequest(1, 2)]));
        var invoiceService = new InvoiceService(new Factory(_options), new InvoicePathProvider(_outputDirectory));

        var result = await invoiceService.GeneratePdfAsync(receipt.OrderId);

        Assert.Equal(receipt.OrderId, result.OrderId);
        Assert.True(File.Exists(result.FilePath));
        Assert.True(new FileInfo(result.FilePath).Length > 1_000);
        await using var stream = File.OpenRead(result.FilePath);
        var header = new byte[5];
        _ = await stream.ReadAsync(header);
        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(header));
    }

    private sealed class Factory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);
        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(CreateDbContext());
    }
}
