using OpenCvSharp;

namespace SmartPOS.Services;

public interface IFaceVerificationService
{
    Task<int> GetFaceSampleCountAsync(int employeeId);
    Task<bool> HasUsableEnrollmentAsync(int employeeId);
    Task CompleteEnrollmentAsync(int employeeId, IReadOnlyList<Mat> normalizedSamples);
    Task RebuildModelAsync();
    FaceVerificationResult Verify(int expectedEmployeeId, Mat frame);
}
