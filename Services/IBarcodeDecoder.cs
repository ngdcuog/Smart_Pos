using OpenCvSharp;

namespace SmartPOS.Services;

public interface IBarcodeDecoder
{
    string? Decode(Mat frame);
}
