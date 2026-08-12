using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartPOS.Services;
using SmartPOS.Services.Dtos;
using System.Windows;

namespace SmartPOS.ViewModels;

public partial class DashboardViewModel(IReportService reports)
    : PlaceholderViewModel("Tổng quan", "Theo dõi hoạt động bán hàng và vận hành cửa hàng.", string.Empty), IAsyncInitializable
{
    public ObservableCollection<RevenuePointDto> RevenuePoints { get; } = [];
    public ObservableCollection<TopProductDto> TopProducts { get; } = [];
    public ObservableCollection<LowStockDto> LowStockProducts { get; } = [];
    public ObservableCollection<RecentOrderDto> RecentOrders { get; } = [];
    public ObservableCollection<RevenueChartPointViewModel> RevenueChartPoints { get; } = [];

    [ObservableProperty] private decimal revenueToday;
    [ObservableProperty] private int ordersToday;
    [ObservableProperty] private int lowStockCount;
    [ObservableProperty] private int attendanceToday;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private bool hasRevenueData;
    [ObservableProperty] private decimal sevenDayRevenue;
    [ObservableProperty] private bool hasLowStockProducts;
    [ObservableProperty] private bool hasTopProducts;
    [ObservableProperty] private bool hasRecentOrders;
    public Visibility RevenueEmptyVisibility => HasRevenueData ? Visibility.Collapsed : Visibility.Visible;
    public Visibility LowStockEmptyVisibility => HasLowStockProducts ? Visibility.Collapsed : Visibility.Visible;
    public Visibility TopProductsEmptyVisibility => HasTopProducts ? Visibility.Collapsed : Visibility.Visible;
    public Visibility RecentOrdersEmptyVisibility => HasRecentOrders ? Visibility.Collapsed : Visibility.Visible;

    public Task InitializeAsync() => LoadAsync();

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            var dashboard = await reports.GetDashboardAsync();
            RevenueToday = dashboard.RevenueToday;
            OrdersToday = dashboard.OrdersToday;
            LowStockCount = dashboard.LowStockCount;
            AttendanceToday = dashboard.AttendanceToday;
            Set(RevenuePoints, dashboard.RevenuePoints);
            Set(RevenueChartPoints, CreateChartPoints(dashboard.RevenuePoints));
            SevenDayRevenue = dashboard.RevenuePoints.Sum(point => point.Revenue);
            HasRevenueData = SevenDayRevenue > 0;
            OnPropertyChanged(nameof(RevenueEmptyVisibility));
            Set(TopProducts, dashboard.TopProducts);
            Set(LowStockProducts, dashboard.LowStockProducts);
            Set(RecentOrders, dashboard.RecentOrders);
            HasLowStockProducts = LowStockProducts.Count > 0;
            HasTopProducts = TopProducts.Count > 0;
            HasRecentOrders = RecentOrders.Count > 0;
            OnPropertyChanged(nameof(LowStockEmptyVisibility));
            OnPropertyChanged(nameof(TopProductsEmptyVisibility));
            OnPropertyChanged(nameof(RecentOrdersEmptyVisibility));
        }
        catch
        {
            ErrorMessage = "Không thể tải dữ liệu tổng quan. Vui lòng thử lại.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static void Set<T>(ObservableCollection<T> target, IReadOnlyList<T> source)
    {
        target.Clear();
        foreach (var item in source) target.Add(item);
    }

    private static IReadOnlyList<RevenueChartPointViewModel> CreateChartPoints(IReadOnlyList<RevenuePointDto> points)
    {
        var maximum = points.Count == 0 ? 0 : points.Max(point => point.Revenue);
        return points.Select(point => new RevenueChartPointViewModel(
            point.Date.ToString("dd/MM"), point.Revenue, maximum == 0 || point.Revenue == 0 ? 0 : Math.Max(12, (double)(point.Revenue / maximum * 150)))).ToList();
    }
}
