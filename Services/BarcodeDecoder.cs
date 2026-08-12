using System.Runtime.InteropServices;
using OpenCvSharp;
using ZXing;
using ZXing.Common;

namespace SmartPOS.Services;

/// <summary>Decodes retail barcode formats from a single OpenCV camera frame.</summary>
public sealed class BarcodeDecoder : IBarcodeDecoder
{
    private static readonly BarcodeFormat[] SupportedFormats =
    [
        BarcodeFormat.EAN_13, BarcodeFormat.EAN_8, BarcodeFormat.UPC_A, BarcodeFormat.UPC_E,
        BarcodeFormat.CODE_128, BarcodeFormat.CODE_39, BarcodeFormat.ITF, BarcodeFormat.CODABAR,
        BarcodeFormat.QR_CODE
    ];

    public string? Decode(Mat frame)
    {
        if (frame.Empty()) return null;

        try
        {
            using var rgb = new Mat();
            Cv2.CvtColor(frame, rgb, ColorConversionCodes.BGR2RGB);
            var pixels = new byte[checked(rgb.Rows * rgb.Cols * rgb.Channels())];
            Marshal.Copy(rgb.Data, pixels, 0, pixels.Length);
            var source = new RGBLuminanceSource(pixels, rgb.Width, rgb.Height, RGBLuminanceSource.BitmapFormat.RGB24);
            var reader = new BarcodeReaderGeneric
            {
                AutoRotate = true,
                Options = new DecodingOptions { TryHarder = true, TryInverted = true, PossibleFormats = SupportedFormats }
            };
            return reader.Decode(source)?.Text?.Trim();
        }
        catch
        {
            // A bad/partial webcam frame is simply not a readable barcode.
            return null;
        }
    }
}
