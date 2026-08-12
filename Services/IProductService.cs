using SmartPOS.Models.Enums;
using SmartPOS.Services.Dtos;

namespace SmartPOS.Services;

public interface IProductService
{
    Task<IReadOnlyList<ProductListItem>> GetProductsAsync(string? search, int? categoryId, StockFilter stockFilter);
    Task<IReadOnlyList<CategoryOption>> GetCategoriesAsync();
    Task CreateProductAsync(ProductInput input);
    Task UpdateProductAsync(ProductInput input);
    Task<IReadOnlyList<SaleProductItem>> GetSaleProductsAsync(string? search);
    Task<SaleProductItem?> GetSaleProductByBarcodeAsync(string barcode);
    Task<bool> IsCaseBarcodeAsync(string barcode);
}
