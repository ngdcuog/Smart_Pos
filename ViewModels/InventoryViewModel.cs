using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartPOS.Models.Enums;
using SmartPOS.Services;
using SmartPOS.Services.Dtos;
using System.Windows;

namespace SmartPOS.ViewModels;

public partial class InventoryViewModel(IInventoryService inventoryService) : PlaceholderViewModel("Kho hàng", "Theo dõi tồn kho và các giao dịch nhập hàng.", string.Empty), IAsyncInitializable
{
    public ObservableCollection<InventoryItem> InventoryItems { get; } = [];
    public ObservableCollection<InventoryItem> ImportProductOptions { get; } = [];
    public ObservableCollection<StockTransactionItem> RecentTransactions { get; } = [];
    public IReadOnlyList<StockFilterOption> StockFilters { get; } =
    [new(StockFilter.All, "Tất cả tồn kho"), new(StockFilter.LowStock, "Sắp hết"), new(StockFilter.OutOfStock, "Hết hàng")];

    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private StockFilterOption? selectedStockFilter;
    [ObservableProperty] private InventoryItem? selectedInventoryItem;
    [ObservableProperty] private string caseBarcodeInput = string.Empty;
    [ObservableProperty] private string importUnitQuantityText = string.Empty;
    [ObservableProperty] private string looseUnitQuantityText = string.Empty;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isImporting;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string? successMessage;
    [ObservableProperty] private int totalProducts;
    [ObservableProperty] private int lowStockCount;
    [ObservableProperty] private int outOfStockCount;
    [ObservableProperty] private bool hasInventoryItems;
    [ObservableProperty] private bool hasRecentTransactions;
    public Visibility InventoryEmptyVisibility => IsLoading || HasInventoryItems ? Visibility.Collapsed : Visibility.Visible;
    public Visibility RecentTransactionsEmptyVisibility => IsLoading || HasRecentTransactions ? Visibility.Collapsed : Visibility.Visible;
    public string PackagingHint => SelectedInventoryItem is null
        ? "Chọn sản phẩm để xem quy đổi đơn vị."
        : $"1 {SelectedInventoryItem.ImportUnitName} = {SelectedInventoryItem.UnitsPerImportUnit:N0} {SelectedInventoryItem.RetailUnitName.ToLowerInvariant()}";
    public string ImportUnitInputLabel => SelectedInventoryItem is null ? "Số thùng / kiện" : $"Số {SelectedInventoryItem.ImportUnitName.ToLowerInvariant()}";
    public string LooseUnitInputLabel => SelectedInventoryItem is null ? "Số lẻ" : $"Số {SelectedInventoryItem.RetailUnitName.ToLowerInvariant()} lẻ";
    public string ReceiptSummary
    {
        get
        {
            if (SelectedInventoryItem is null || !int.TryParse(ImportUnitQuantityText, out var importUnits) || importUnits < 0
                || !int.TryParse(LooseUnitQuantityText, out var looseUnits) || looseUnits < 0) return string.Empty;
            var total = importUnits * SelectedInventoryItem.UnitsPerImportUnit + looseUnits;
            return total > 0 ? $"Tồn sẽ tăng {total:N0} {SelectedInventoryItem.RetailUnitName.ToLowerInvariant()}." : string.Empty;
        }
    }

    public async Task InitializeAsync()
    {
        SelectedStockFilter ??= StockFilters[0];
        await LoadInventoryAsync();
    }

    [RelayCommand]
    private async Task LoadInventoryAsync()
    {
        try
        {
            IsLoading = true; ErrorMessage = null;
            var inventory = await inventoryService.GetInventoryAsync(SearchText, SelectedStockFilter?.Value ?? StockFilter.All);
            var summaryInventory = await inventoryService.GetInventoryAsync(null, StockFilter.All);
            var recentTransactions = await inventoryService.GetRecentTransactionsAsync();
            InventoryItems.Clear(); foreach (var item in inventory) InventoryItems.Add(item);
            HasInventoryItems = InventoryItems.Count > 0;
            ImportProductOptions.Clear(); foreach (var item in summaryInventory) ImportProductOptions.Add(item);
            RecentTransactions.Clear(); foreach (var item in recentTransactions) RecentTransactions.Add(item);
            HasRecentTransactions = RecentTransactions.Count > 0;
            TotalProducts = summaryInventory.Count;
            LowStockCount = summaryInventory.Count(x => x.StockStatus == "Sắp hết");
            OutOfStockCount = summaryInventory.Count(x => x.StockStatus == "Hết hàng");
        }
        catch { ErrorMessage = "Không thể tải dữ liệu kho hàng. Vui lòng thử lại."; }
        finally { IsLoading = false; }
    }

    [RelayCommand] private Task RefreshAsync() => LoadInventoryAsync();
    partial void OnHasInventoryItemsChanged(bool value) => OnPropertyChanged(nameof(InventoryEmptyVisibility));
    partial void OnHasRecentTransactionsChanged(bool value) => OnPropertyChanged(nameof(RecentTransactionsEmptyVisibility));
    partial void OnIsLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(InventoryEmptyVisibility));
        OnPropertyChanged(nameof(RecentTransactionsEmptyVisibility));
    }

    [RelayCommand]
    private void OpenImportStock()
    {
        ErrorMessage = null; SuccessMessage = null;
        if (SelectedInventoryItem is null) SelectedInventoryItem = ImportProductOptions.FirstOrDefault();
    }

    [RelayCommand]
    private async Task FindByCaseBarcodeAsync()
    {
        var barcode = CaseBarcodeInput.Trim();
        if (string.IsNullOrWhiteSpace(barcode)) return;

        try
        {
            ErrorMessage = null;
            SuccessMessage = null;
            var item = await inventoryService.GetInventoryItemByCaseBarcodeAsync(barcode);
            if (item is null)
            {
                ErrorMessage = "Không tìm thấy sản phẩm với mã vạch thùng này.";
                return;
            }

            SelectedInventoryItem = ImportProductOptions.FirstOrDefault(x => x.ProductId == item.ProductId) ?? item;
            CaseBarcodeInput = string.Empty;
            SuccessMessage = $"Đã chọn {item.ProductName}. {PackagingHint}";
        }
        catch
        {
            ErrorMessage = "Không thể xử lý mã vạch thùng. Vui lòng quét lại.";
        }
    }

    [RelayCommand]
    private async Task ConfirmImportStockAsync()
    {
        if (SelectedInventoryItem is null) { ErrorMessage = "Vui lòng chọn sản phẩm cần nhập."; return; }
        if (!int.TryParse(ImportUnitQuantityText, out var importUnitQuantity) || importUnitQuantity < 0
            || !int.TryParse(LooseUnitQuantityText, out var looseUnitQuantity) || looseUnitQuantity < 0)
        {
            ErrorMessage = "Số lượng thùng và số lượng lẻ phải là số không âm.";
            return;
        }
        try
        {
            IsImporting = true; ErrorMessage = null; SuccessMessage = null;
            var summary = ReceiptSummary;
            await inventoryService.ImportStockAsync(new StockReceiptRequest(SelectedInventoryItem.ProductId, importUnitQuantity, looseUnitQuantity));
            SuccessMessage = string.IsNullOrWhiteSpace(summary) ? "Nhập kho thành công." : $"Nhập kho thành công. {summary}";
            ImportUnitQuantityText = LooseUnitQuantityText = string.Empty;
            await LoadInventoryAsync();
        }
        catch (ProductServiceException ex) { ErrorMessage = ex.Message; }
        catch { ErrorMessage = "Không thể nhập kho. Vui lòng thử lại."; }
        finally { IsImporting = false; }
    }

    partial void OnSelectedInventoryItemChanged(InventoryItem? value)
    {
        OnPropertyChanged(nameof(PackagingHint));
        OnPropertyChanged(nameof(ImportUnitInputLabel));
        OnPropertyChanged(nameof(LooseUnitInputLabel));
        OnPropertyChanged(nameof(ReceiptSummary));
    }

    partial void OnImportUnitQuantityTextChanged(string value) => OnPropertyChanged(nameof(ReceiptSummary));
    partial void OnLooseUnitQuantityTextChanged(string value) => OnPropertyChanged(nameof(ReceiptSummary));
}
