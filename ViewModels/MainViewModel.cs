using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartPOS.Services;

namespace SmartPOS.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel(IProductService productService, IProductImageStorage imageStorage, IInventoryService inventoryService, IOrderService orderService, IInvoiceService invoiceService, IEmployeeService employeeService, IAttendanceService attendanceService, IFaceVerificationService faceVerificationService, ICameraService cameraService, IBarcodeDecoder barcodeDecoder, BarcodeScannerSettings barcodeScannerSettings, FaceDetectionService detectionService, FaceVerificationSettings faceSettings, IReportService reportService, IAIChatService aiChatService, ICurrentUserService currentUser)
    {
        CurrentUserName = currentUser.DisplayName;
        CurrentUserRole = currentUser.Role;
        NavigationItems = new ObservableCollection<NavigationItemViewModel>
        {
            new("Tổng quan", "\uE80F", new DashboardViewModel(reportService)),
            new("Bán hàng", "\uE719", new POSViewModel(productService, orderService, invoiceService, currentUser, cameraService, barcodeDecoder, barcodeScannerSettings)),
            new("Sản phẩm", "\uE71D", new ProductManagementViewModel(productService, imageStorage)),
            new("Kho hàng", "\uE7B8", new InventoryViewModel(inventoryService)),
            new("Nhân viên", "\uE716", new EmployeeManagementViewModel(employeeService, cameraService, faceVerificationService, detectionService, faceSettings)),
            new("Chấm công", "\uE916", new AttendanceViewModel(attendanceService, faceVerificationService, cameraService, faceSettings)),
            new("Báo cáo", "\uE9D2", new ReportViewModel(reportService)),
            new("Trợ lý AI", "\uE8BD", new AIChatViewModel(aiChatService))
        };

        SelectedNavigationItem = NavigationItems[0];
    }

    // Temporary development identity; authentication will replace these values in a later phase.
    public string CurrentUserName { get; }

    public string CurrentUserRole { get; }

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }

    [ObservableProperty]
    private NavigationItemViewModel? selectedNavigationItem;

    [ObservableProperty]
    private PlaceholderViewModel? currentViewModel;

    partial void OnSelectedNavigationItemChanged(NavigationItemViewModel? value)
    {
        CurrentViewModel = value?.ViewModel;
        if (CurrentViewModel is IAsyncInitializable initializable) _ = initializable.InitializeAsync();
    }

    [RelayCommand] private void NavigateDashboard() => NavigateTo<DashboardViewModel>();
    [RelayCommand] private void NavigatePOS() => NavigateTo<POSViewModel>();
    [RelayCommand] private void NavigateProducts() => NavigateTo<ProductManagementViewModel>();
    [RelayCommand] private void NavigateInventory() => NavigateTo<InventoryViewModel>();
    [RelayCommand] private void NavigateEmployees() => NavigateTo<EmployeeManagementViewModel>();
    [RelayCommand] private void NavigateAttendance() => NavigateTo<AttendanceViewModel>();
    [RelayCommand] private void NavigateReports() => NavigateTo<ReportViewModel>();
    [RelayCommand] private void NavigateAIChat() => NavigateTo<AIChatViewModel>();

    [RelayCommand]
    private void Logout()
    {
        // Authentication and logout behavior are intentionally deferred beyond the application shell phase.
    }

    private void NavigateTo<TViewModel>() where TViewModel : PlaceholderViewModel
        => SelectedNavigationItem = NavigationItems.First(item => item.ViewModel is TViewModel);
}
