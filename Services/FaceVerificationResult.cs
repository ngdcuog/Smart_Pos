namespace SmartPOS.Services;

public sealed record FaceVerificationResult(bool Success, int ExpectedEmployeeId, int PredictedEmployeeId, double Distance, string Message);
