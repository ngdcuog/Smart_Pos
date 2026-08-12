using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SmartPOS.Services;
namespace SmartPOS.ViewModels;
public partial class FaceEnrollmentViewModel : ObservableObject
{
    private readonly ICameraService _camera; private readonly IFaceVerificationService _faces; private readonly FaceDetectionService _detection; private readonly FaceVerificationSettings _settings; private readonly List<Mat> _samples=[]; private DateTime _last;
    public FaceEnrollmentViewModel(int id,string code,string name,ICameraService camera,IFaceVerificationService faces,FaceDetectionService detection,FaceVerificationSettings settings){EmployeeId=id;EmployeeCode=code;EmployeeName=name;_camera=camera;_faces=faces;_detection=detection;_settings=settings;_camera.FrameCaptured+=OnFrame;}
    public int EmployeeId{get;} public string EmployeeCode{get;} public string EmployeeName{get;} public int SampleTarget=>_settings.SampleTarget;
    [ObservableProperty] private ImageSource? cameraPreview; [ObservableProperty] private int capturedSamples; [ObservableProperty] private bool isCapturing; [ObservableProperty] private string statusMessage="Đang chờ mở camera.";
    public string Instruction => CapturedSamples < 7 ? "Nhìn thẳng vào camera" : CapturedSamples < 13 ? "Xoay nhẹ sang trái" : "Xoay nhẹ sang phải";
    public async Task ActivateAsync(){try{StatusMessage="Đang mở camera...";await _camera.OpenAsync(_settings.CameraIndex);StatusMessage="Camera sẵn sàng. Bấm Bắt đầu đăng ký.";}catch{StatusMessage="Không thể mở camera. Vui lòng kiểm tra thiết bị.";}}
    [RelayCommand] private void StartEnrollment(){IsCapturing=true;StatusMessage="Đang thu thập mẫu khuôn mặt.";}
    private async void OnFrame(Mat frame){try{var preview=frame.ToBitmapSource();preview.Freeze();_ = Application.Current.Dispatcher.BeginInvoke(()=>CameraPreview=preview);if(!IsCapturing||DateTime.UtcNow-_last<TimeSpan.FromMilliseconds(_settings.CaptureIntervalMs))return;using var normalized=_detection.Normalize(frame,out var message);StatusMessage=message;if(normalized.Empty())return;_last=DateTime.UtcNow;_samples.Add(normalized.Clone());CapturedSamples=_samples.Count;OnPropertyChanged(nameof(Instruction));StatusMessage=$"Đã thu thập {CapturedSamples}/{SampleTarget} mẫu.";if(CapturedSamples>=SampleTarget){IsCapturing=false;StatusMessage="Đang lưu và huấn luyện mô hình...";await _faces.CompleteEnrollmentAsync(EmployeeId,_samples);StatusMessage="Đăng ký khuôn mặt hoàn tất.";await StopAsync();}}catch(Exception){StatusMessage="Không thể hoàn tất đăng ký. Dữ liệu cũ vẫn được giữ nguyên.";}finally{frame.Dispose();}}
    [RelayCommand] public async Task StopAsync(){IsCapturing=false;await _camera.StopAsync();foreach(var s in _samples)s.Dispose();_samples.Clear();}
}
