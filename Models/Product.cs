namespace SmartPOS.Models;

public class Product
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string? CaseBarcode { get; set; }
    public string RetailUnitName { get; set; } = "Cái";
    public string ImportUnitName { get; set; } = "Thùng";
    public int UnitsPerImportUnit { get; set; } = 1;
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
    public int MinStockAlert { get; set; }
    public string? ImagePath { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}
