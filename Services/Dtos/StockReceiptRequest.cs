namespace SmartPOS.Services.Dtos;

/// <summary>
/// A stock receipt expressed in purchase packaging. Stock remains stored in retail units.
/// </summary>
public sealed record StockReceiptRequest(int ProductId, int ImportUnitQuantity, int LooseUnitQuantity,
    decimal? UnitCost = null);
