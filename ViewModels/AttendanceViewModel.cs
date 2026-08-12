using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartPOS.Services;
using SmartPOS.Services.Dtos;
using System.Windows;

namespace SmartPOS.ViewModels;
public partial class AttendanceViewModel(IAttendanceService attendanceService, IFaceVerificationService faceService, ICameraService camera, FaceVerificationSettings settings) : PlaceholderViewModel("Chấm công", "Quét QR hoặc nhập mã nhân viên để check-in/check-out.", string.Empty), IAsyncInitializable
{
    public ObservableCollection<AttendanceRecordItem> TodayRecords { get; } = [];
    [ObservableProperty] private string employeeCodeInput = string.Empty; [ObservableProperty] private AttendanceEmployee? identifiedEmployee;
    [ObservableProperty] private bool isLoading; [ObservableProperty] private bool isWorking; [ObservableProperty] private bool hasRecords; [ObservableProperty] private string? errorMessage; [ObservableProperty] private string? successMessage; [ObservableProperty] private bool hasError; [ObservableProperty] private bool hasSuccess;
    [ObservableProperty] private string faceStatus = "Nhập mã nhân viên để bắt đầu."; [ObservableProperty] private bool isFaceEnrolled; [ObservableProperty] private bool isFaceVerified; [ObservableProperty] private bool isFallbackAvailable; [ObservableProperty] private int faceAttempts;
    public bool CanCheckIn => IdentifiedEmployee is { HasCheckedInToday: false } && (IsFaceVerified || IsFallbackAvailable);
    public bool CanCheckOut => IdentifiedEmployee is { HasCheckedInToday: true, HasCheckedOutToday: false } && (IsFaceVerified || IsFallbackAvailable);
    public int MaxFaceAttempts => settings.MaxAttempts;
    public bool CanVerifyFace => IsFaceEnrolled && !IsFaceVerified && FaceAttempts < settings.MaxAttempts;
    public Visibility TodayRecordsEmptyVisibility => IsLoading || HasRecords ? Visibility.Collapsed : Visibility.Visible;
    public string SummaryEmployeeName => IdentifiedEmployee?.FullName ?? "Chưa xác định nhân viên";
    public string SummaryEmployeeCode => IdentifiedEmployee?.EmployeeCode ?? "Nhập mã hoặc quét QR để tiếp tục";
    public Task InitializeAsync() => LoadTodayAsync();
    [RelayCommand] private async Task IdentifyEmployeeAsync()
    {
        try { IsWorking = true; ErrorMessage = SuccessMessage = null; IdentifiedEmployee = await attendanceService.ResolveEmployeeAsync(EmployeeCodeInput); IsFaceEnrolled = await faceService.HasUsableEnrollmentAsync(IdentifiedEmployee.EmployeeId); IsFaceVerified = false; FaceAttempts = 0; IsFallbackAvailable = !IsFaceEnrolled; FaceStatus = IsFaceEnrolled ? "Đã có đăng ký khuôn mặt. Bạn có thể xác thực hỗ trợ bằng camera." : "Nhân viên chưa đăng ký khuôn mặt. Có thể tiếp tục bằng mã nhân viên."; NotifyActions(); }
        catch (AttendanceServiceException ex) { IdentifiedEmployee = null; ErrorMessage = ex.Message; NotifyActions(); }
        catch { ErrorMessage = "Không thể xác định nhân viên. Vui lòng thử lại."; }
        finally { IsWorking = false; }
    }
    [RelayCommand] private async Task VerifyFaceAsync()
    {
        if (IdentifiedEmployee is null || !CanVerifyFace) return; try { IsWorking = true; ErrorMessage = null; await camera.OpenAsync(settings.CameraIndex); using var frame = await camera.CaptureFrameAsync(); if (frame is null) throw new InvalidOperationException("Camera không trả về hình ảnh."); var result = faceService.Verify(IdentifiedEmployee.EmployeeId, frame); IsFaceVerified = result.Success; if (result.Success) FaceStatus = $"{result.Message} (distance: {result.Distance:0.0})"; else { FaceAttempts++; FaceStatus = $"{result.Message} (lần thất bại {FaceAttempts}/{settings.MaxAttempts}, distance: {result.Distance:0.0})"; if (FaceAttempts >= settings.MaxAttempts) { IsFallbackAvailable = true; FaceStatus += " Bạn có thể tiếp tục bằng mã nhân viên."; } } Console.WriteLine($"FaceVerify expected={result.ExpectedEmployeeId} predicted={result.PredictedEmployeeId} distance={result.Distance:0.0} threshold={settings.DistanceThreshold:0.0} accepted={result.Success}"); NotifyActions(); }
        catch (Exception ex) { IsFallbackAvailable = true; FaceStatus = "Không thể sử dụng camera. Bạn có thể tiếp tục bằng mã nhân viên."; ErrorMessage = ex.Message; NotifyActions(); }
        finally { await camera.StopAsync(); IsWorking = false; }
    }
    [RelayCommand] private void UseCodeFallback() { IsFallbackAvailable = true; FaceStatus = "Đã chọn chấm công bằng mã nhân viên/QR theo quy trình dự phòng."; NotifyActions(); }
    [RelayCommand] private async Task CheckInAsync() { try { var result = await attendanceService.CheckInAsync(EmployeeCodeInput); SuccessMessage = $"{result.EmployeeName}: {result.Message}"; await LoadTodayAsync(); } catch (AttendanceServiceException ex) { ErrorMessage = ex.Message; } finally { await camera.StopAsync(); } }
    [RelayCommand] private async Task CheckOutAsync() { try { var result = await attendanceService.CheckOutAsync(EmployeeCodeInput); SuccessMessage = $"{result.EmployeeName}: {result.Message}"; await LoadTodayAsync(); } catch (AttendanceServiceException ex) { ErrorMessage = ex.Message; } finally { await camera.StopAsync(); } }
    [RelayCommand] public Task StopCameraAsync() => camera.StopAsync();
    [RelayCommand] private async Task LoadTodayAsync() { try { IsLoading = true; var records = await attendanceService.GetAttendanceAsync(DateTime.Today, DateTime.Today, null); TodayRecords.Clear(); foreach (var r in records) TodayRecords.Add(r); HasRecords = TodayRecords.Count > 0; } finally { IsLoading = false; } }
    private void NotifyActions() { OnPropertyChanged(nameof(CanCheckIn)); OnPropertyChanged(nameof(CanCheckOut)); OnPropertyChanged(nameof(CanVerifyFace)); }
    partial void OnIdentifiedEmployeeChanged(AttendanceEmployee? value)
    {
        OnPropertyChanged(nameof(SummaryEmployeeName));
        OnPropertyChanged(nameof(SummaryEmployeeCode));
    }
    partial void OnErrorMessageChanged(string? value) => HasError = !string.IsNullOrWhiteSpace(value); partial void OnSuccessMessageChanged(string? value) => HasSuccess = !string.IsNullOrWhiteSpace(value);
    partial void OnHasRecordsChanged(bool value) => OnPropertyChanged(nameof(TodayRecordsEmptyVisibility));
    partial void OnIsLoadingChanged(bool value) => OnPropertyChanged(nameof(TodayRecordsEmptyVisibility));
}
