namespace SmartPOS.Services.Dtos;

public sealed record ProductListItem(int ProductId, int CategoryId, string ProductName, string Barcode, string CategoryName,
    decimal CostPrice, decimal SellingPrice, int StockQuantity, int MinStockAlert, string StockStatus, string? ImagePath,
    string RetailUnitName = "Cái", string ImportUnitName = "Thùng", int UnitsPerImportUnit = 1,
    string? CaseBarcode = null);
