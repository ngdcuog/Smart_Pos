using System.IO;
using Microsoft.EntityFrameworkCore;
using OpenCvSharp;
using OpenCvSharp.Face;
using SmartPOS.Data;
using SmartPOS.Models;

namespace SmartPOS.Services;

public sealed class FaceVerificationService(IDbContextFactory<AppDbContext> contextFactory, FaceDetectionService detection, FaceVerificationSettings settings) : IFaceVerificationService
{
    private readonly string _root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SmartPOS");
    private string SamplesRoot => Path.Combine(_root, "FaceSamples");
    private string ModelPath => Path.Combine(_root, "FaceModels", "lbph_model.yml");
    public async Task<int> GetFaceSampleCountAsync(int employeeId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var paths = await context.FaceSamples.AsNoTracking().Where(x => x.EmployeeId == employeeId).Select(x => x.ImagePath).ToListAsync();
        return paths.Count(File.Exists);
    }
    public async Task<bool> HasUsableEnrollmentAsync(int employeeId) => await GetFaceSampleCountAsync(employeeId) >= settings.SampleTarget;
    public async Task CompleteEnrollmentAsync(int employeeId, IReadOnlyList<Mat> normalizedSamples)
    {
        if (normalizedSamples.Count < settings.SampleTarget) throw new InvalidOperationException($"Cần đủ {settings.SampleTarget} mẫu khuôn mặt hợp lệ.");
        await using var c = await contextFactory.CreateDbContextAsync(); var employee = await c.Employees.FindAsync(employeeId) ?? throw new InvalidOperationException("Không tìm thấy nhân viên.");
        var stage = Path.Combine(Path.GetTempPath(), "SmartPOSFace", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(stage);
        try
        {
            for (var i = 0; i < settings.SampleTarget; i++) Cv2.ImWrite(Path.Combine(stage, $"sample_{i + 1:D3}.png"), normalizedSamples[i]);
            var finalDirectory = Path.Combine(SamplesRoot, employee.EmployeeCode); Directory.CreateDirectory(SamplesRoot);
            var old = await c.FaceSamples.Where(x => x.EmployeeId == employeeId).ToListAsync();
            var backup = finalDirectory + ".backup-" + Guid.NewGuid().ToString("N"); if (Directory.Exists(finalDirectory)) Directory.Move(finalDirectory, backup); Directory.Move(stage, finalDirectory);
            c.FaceSamples.RemoveRange(old); for (var i = 0; i < settings.SampleTarget; i++) c.FaceSamples.Add(new FaceSample { EmployeeId = employeeId, ImagePath = Path.Combine(finalDirectory, $"sample_{i + 1:D3}.png"), CreatedDate = DateTime.Now });
            await c.SaveChangesAsync(); await RebuildModelAsync(); if (Directory.Exists(backup)) Directory.Delete(backup, true);
        }
        catch { if (Directory.Exists(stage)) Directory.Delete(stage, true); throw; }
    }
    public async Task RebuildModelAsync()
    {
        await using var c = await contextFactory.CreateDbContextAsync(); var samples = await c.FaceSamples.AsNoTracking().ToListAsync(); var images = new List<Mat>(); var labels = new List<int>();
        try { foreach (var s in samples.Where(x => File.Exists(x.ImagePath))) { var image = Cv2.ImRead(s.ImagePath, ImreadModes.Grayscale); if (!image.Empty()) { images.Add(image); labels.Add(s.EmployeeId); } } if (images.Count == 0) throw new InvalidOperationException("Không có mẫu khuôn mặt hợp lệ để huấn luyện."); Directory.CreateDirectory(Path.GetDirectoryName(ModelPath)!); var temp = ModelPath + ".tmp"; using var recognizer = LBPHFaceRecognizer.Create(); recognizer.Train(images, labels); recognizer.Write(temp); File.Move(temp, ModelPath, true); }
        finally { foreach (var image in images) image.Dispose(); }
    }
    public FaceVerificationResult Verify(int expectedEmployeeId, Mat frame)
    {
        using var face = detection.Normalize(frame, out var detectionMessage); if (face.Empty()) return new(false, expectedEmployeeId, -1, double.PositiveInfinity, detectionMessage);
        if (!File.Exists(ModelPath)) return new(false, expectedEmployeeId, -1, double.PositiveInfinity, "Chưa có mô hình khuôn mặt. Vui lòng dùng mã nhân viên.");
        using var recognizer = LBPHFaceRecognizer.Create(); recognizer.Read(ModelPath); recognizer.Predict(face, out var predicted, out var distance);
        var success = predicted == expectedEmployeeId && distance <= settings.DistanceThreshold;
        return new(success, expectedEmployeeId, predicted, distance, success ? "Xác thực khuôn mặt hỗ trợ thành công." : "Khuôn mặt không khớp hoặc vượt ngưỡng thử nghiệm.");
    }
}
