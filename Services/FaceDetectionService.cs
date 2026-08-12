using System.IO;
using OpenCvSharp;

namespace SmartPOS.Services;

public sealed class FaceDetectionService : IDisposable
{
    private readonly CascadeClassifier _cascade;
    private readonly FaceVerificationSettings _settings;
    public FaceDetectionService(FaceVerificationSettings settings)
    {
        _settings = settings;
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "OpenCV", "haarcascade_frontalface_default.xml");
        if (!File.Exists(path)) throw new InvalidOperationException("Không tìm thấy tệp Haar Cascade.");
        _cascade = new CascadeClassifier(path);
        if (_cascade.Empty()) throw new InvalidOperationException("Không thể tải Haar Cascade.");
    }
    public Mat Normalize(Mat frame, out string message)
    {
        using var gray = new Mat(); Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY); Cv2.EqualizeHist(gray, gray);
        var faces = _cascade.DetectMultiScale(gray, 1.1, 5, HaarDetectionTypes.ScaleImage, new Size(80, 80));
        if (faces.Length == 0) { message = "Không phát hiện khuôn mặt."; return new Mat(); }
        if (faces.Length > 1) { message = "Vui lòng chỉ để một người trong khung hình."; return new Mat(); }
        var result = new Mat(gray, faces[0]).Clone(); Cv2.Resize(result, result, new Size(_settings.SampleWidth, _settings.SampleHeight)); message = "Đã phát hiện một khuôn mặt."; return result;
    }
    public void Dispose() => _cascade.Dispose();
}
