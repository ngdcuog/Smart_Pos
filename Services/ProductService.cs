using Microsoft.EntityFrameworkCore;
using SmartPOS.Data;
using SmartPOS.Models;
using SmartPOS.Models.Enums;
using SmartPOS.Services.Dtos;

namespace SmartPOS.Services;

public sealed class ProductService(IDbContextFactory<AppDbContext> contextFactory, IProductImageStorage imageStorage) : IProductService
{
    public async Task<IReadOnlyList<ProductListItem>> GetProductsAsync(string? search, int? categoryId, StockFilter stockFilter)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var query = context.Products.AsNoTracking().Include(x => x.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => EF.Functions.Like(x.ProductName, $"%{term}%") || EF.Functions.Like(x.Barcode, $"%{term}%"));
        }

        if (categoryId.HasValue) query = query.Where(x => x.CategoryId == categoryId.Value);
        query = stockFilter switch
        {
            StockFilter.LowStock => query.Where(x => x.StockQuantity > 0 && x.StockQuantity <= x.MinStockAlert),
            StockFilter.OutOfStock => query.Where(x => x.StockQuantity == 0),
            _ => query
        };

        return await query.OrderBy(x => x.ProductName).Select(x => new ProductListItem(x.ProductId, x.CategoryId, x.ProductName,
            x.Barcode, x.Category.CategoryName, x.CostPrice, x.SellingPrice, x.StockQuantity, x.MinStockAlert,
            x.StockQuantity == 0 ? "Hết hàng" : x.StockQuantity <= x.MinStockAlert ? "Sắp hết" : "Đủ hàng", x.ImagePath,
            x.RetailUnitName, x.ImportUnitName, x.UnitsPerImportUnit, x.CaseBarcode)).ToListAsync();
    }

    public async Task<IReadOnlyList<CategoryOption>> GetCategoriesAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var categories = await context.Categories.AsNoTracking().OrderBy(x => x.CategoryName)
            .Select(x => new CategoryOption(x.CategoryId, x.CategoryName)).ToListAsync();
        categories.Insert(0, new CategoryOption(null, "Tất cả danh mục"));
        return categories;
    }

    public async Task CreateProductAsync(ProductInput input)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await ValidateAsync(context, input, null);
        context.Products.Add(new Product
        {
            CategoryId = input.CategoryId,
            ProductName = input.ProductName.Trim(),
            Barcode = input.Barcode.Trim(),
            CaseBarcode = NormalizeOptionalBarcode(input.CaseBarcode),
            RetailUnitName = input.RetailUnitName.Trim(),
            ImportUnitName = input.ImportUnitName.Trim(),
            UnitsPerImportUnit = input.UnitsPerImportUnit,
            CostPrice = input.CostPrice,
            SellingPrice = input.SellingPrice,
            StockQuantity = input.StockQuantity,
            MinStockAlert = input.MinStockAlert,
            ImagePath = input.ImagePath
        });
        await context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<SaleProductItem>> GetSaleProductsAsync(string? search)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var query = context.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) { var term = search.Trim(); query = query.Where(x => EF.Functions.Like(x.ProductName, $"%{term}%") || EF.Functions.Like(x.Barcode, $"%{term}%")); }
        return await query.OrderBy(x => x.ProductName).Take(50).Select(x => new SaleProductItem(x.ProductId, x.ProductName,
            x.Barcode, x.SellingPrice, x.StockQuantity, x.MinStockAlert,
            x.StockQuantity == 0 ? "Hết hàng" : x.StockQuantity <= x.MinStockAlert ? "Sắp hết" : "Đủ hàng",
            x.ImagePath, x.RetailUnitName)).ToListAsync();
    }

    public async Task<SaleProductItem?> GetSaleProductByBarcodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return null;
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Products.AsNoTracking().Where(x => x.Barcode == barcode.Trim()).Select(x => new SaleProductItem(x.ProductId,
            x.ProductName, x.Barcode, x.SellingPrice, x.StockQuantity, x.MinStockAlert,
            x.StockQuantity == 0 ? "Hết hàng" : x.StockQuantity <= x.MinStockAlert ? "Sắp hết" : "Đủ hàng",
            x.ImagePath, x.RetailUnitName)).SingleOrDefaultAsync();
    }

    public async Task<bool> IsCaseBarcodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return false;
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Products.AsNoTracking().AnyAsync(x => x.CaseBarcode == barcode.Trim());
    }

    public async Task UpdateProductAsync(ProductInput input)
    {
        if (!input.ProductId.HasValue) throw new ProductServiceException("Không tìm thấy sản phẩm.");
        await using var context = await contextFactory.CreateDbContextAsync();
        var product = await context.Products.FindAsync(input.ProductId.Value) ?? throw new ProductServiceException("Không tìm thấy sản phẩm.");
        await ValidateAsync(context, input, product.ProductId);
        product.CategoryId = input.CategoryId;
        product.ProductName = input.ProductName.Trim();
        product.Barcode = input.Barcode.Trim();
        product.CaseBarcode = NormalizeOptionalBarcode(input.CaseBarcode);
        product.RetailUnitName = input.RetailUnitName.Trim();
        product.ImportUnitName = input.ImportUnitName.Trim();
        product.UnitsPerImportUnit = input.UnitsPerImportUnit;
        product.CostPrice = input.CostPrice;
        product.SellingPrice = input.SellingPrice;
        product.MinStockAlert = input.MinStockAlert;
        var oldImagePath = product.ImagePath;
        product.ImagePath = input.ImagePath;
        // Stock quantity is intentionally adjusted only by InventoryService after creation.
        await context.SaveChangesAsync();
        if (!string.Equals(oldImagePath, product.ImagePath, StringComparison.OrdinalIgnoreCase)) imageStorage.DeleteManagedImage(oldImagePath);
    }

    private static async Task ValidateAsync(AppDbContext context, ProductInput input, int? existingProductId)
    {
        if (string.IsNullOrWhiteSpace(input.ProductName)) throw new ProductServiceException("Tên sản phẩm là bắt buộc.");
        if (string.IsNullOrWhiteSpace(input.Barcode)) throw new ProductServiceException("Mã vạch là bắt buộc.");
        if (input.CostPrice < 0 || input.SellingPrice < 0 || input.StockQuantity < 0 || input.MinStockAlert < 0)
            throw new ProductServiceException("Giá và số lượng tồn kho không được nhỏ hơn 0.");
        if (string.IsNullOrWhiteSpace(input.RetailUnitName) || string.IsNullOrWhiteSpace(input.ImportUnitName))
            throw new ProductServiceException("Tên đơn vị bán lẻ và đơn vị nhập là bắt buộc.");
        if (input.UnitsPerImportUnit < 1)
            throw new ProductServiceException("Số đơn vị lẻ trong mỗi đơn vị nhập phải lớn hơn hoặc bằng 1.");
        if (!await context.Categories.AnyAsync(x => x.CategoryId == input.CategoryId)) throw new ProductServiceException("Danh mục không tồn tại.");
        var barcode = input.Barcode.Trim();
        var caseBarcode = NormalizeOptionalBarcode(input.CaseBarcode);
        if (caseBarcode == barcode)
            throw new ProductServiceException("Mã vạch bán lẻ và mã vạch thùng phải khác nhau.");
        if (await context.Products.AnyAsync(x => (x.Barcode == barcode || x.CaseBarcode == barcode) && x.ProductId != existingProductId))
            throw new ProductServiceException("Mã vạch này đã tồn tại. Vui lòng sử dụng mã khác.");
        if (caseBarcode is not null && await context.Products.AnyAsync(x => (x.Barcode == caseBarcode || x.CaseBarcode == caseBarcode) && x.ProductId != existingProductId))
            throw new ProductServiceException("Mã vạch thùng này đã tồn tại. Vui lòng sử dụng mã khác.");
    }

    private static string? NormalizeOptionalBarcode(string? barcode) => string.IsNullOrWhiteSpace(barcode) ? null : barcode.Trim();
}
