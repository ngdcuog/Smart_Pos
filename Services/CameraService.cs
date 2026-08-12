using OpenCvSharp;
namespace SmartPOS.Services;
public sealed class CameraService : ICameraService
{
    private readonly SemaphoreSlim _captureGate = new(1, 1);
    private VideoCapture? _capture; private CancellationTokenSource? _cts; private Task? _loop;
    public bool IsRunning => _capture?.IsOpened() == true; public event Action<Mat>? FrameCaptured;
    public async Task OpenAsync(int cameraIndex, CancellationToken cancellationToken = default)
    {
        await StopAsync(); _capture = new VideoCapture(cameraIndex); if (!_capture.IsOpened()) { _capture.Dispose(); _capture = null; throw new InvalidOperationException("Không thể mở camera."); }
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken); _loop = Task.Run(() => LoopAsync(_cts.Token), _cts.Token);
    }
    private async Task LoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested && _capture?.IsOpened() == true)
        {
            Mat? snapshot = null;
            var lockTaken = false;
            try
            {
                await _captureGate.WaitAsync(token).ConfigureAwait(false);
                lockTaken = true;
                using var frame = new Mat();
                if (_capture?.Read(frame) == true && !frame.Empty()) snapshot = frame.Clone();
            }
            finally { if (lockTaken) _captureGate.Release(); }
            if (snapshot is not null) FrameCaptured?.Invoke(snapshot);
            await Task.Delay(33, token).ConfigureAwait(false);
        }
    }
    public async Task<Mat?> CaptureFrameAsync(CancellationToken cancellationToken = default)
    {
        await _captureGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_capture?.IsOpened() != true) return null;
            var frame = new Mat(); if (_capture.Read(frame) && !frame.Empty()) return frame; frame.Dispose(); return null;
        }
        finally { _captureGate.Release(); }
    }
    public async Task StopAsync()
    {
        if (_cts is not null) { _cts.Cancel(); if (_loop is not null) try { await _loop; } catch (OperationCanceledException) { } _cts.Dispose(); _cts = null; _loop = null; }
        await _captureGate.WaitAsync().ConfigureAwait(false);
        try { _capture?.Release(); _capture?.Dispose(); _capture = null; }
        finally { _captureGate.Release(); }
    }
    public async ValueTask DisposeAsync() { await StopAsync(); _captureGate.Dispose(); }
}
