using SmartPOS.Models.Enums;
using SmartPOS.Services.Dtos;

namespace SmartPOS.Services;

public interface IInventoryService
{
    Task<IReadOnlyList<InventoryItem>> GetInventoryAsync(string? search, StockFilter stockFilter);
    Task<IReadOnlyList<StockTransactionItem>> GetRecentTransactionsAsync(int take = 8);
    Task<InventoryItem?> GetInventoryItemByCaseBarcodeAsync(string barcode);
    Task ImportStockAsync(int productId, int quantity);
    Task ImportStockAsync(StockReceiptRequest request);
}
