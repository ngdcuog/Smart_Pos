namespace SmartPOS.Services.Dtos;
public sealed record SaleProductItem(int ProductId, string ProductName, string Barcode, decimal SellingPrice, int StockQuantity, int MinStockAlert, string StockStatus, string? ImagePath,
    string RetailUnitName = "Cái")
{
    public bool IsInStock => StockQuantity > 0;
}
