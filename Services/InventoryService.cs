using Microsoft.EntityFrameworkCore;
using SmartPOS.Data;
using SmartPOS.Models;
using SmartPOS.Models.Enums;
using SmartPOS.Services.Dtos;

namespace SmartPOS.Services;

public sealed class InventoryService(IDbContextFactory<AppDbContext> contextFactory) : IInventoryService
{
    public async Task<IReadOnlyList<InventoryItem>> GetInventoryAsync(string? search, StockFilter stockFilter)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var query = context.Products.AsNoTracking().Include(x => x.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => EF.Functions.Like(x.ProductName, $"%{term}%") || EF.Functions.Like(x.Barcode, $"%{term}%"));
        }

        query = stockFilter switch
        {
            StockFilter.LowStock => query.Where(x => x.StockQuantity > 0 && x.StockQuantity <= x.MinStockAlert),
            StockFilter.OutOfStock => query.Where(x => x.StockQuantity == 0),
            _ => query
        };

        return await query.OrderBy(x => x.ProductName).Select(x => new InventoryItem(x.ProductId, x.ProductName,
            x.Category.CategoryName, x.StockQuantity, x.MinStockAlert,
            x.StockQuantity == 0 ? "Hết hàng" : x.StockQuantity <= x.MinStockAlert ? "Sắp hết" : "Đủ hàng",
            x.RetailUnitName, x.ImportUnitName, x.UnitsPerImportUnit, x.CaseBarcode)).ToListAsync();
    }

    public async Task<IReadOnlyList<StockTransactionItem>> GetRecentTransactionsAsync(int take = 8)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var transactions = await context.StockTransactions.AsNoTracking().Include(x => x.Product)
            .OrderByDescending(x => x.TransactionDate).Take(take)
            .Select(x => new
            {
                x.TransactionDate,
                ProductName = x.Product.ProductName,
                x.Type,
                x.Quantity,
                x.ImportUnitQuantity,
                x.LooseUnitQuantity,
                x.ImportUnitNameSnapshot,
                x.UnitsPerImportUnitSnapshot,
                RetailUnitName = x.Product.RetailUnitName
            }).ToListAsync();

        return transactions.Select(x => new StockTransactionItem(x.TransactionDate, x.ProductName,
            x.Type == StockTransactionType.Import ? "Nhập kho" : "Xuất kho", x.Quantity,
            FormatReceiptDescription(x.ImportUnitQuantity, x.LooseUnitQuantity, x.ImportUnitNameSnapshot,
                x.UnitsPerImportUnitSnapshot, x.RetailUnitName))).ToList();
    }

    public async Task<InventoryItem?> GetInventoryItemByCaseBarcodeAsync(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return null;
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Products.AsNoTracking().Include(x => x.Category)
            .Where(x => x.CaseBarcode == barcode.Trim())
            .Select(x => new InventoryItem(x.ProductId, x.ProductName, x.Category.CategoryName, x.StockQuantity,
                x.MinStockAlert, x.StockQuantity == 0 ? "Hết hàng" : x.StockQuantity <= x.MinStockAlert ? "Sắp hết" : "Đủ hàng",
                x.RetailUnitName, x.ImportUnitName, x.UnitsPerImportUnit, x.CaseBarcode))
            .SingleOrDefaultAsync();
    }

    public async Task ImportStockAsync(int productId, int quantity)
    {
        await ImportStockAsync(new StockReceiptRequest(productId, 0, quantity));
    }

    public async Task ImportStockAsync(StockReceiptRequest request)
    {
        if (request.ImportUnitQuantity < 0 || request.LooseUnitQuantity < 0)
            throw new ProductServiceException("Số lượng nhập không được nhỏ hơn 0.");
        if (request.ImportUnitQuantity == 0 && request.LooseUnitQuantity == 0)
            throw new ProductServiceException("Vui lòng nhập ít nhất một thùng hoặc một đơn vị lẻ.");
        if (request.UnitCost is < 0)
            throw new ProductServiceException("Đơn giá nhập không được nhỏ hơn 0.");

        await using var context = await contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var product = await context.Products.SingleOrDefaultAsync(x => x.ProductId == request.ProductId)
            ?? throw new ProductServiceException("Không tìm thấy sản phẩm.");

        if (request.LooseUnitQuantity >= product.UnitsPerImportUnit)
            throw new ProductServiceException($"Số {product.RetailUnitName.ToLowerInvariant()} lẻ phải nhỏ hơn {product.UnitsPerImportUnit}.");

        int addedRetailUnits;
        try
        {
            addedRetailUnits = checked(request.ImportUnitQuantity * product.UnitsPerImportUnit + request.LooseUnitQuantity);
            product.StockQuantity = checked(product.StockQuantity + addedRetailUnits);
        }
        catch (OverflowException)
        {
            throw new ProductServiceException("Số lượng nhập quá lớn.");
        }

        context.StockTransactions.Add(new StockTransaction
        {
            ProductId = request.ProductId,
            Quantity = addedRetailUnits,
            Type = StockTransactionType.Import,
            TransactionDate = DateTime.Now,
            ImportUnitQuantity = request.ImportUnitQuantity,
            LooseUnitQuantity = request.LooseUnitQuantity,
            UnitsPerImportUnitSnapshot = product.UnitsPerImportUnit,
            ImportUnitNameSnapshot = product.ImportUnitName,
            UnitCostSnapshot = request.UnitCost
        });
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private static string? FormatReceiptDescription(int? importUnitQuantity, int? looseUnitQuantity,
        string? importUnitName, int? unitsPerImportUnit, string retailUnitName)
    {
        if (!importUnitQuantity.HasValue && !looseUnitQuantity.HasValue) return null;
        var parts = new List<string>();
        if (importUnitQuantity.GetValueOrDefault() > 0)
            parts.Add($"{importUnitQuantity} {importUnitName ?? "Thùng"}");
        if (looseUnitQuantity.GetValueOrDefault() > 0)
            parts.Add($"{looseUnitQuantity} {retailUnitName} lẻ");
        return parts.Count == 0 ? $"SL: {(importUnitQuantity.GetValueOrDefault() * unitsPerImportUnit.GetValueOrDefault(1) + looseUnitQuantity.GetValueOrDefault())}" : string.Join(" + ", parts);
    }
}
