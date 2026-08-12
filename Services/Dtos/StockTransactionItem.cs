namespace SmartPOS.Services.Dtos;

public sealed record StockTransactionItem(DateTime TransactionDate, string ProductName, string Type, int Quantity,
    string? ReceiptDescription = null)
{
    public string QuantityDisplay => string.IsNullOrWhiteSpace(ReceiptDescription) ? $"SL: {Quantity}" : ReceiptDescription;
}
