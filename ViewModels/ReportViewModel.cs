using System.Collections.ObjectModel;using CommunityToolkit.Mvvm.ComponentModel;using CommunityToolkit.Mvvm.Input;using SmartPOS.Services;using SmartPOS.Services.Dtos;using System.Windows;
namespace SmartPOS.ViewModels;
public partial class ReportViewModel(IReportService reports):PlaceholderViewModel("Báo cáo","Phân tích hoạt động bán hàng theo khoảng thời gian.",string.Empty),IAsyncInitializable
{
    public ObservableCollection<RevenuePointDto> RevenuePoints { get; } = [];
    public ObservableCollection<RevenueChartPointViewModel> RevenueChartPoints { get; } = [];
    public ObservableCollection<TopProductDto> TopProducts { get; } = [];
    public ObservableCollection<CategorySalesDto> CategorySales { get; } = [];
    [ObservableProperty] private DateTime fromDate = DateTime.Today.AddDays(-6);
    [ObservableProperty] private DateTime toDate = DateTime.Today;
    [ObservableProperty] private decimal totalRevenue;
    [ObservableProperty] private int totalOrders;
    [ObservableProperty] private decimal averageOrderValue;
    [ObservableProperty] private int unitsSold;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool hasReportData;
    [ObservableProperty] private bool hasTopProducts;
    [ObservableProperty] private bool hasCategorySales;
    public Visibility ReportEmptyVisibility => IsLoading || HasReportData ? Visibility.Collapsed : Visibility.Visible;
    public Visibility TopProductsEmptyVisibility => IsLoading || HasTopProducts ? Visibility.Collapsed : Visibility.Visible;
    public Visibility CategorySalesEmptyVisibility => IsLoading || HasCategorySales ? Visibility.Collapsed : Visibility.Visible;

    public Task InitializeAsync() => LoadAsync();

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            var report = await reports.GetSalesReportAsync(FromDate, ToDate);
            TotalRevenue = report.TotalRevenue;
            TotalOrders = report.TotalOrders;
            AverageOrderValue = report.AverageOrderValue;
            UnitsSold = report.UnitsSold;
            Set(RevenuePoints, report.RevenuePoints);
            Set(RevenueChartPoints, CreateChartPoints(report.RevenuePoints));
            Set(TopProducts, report.TopProducts);
            Set(CategorySales, report.CategorySales);
            HasReportData = report.TotalOrders > 0;
            HasTopProducts = TopProducts.Count > 0;
            HasCategorySales = CategorySales.Count > 0;
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
            HasReportData = HasTopProducts = HasCategorySales = false;
        }
        catch
        {
            ErrorMessage = "Không thể tải dữ liệu báo cáo. Vui lòng thử lại.";
            HasReportData = HasTopProducts = HasCategorySales = false;
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(ReportEmptyVisibility));
            OnPropertyChanged(nameof(TopProductsEmptyVisibility));
            OnPropertyChanged(nameof(CategorySalesEmptyVisibility));
        }
    }

    [RelayCommand] private async Task SetTodayAsync() { FromDate = ToDate = DateTime.Today; await LoadAsync(); }
    [RelayCommand] private async Task Set7DaysAsync() { FromDate = DateTime.Today.AddDays(-6); ToDate = DateTime.Today; await LoadAsync(); }
    private static void Set<T>(ObservableCollection<T> target, IReadOnlyList<T> source) { target.Clear(); foreach (var item in source) target.Add(item); }
    private static IReadOnlyList<RevenueChartPointViewModel> CreateChartPoints(IReadOnlyList<RevenuePointDto> points)
    {
        var maximum = points.Count == 0 ? 0 : points.Max(point => point.Revenue);
        return points.Select(point => new RevenueChartPointViewModel(point.Date.ToString("dd/MM"), point.Revenue,
            maximum == 0 || point.Revenue == 0 ? 0 : Math.Max(10, (double)(point.Revenue / maximum * 118)))).ToList();
    }
}
