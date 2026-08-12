using OpenCvSharp;
using OpenCvSharp.Face;

namespace SmartPOS.Tests;

public sealed class OpenCvRuntimeTests
{
    [Fact]
    public void NativeRuntime_Loads_AndCanAllocateMat()
    {
        using var mat = new Mat(2, 2, MatType.CV_8UC1, Scalar.All(128));
        Assert.False(mat.Empty());
        Assert.Equal(4, mat.Total());
    }

    [Fact]
    public void ContribRuntime_Loads_LbphRecognizer()
    {
        using var recognizer = LBPHFaceRecognizer.Create();
        Assert.NotNull(recognizer);
    }
}
