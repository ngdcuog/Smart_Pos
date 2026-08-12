using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SmartPOS.Models.Enums;
using SmartPOS.Services;
using SmartPOS.Services.Dtos;
using System.Windows;

namespace SmartPOS.ViewModels;

public partial class ProductManagementViewModel(IProductService productService, IProductImageStorage imageStorage) : PlaceholderViewModel("Sản phẩm", "Quản lý danh mục, thông tin và trạng thái sản phẩm.", string.Empty), IAsyncInitializable
{
    public ObservableCollection<ProductListItem> Products { get; } = [];
    public ObservableCollection<CategoryOption> Categories { get; } = [];
    public IEnumerable<CategoryOption> EditorCategories => Categories.Where(x => x.CategoryId.HasValue);
    public IReadOnlyList<StockFilterOption> StockFilters { get; } =
    [new(StockFilter.All, "Tất cả tồn kho"), new(StockFilter.LowStock, "Sắp hết"), new(StockFilter.OutOfStock, "Hết hàng")];

    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private CategoryOption? selectedCategory;
    [ObservableProperty] private StockFilterOption? selectedStockFilter;
    [ObservableProperty] private ProductListItem? selectedProduct;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string? successMessage;
    [ObservableProperty] private bool hasError;
    [ObservableProperty] private bool hasSuccess;
    [ObservableProperty] private bool hasProducts;
    [ObservableProperty] private bool isEditing;
    [ObservableProperty] private bool isNewProduct;
    [ObservableProperty] private string editorTitle = "Thông tin sản phẩm";
    [ObservableProperty] private string productName = string.Empty;
    [ObservableProperty] private CategoryOption? editorCategory;
    [ObservableProperty] private string barcode = string.Empty;
    [ObservableProperty] private string costPriceText = "0";
    [ObservableProperty] private string sellingPriceText = "0";
    [ObservableProperty] private string stockQuantityText = "0";
    [ObservableProperty] private string minStockAlertText = "0";
    [ObservableProperty] private string retailUnitName = "Cái";
    [ObservableProperty] private string importUnitName = "Thùng";
    [ObservableProperty] private string unitsPerImportUnitText = "1";
    [ObservableProperty] private string caseBarcode = string.Empty;
    [ObservableProperty] private string? selectedImageSourcePath;
    [ObservableProperty] private string? imagePath;
    public Visibility ProductsEmptyVisibility => IsLoading || HasProducts ? Visibility.Collapsed : Visibility.Visible;

    public async Task InitializeAsync()
    {
        SelectedStockFilter ??= StockFilters[0];
        if (Categories.Count == 0)
        {
            foreach (var category in await productService.GetCategoriesAsync()) Categories.Add(category);
            OnPropertyChanged(nameof(EditorCategories));
            SelectedCategory = Categories.FirstOrDefault();
        }
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        try
        {
            IsLoading = true; ErrorMessage = null;
            var products = await productService.GetProductsAsync(SearchText, SelectedCategory?.CategoryId, SelectedStockFilter?.Value ?? StockFilter.All);
            Products.Clear(); foreach (var product in products) Products.Add(product);
            HasProducts = Products.Count > 0;
        }
        catch { ErrorMessage = "Không thể tải danh sách sản phẩm. Vui lòng thử lại."; }
        finally { IsLoading = false; }
    }

    [RelayCommand] private Task RefreshAsync() => LoadProductsAsync();

    [RelayCommand]
    private void AddProduct()
    {
        IsNewProduct = true; IsEditing = true; EditorTitle = "Thêm sản phẩm"; ErrorMessage = null; SuccessMessage = null;
        ProductName = Barcode = CaseBarcode = string.Empty; CostPriceText = SellingPriceText = StockQuantityText = MinStockAlertText = "0";
        RetailUnitName = "Cái"; ImportUnitName = "Thùng"; UnitsPerImportUnitText = "1";
        SelectedImageSourcePath = ImagePath = null;
        EditorCategory = Categories.FirstOrDefault(x => x.CategoryId.HasValue);
        SelectedProduct = null;
    }

    partial void OnSelectedProductChanged(ProductListItem? value)
    {
        if (value is null || IsSaving) return;
        IsNewProduct = false; IsEditing = true; EditorTitle = "Chỉnh sửa sản phẩm"; ErrorMessage = null; SuccessMessage = null;
        ProductName = value.ProductName; Barcode = value.Barcode; CostPriceText = value.CostPrice.ToString("0.##"); SellingPriceText = value.SellingPrice.ToString("0.##");
        StockQuantityText = value.StockQuantity.ToString(); MinStockAlertText = value.MinStockAlert.ToString();
        RetailUnitName = value.RetailUnitName; ImportUnitName = value.ImportUnitName;
        UnitsPerImportUnitText = value.UnitsPerImportUnit.ToString(); CaseBarcode = value.CaseBarcode ?? string.Empty;
        EditorCategory = Categories.FirstOrDefault(x => x.CategoryId == value.CategoryId);
        ImagePath = value.ImagePath;
        SelectedImageSourcePath = null;
    }

    [RelayCommand]
    private async Task SaveProductAsync()
    {
        if (EditorCategory?.CategoryId is not int categoryId || !TryReadInput(categoryId, out var input)) return;
        try
        {
            IsSaving = true; ErrorMessage = null; SuccessMessage = null;
            string? managedImagePath = ImagePath;
            if (!string.IsNullOrWhiteSpace(SelectedImageSourcePath)) managedImagePath = await imageStorage.CopyFromAsync(SelectedImageSourcePath);
            input = input with { ImagePath = managedImagePath };
            if (IsNewProduct) await productService.CreateProductAsync(input); else await productService.UpdateProductAsync(input);
            SuccessMessage = IsNewProduct ? "Đã thêm sản phẩm." : "Đã cập nhật sản phẩm.";
            IsEditing = false; await LoadProductsAsync();
        }
        catch (ProductServiceException ex) { ErrorMessage = ex.Message; }
        catch { ErrorMessage = "Không thể lưu sản phẩm. Vui lòng thử lại."; }
        finally { IsSaving = false; }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false; ErrorMessage = null;
    }

    [RelayCommand]
    private void ChooseImage()
    {
        var dialog = new OpenFileDialog { Filter = "Ảnh sản phẩm (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png", Multiselect = false };
        if (dialog.ShowDialog() == true) { SelectedImageSourcePath = dialog.FileName; ImagePath = dialog.FileName; }
    }

    partial void OnErrorMessageChanged(string? value) => HasError = !string.IsNullOrWhiteSpace(value);
    partial void OnSuccessMessageChanged(string? value) => HasSuccess = !string.IsNullOrWhiteSpace(value);
    partial void OnHasProductsChanged(bool value) => OnPropertyChanged(nameof(ProductsEmptyVisibility));
    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(ProductsEmptyVisibility));

    private bool TryReadInput(int categoryId, out ProductInput input)
    {
        input = default!;
        if (!decimal.TryParse(CostPriceText, out var costPrice) || !decimal.TryParse(SellingPriceText, out var sellingPrice)
            || !int.TryParse(StockQuantityText, out var stockQuantity) || !int.TryParse(MinStockAlertText, out var minStockAlert)
            || !int.TryParse(UnitsPerImportUnitText, out var unitsPerImportUnit))
        { ErrorMessage = "Giá và số lượng phải là số hợp lệ."; return false; }
        input = new ProductInput(IsNewProduct ? null : SelectedProduct?.ProductId, categoryId, ProductName, Barcode,
            costPrice, sellingPrice, stockQuantity, minStockAlert, ImagePath, RetailUnitName, ImportUnitName,
            unitsPerImportUnit, CaseBarcode);
        return true;
    }
}
