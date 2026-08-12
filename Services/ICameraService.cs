using OpenCvSharp;
namespace SmartPOS.Services;
public interface ICameraService : IAsyncDisposable
{
    bool IsRunning { get; }
    event Action<Mat>? FrameCaptured;
    Task OpenAsync(int cameraIndex, CancellationToken cancellationToken = default);
    Task StopAsync();
    Task<Mat?> CaptureFrameAsync(CancellationToken cancellationToken = default);
}
