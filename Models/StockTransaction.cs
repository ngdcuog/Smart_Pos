using SmartPOS.Models.Enums;

namespace SmartPOS.Models;

public class StockTransaction
{
    public int StockTransactionId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public StockTransactionType Type { get; set; }
    public DateTime TransactionDate { get; set; }
    public int? ImportUnitQuantity { get; set; }
    public int? LooseUnitQuantity { get; set; }
    public int? UnitsPerImportUnitSnapshot { get; set; }
    public string? ImportUnitNameSnapshot { get; set; }
    public decimal? UnitCostSnapshot { get; set; }
    public Product Product { get; set; } = null!;
}
