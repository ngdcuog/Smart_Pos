using Microsoft.EntityFrameworkCore;
using SmartPOS.Data;
using SmartPOS.Models.Enums;
using SmartPOS.Services;
using SmartPOS.Services.Dtos;

namespace SmartPOS.Tests;

public sealed class ProductAndInventoryServiceTests : IAsyncLifetime
{
    private const string ConnectionString = "Server=(localdb)\\MSSQLLocalDB;Database=SmartPOSPhase4Tests;Trusted_Connection=True;TrustServerCertificate=True";
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
    public async Task CreateProductAsync_RejectsDuplicateBarcode()
    {
        var service = new ProductService(new TestDbContextFactory(_options), new TestImageStorage());
        var input = new ProductInput(null, 1, "Sản phẩm kiểm thử", "8934588012221", 1000m, 2000m, 3, 1);

        var exception = await Assert.ThrowsAsync<ProductServiceException>(() => service.CreateProductAsync(input));

        Assert.Equal("Mã vạch này đã tồn tại. Vui lòng sử dụng mã khác.", exception.Message);
    }

    [Fact]
    public async Task ImportStockAsync_IncrementsStockAndCreatesTransaction()
    {
        var factory = new TestDbContextFactory(_options);
        var service = new InventoryService(factory);
        await using var beforeContext = factory.CreateDbContext();
        var before = await beforeContext.Products.Where(x => x.ProductId == 3).Select(x => x.StockQuantity).SingleAsync();

        await service.ImportStockAsync(3, 7);

        await using var afterContext = factory.CreateDbContext();
        var product = await afterContext.Products.SingleAsync(x => x.ProductId == 3);
        var transaction = await afterContext.StockTransactions.OrderByDescending(x => x.StockTransactionId).FirstAsync(x => x.ProductId == 3);
        Assert.Equal(before + 7, product.StockQuantity);
        Assert.Equal(7, transaction.Quantity);
        Assert.Equal("Import", transaction.Type.ToString());
    }

    [Fact]
    public async Task ProductService_CreatesUpdatesAndFiltersProducts()
    {
        var service = new ProductService(new TestDbContextFactory(_options), new TestImageStorage());
        var input = new ProductInput(null, 1, "Nước kiểm thử", "8934588099999", 5000m, 9000m, 6, 2);
        await service.CreateProductAsync(input);

        var bySearch = await service.GetProductsAsync("8099999", null, StockFilter.All);
        var created = Assert.Single(bySearch);
        Assert.Equal("Nước kiểm thử", created.ProductName);

        await service.UpdateProductAsync(new ProductInput(created.ProductId, 2, "Nước kiểm thử mới", "8934588099999", 6000m, 10000m, 999, 3));
        var afterUpdate = Assert.Single(await service.GetProductsAsync("kiểm thử mới", 2, StockFilter.All));
        Assert.Equal(6, afterUpdate.StockQuantity);
        Assert.Equal(3, afterUpdate.MinStockAlert);
    }

    [Fact]
    public async Task ImportStockAsync_RejectsNonPositiveQuantityWithoutChangingStock()
    {
        var factory = new TestDbContextFactory(_options);
        var service = new InventoryService(factory);
        await using var beforeContext = factory.CreateDbContext();
        var before = await beforeContext.Products.Where(x => x.ProductId == 3).Select(x => x.StockQuantity).SingleAsync();

        await Assert.ThrowsAsync<ProductServiceException>(() => service.ImportStockAsync(3, 0));

        await using var afterContext = factory.CreateDbContext();
        var after = await afterContext.Products.Where(x => x.ProductId == 3).Select(x => x.StockQuantity).SingleAsync();
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task ImportStockAsync_ConvertsCasesAndLooseRetailUnitsAndCapturesReceiptSnapshot()
    {
        var factory = new TestDbContextFactory(_options);
        var service = new InventoryService(factory);
        await using var beforeContext = factory.CreateDbContext();
        var before = await beforeContext.Products.Where(x => x.ProductId == 3).Select(x => x.StockQuantity).SingleAsync();

        await service.ImportStockAsync(new StockReceiptRequest(3, 2, 3, 7250m));

        await using var afterContext = factory.CreateDbContext();
        var product = await afterContext.Products.SingleAsync(x => x.ProductId == 3);
        var transaction = await afterContext.StockTransactions.OrderByDescending(x => x.StockTransactionId).FirstAsync(x => x.ProductId == 3);
        Assert.Equal(before + 51, product.StockQuantity);
        Assert.Equal(51, transaction.Quantity);
        Assert.Equal(2, transaction.ImportUnitQuantity);
        Assert.Equal(3, transaction.LooseUnitQuantity);
        Assert.Equal(24, transaction.UnitsPerImportUnitSnapshot);
        Assert.Equal("Thùng", transaction.ImportUnitNameSnapshot);
        Assert.Equal(7250m, transaction.UnitCostSnapshot);
    }

    [Fact]
    public async Task ImportStockAsync_RejectsLooseQuantityAtOrAboveOneCase()
    {
        var factory = new TestDbContextFactory(_options);
        var service = new InventoryService(factory);
        await using var beforeContext = factory.CreateDbContext();
        var before = await beforeContext.Products.Where(x => x.ProductId == 3).Select(x => x.StockQuantity).SingleAsync();

        await Assert.ThrowsAsync<ProductServiceException>(() => service.ImportStockAsync(new StockReceiptRequest(3, 0, 24)));

        await using var afterContext = factory.CreateDbContext();
        var after = await afterContext.Products.Where(x => x.ProductId == 3).Select(x => x.StockQuantity).SingleAsync();
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task ProductService_SeparatesRetailAndCaseBarcodeNamespaces()
    {
        var service = new ProductService(new TestDbContextFactory(_options), new TestImageStorage());

        Assert.True(await service.IsCaseBarcodeAsync("18934588012230"));
        Assert.Null(await service.GetSaleProductByBarcodeAsync("18934588012230"));
        await Assert.ThrowsAsync<ProductServiceException>(() => service.CreateProductAsync(
            new ProductInput(null, 1, "Sản phẩm mã thùng trùng", "8934588099988", 1000m, 2000m, 0, 1,
                CaseBarcode: "8934588012223")));
    }

    private sealed class TestDbContextFactory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => new(options);
        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(CreateDbContext());
    }
    private sealed class TestImageStorage : IProductImageStorage { public Task<string> CopyFromAsync(string sourcePath, CancellationToken cancellationToken = default) => Task.FromResult(sourcePath); public void DeleteManagedImage(string? path) { } }
}
