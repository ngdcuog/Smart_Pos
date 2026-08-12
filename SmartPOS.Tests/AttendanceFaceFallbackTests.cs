using OpenCvSharp;
using SmartPOS.Services;
using SmartPOS.Services.Dtos;
using SmartPOS.ViewModels;

namespace SmartPOS.Tests;

public sealed class AttendanceFaceFallbackTests
{
    [Fact]
    public async Task ThreeFaceMismatches_EnableFallback_AndCodeCheckInStillWorks()
    {
        var attendance = new FakeAttendanceService();
        var viewModel = new AttendanceViewModel(attendance, new MismatchFaceService(), new FakeCamera(), new FaceVerificationSettings { MaxAttempts = 3 });
        viewModel.EmployeeCodeInput = "NV0001";
        await viewModel.IdentifyEmployeeCommand.ExecuteAsync(null);
        await viewModel.VerifyFaceCommand.ExecuteAsync(null);
        await viewModel.VerifyFaceCommand.ExecuteAsync(null);
        await viewModel.VerifyFaceCommand.ExecuteAsync(null);
        Assert.Equal(3, viewModel.FaceAttempts); Assert.True(viewModel.IsFallbackAvailable); Assert.False(viewModel.CanVerifyFace); Assert.True(viewModel.CanCheckIn);
        await viewModel.CheckInCommand.ExecuteAsync(null);
        Assert.Equal(1, attendance.CheckInCalls);
    }

    private sealed class FakeAttendanceService : IAttendanceService
    {
        public int CheckInCalls { get; private set; }
        public Task<AttendanceEmployee> ResolveEmployeeAsync(string rawEmployeeCode) => Task.FromResult(new AttendanceEmployee(1, "NV0001", "Nguyễn Minh Anh", false, false));
        public Task<AttendanceActionResult> CheckInAsync(string rawEmployeeCode) { CheckInCalls++; return Task.FromResult(new AttendanceActionResult("NV0001", "Nguyễn Minh Anh", DateTime.Now, "OK")); }
        public Task<AttendanceActionResult> CheckOutAsync(string rawEmployeeCode) => throw new NotSupportedException();
        public Task<IReadOnlyList<AttendanceRecordItem>> GetAttendanceAsync(DateTime? fromDate, DateTime? toDate, string? search) => Task.FromResult<IReadOnlyList<AttendanceRecordItem>>([]);
    }
    private sealed class MismatchFaceService : IFaceVerificationService
    {
        public Task<int> GetFaceSampleCountAsync(int employeeId) => Task.FromResult(20);
        public Task<bool> HasUsableEnrollmentAsync(int employeeId) => Task.FromResult(true);
        public Task CompleteEnrollmentAsync(int employeeId, IReadOnlyList<Mat> normalizedSamples) => Task.CompletedTask;
        public Task RebuildModelAsync() => Task.CompletedTask;
        public FaceVerificationResult Verify(int expectedEmployeeId, Mat frame) => new(false, expectedEmployeeId, 2, 20, "Khuôn mặt không khớp.");
    }
    private sealed class FakeCamera : ICameraService
    {
        public bool IsRunning { get; private set; }
        public event Action<Mat>? FrameCaptured { add { } remove { } }
        public Task OpenAsync(int cameraIndex, CancellationToken cancellationToken = default) { IsRunning = true; return Task.CompletedTask; }
        public Task<Mat?> CaptureFrameAsync(CancellationToken cancellationToken = default) => Task.FromResult<Mat?>(new Mat(20, 20, MatType.CV_8UC3));
        public Task StopAsync() { IsRunning = false; return Task.CompletedTask; }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
