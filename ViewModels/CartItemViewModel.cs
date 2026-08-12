using CommunityToolkit.Mvvm.ComponentModel;
using SmartPOS.Services.Dtos;
namespace SmartPOS.ViewModels;
public partial class CartItemViewModel(SaleProductItem product) : ObservableObject
{
    public int ProductId { get; } = product.ProductId;
    public string ProductName { get; } = product.ProductName;
    public decimal UnitPrice { get; } = product.SellingPrice;
    public int AvailableStock { get; private set; } = product.StockQuantity;
    [ObservableProperty] private int quantity = 1;
    public decimal LineTotal => UnitPrice * Quantity;
    public bool CanIncrease => Quantity < AvailableStock;
    public void UpdateAvailableStock(int stock) { AvailableStock = stock; OnPropertyChanged(nameof(AvailableStock)); OnPropertyChanged(nameof(CanIncrease)); }
    public bool Increase() { if (!CanIncrease) return false; Quantity++; return true; }
    public bool Decrease() { if (Quantity <= 1) return false; Quantity--; return true; }
    partial void OnQuantityChanged(int value) { OnPropertyChanged(nameof(LineTotal)); OnPropertyChanged(nameof(CanIncrease)); }
}
