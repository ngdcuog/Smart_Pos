using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartPOS.Models.Enums;
using SmartPOS.Services;
using SmartPOS.Services.Dtos;

namespace SmartPOS.ViewModels;

public sealed record EmployeeRoleOption(EmployeeRole? Value, string Name);
public sealed record EmployeeStatusOption(bool? Value, string Name);

public partial class EmployeeManagementViewModel(IEmployeeService employeeService, ICameraService camera, IFaceVerificationService faces, FaceDetectionService detection, FaceVerificationSettings settings) : PlaceholderViewModel("Nhân viên", "Quản lý thông tin và trạng thái làm việc của nhân viên.", string.Empty), IAsyncInitializable
{
    public ObservableCollection<EmployeeListItem> Employees { get; } = [];
    public IReadOnlyList<EmployeeRoleOption> RoleFilters { get; } = [new(null, "Tất cả vai trò"), new(EmployeeRole.Manager, "Quản lý"), new(EmployeeRole.Cashier, "Thu ngân")];
    public IReadOnlyList<EmployeeStatusOption> StatusFilters { get; } = [new(null, "Tất cả trạng thái"), new(true, "Đang hoạt động"), new(false, "Đã vô hiệu hóa")];
    public IReadOnlyList<EmployeeRoleOption> EditorRoles { get; } = [new(EmployeeRole.Manager, "Quản lý"), new(EmployeeRole.Cashier, "Thu ngân")];
    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private EmployeeRoleOption? selectedRole;
    [ObservableProperty] private EmployeeStatusOption? selectedStatus;
    [ObservableProperty] private EmployeeListItem? selectedEmployee;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isSaving;
    [ObservableProperty] private bool hasEmployees;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string? successMessage;
    [ObservableProperty] private bool hasError;
    [ObservableProperty] private bool hasSuccess;
    [ObservableProperty] private bool isNewEmployee;
    [ObservableProperty] private string editorTitle = "Thông tin nhân viên";
    [ObservableProperty] private string employeeCode = string.Empty;
    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private EmployeeRoleOption? editorRole;
    public Visibility EmployeesEmptyVisibility => IsLoading || HasEmployees ? Visibility.Collapsed : Visibility.Visible;

    public async Task InitializeAsync()
    {
        SelectedRole ??= RoleFilters[0]; SelectedStatus ??= StatusFilters[0]; EditorRole ??= EditorRoles[1];
        await LoadEmployeesAsync();
    }

    [RelayCommand]
    private async Task LoadEmployeesAsync()
    {
        try { IsLoading = true; ErrorMessage = null; var items = await employeeService.GetEmployeesAsync(SearchText, SelectedRole?.Value, SelectedStatus?.Value); Employees.Clear(); foreach (var item in items) Employees.Add(item); HasEmployees = Employees.Count > 0; }
        catch { ErrorMessage = "Không thể tải danh sách nhân viên. Vui lòng thử lại."; }
        finally { IsLoading = false; }
    }

    [RelayCommand] private Task RefreshAsync() => LoadEmployeesAsync();

    [RelayCommand]
    private void AddEmployee()
    {
        IsNewEmployee = true; EditorTitle = "Thêm nhân viên"; EmployeeCode = FullName = Email = Phone = string.Empty; EditorRole = EditorRoles[1]; SelectedEmployee = null; ErrorMessage = SuccessMessage = null;
    }

    partial void OnSelectedEmployeeChanged(EmployeeListItem? value)
    {
        if (value is null || IsSaving) return;
        IsNewEmployee = false; EditorTitle = "Chỉnh sửa nhân viên"; EmployeeCode = value.EmployeeCode; FullName = value.FullName; Email = value.Email; Phone = value.Phone ?? string.Empty;
        EditorRole = EditorRoles.First(x => x.Value == value.Role); ErrorMessage = SuccessMessage = null;
    }

    [RelayCommand]
    private async Task SaveEmployeeAsync()
    {
        if (EditorRole?.Value is not EmployeeRole role) { ErrorMessage = "Vui lòng chọn vai trò."; return; }
        try
        {
            IsSaving = true; ErrorMessage = SuccessMessage = null;
            var input = new EmployeeInput(IsNewEmployee ? null : SelectedEmployee?.EmployeeId, EmployeeCode, FullName, Email, Phone, role);
            if (IsNewEmployee) await employeeService.CreateEmployeeAsync(input); else await employeeService.UpdateEmployeeAsync(input);
            SuccessMessage = IsNewEmployee ? "Đã thêm nhân viên." : "Đã cập nhật nhân viên."; await LoadEmployeesAsync();
        }
        catch (EmployeeServiceException ex) { ErrorMessage = ex.Message; }
        catch { ErrorMessage = "Không thể lưu nhân viên. Vui lòng thử lại."; }
        finally { IsSaving = false; }
    }

    [RelayCommand]
    private async Task ToggleEmployeeStatusAsync(EmployeeListItem? employee)
    {
        if (employee is null) return;
        try { ErrorMessage = SuccessMessage = null; await employeeService.SetEmployeeActiveStateAsync(employee.EmployeeId, !employee.IsActive); SuccessMessage = employee.IsActive ? "Đã vô hiệu hóa nhân viên." : "Đã kích hoạt nhân viên."; await LoadEmployeesAsync(); }
        catch (EmployeeServiceException ex) { ErrorMessage = ex.Message; }
        catch { ErrorMessage = "Không thể cập nhật trạng thái nhân viên."; }
    }

    partial void OnErrorMessageChanged(string? value) => HasError = !string.IsNullOrWhiteSpace(value);
    [RelayCommand] private void EnrollFace(EmployeeListItem? employee) { if (employee is null || !employee.IsActive) { ErrorMessage = "Vui lòng chọn nhân viên đang hoạt động."; return; } new Views.FaceEnrollmentView { DataContext = new FaceEnrollmentViewModel(employee.EmployeeId, employee.EmployeeCode, employee.FullName, camera, faces, detection, settings), Owner = Application.Current.MainWindow }.ShowDialog(); _ = LoadEmployeesAsync(); }
    partial void OnSuccessMessageChanged(string? value) => HasSuccess = !string.IsNullOrWhiteSpace(value);
    partial void OnHasEmployeesChanged(bool value) => OnPropertyChanged(nameof(EmployeesEmptyVisibility));
    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(EmployeesEmptyVisibility));
}
