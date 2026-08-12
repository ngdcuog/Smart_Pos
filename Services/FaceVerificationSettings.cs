namespace SmartPOS.Services;

public sealed class FaceVerificationSettings
{
    public int CameraIndex { get; init; }
    public int SampleTarget { get; init; } = 20;
    public int SampleWidth { get; init; } = 200;
    public int SampleHeight { get; init; } = 200;
    public int CaptureIntervalMs { get; init; } = 400;
    public int MaxAttempts { get; init; } = 3;
    public double DistanceThreshold { get; init; } = 70;
}
