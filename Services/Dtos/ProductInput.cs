namespace SmartPOS.Services.Dtos;

public sealed record ProductInput(int? ProductId, int CategoryId, string ProductName, string Barcode,
    decimal CostPrice, decimal SellingPrice, int StockQuantity, int MinStockAlert, string? ImagePath = null,
    string RetailUnitName = "Cái", string ImportUnitName = "Thùng", int UnitsPerImportUnit = 1,
    string? CaseBarcode = null);
