namespace SmartPOS.Services.Dtos;

public sealed record InventoryItem(int ProductId, string ProductName, string CategoryName, int StockQuantity,
    int MinStockAlert, string StockStatus, string RetailUnitName = "Cái", string ImportUnitName = "Thùng",
    int UnitsPerImportUnit = 1, string? CaseBarcode = null);
