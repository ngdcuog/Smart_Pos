namespace SmartPOS.Services;

public sealed class BarcodeScannerSettings
{
    public int CameraIndex { get; init; }
    public int DecodeIntervalMs { get; init; } = 250;
    public int DuplicateCooldownMs { get; init; } = 1500;
}
