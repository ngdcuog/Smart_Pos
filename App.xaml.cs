using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartPOS.Data;
using SmartPOS.Services;
using SmartPOS.ViewModels;

namespace SmartPOS;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .Build();
        var connectionString = configuration.GetConnectionString("SmartPOS")
            ?? throw new InvalidOperationException("Connection string 'SmartPOS' is not configured.");

        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddTransient<IProductService, ProductService>();
        services.AddSingleton<IProductImageStorage, ProductImageStorage>();
        services.AddTransient<IInventoryService, InventoryService>();
        services.AddTransient<IOrderService, OrderService>();
        var invoiceOutputDirectory = configuration["Invoice:OutputDirectory"];
        services.AddSingleton<IInvoicePathProvider>(_ => string.IsNullOrWhiteSpace(invoiceOutputDirectory)
            ? new InvoicePathProvider()
            : new InvoicePathProvider(invoiceOutputDirectory));
        services.AddTransient<IInvoiceService, InvoiceService>();
        services.AddTransient<IReportService, ReportService>();
        services.AddSingleton(new AISettings { Provider=configuration["AI:Provider"]??"OpenAI", Model=configuration["AI:Model"]??"gpt-4o-mini", BaseUrl=configuration["AI:BaseUrl"]??"https://api.openai.com/v1/chat/completions", ApiKey=configuration["AI:ApiKey"], ApiKeyEnvironmentVariable=configuration["AI:ApiKeyEnvironmentVariable"]??"SMARTPOS_AI_API_KEY", TimeoutSeconds=int.TryParse(configuration["AI:TimeoutSeconds"],out var timeout)?timeout:30 });
        services.AddHttpClient<IAIChatService, AIChatService>(client => client.Timeout = TimeSpan.FromSeconds(30));
        services.AddTransient<IEmployeeService, EmployeeService>();
        services.AddTransient<IAttendanceService, AttendanceService>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton(new AttendanceSettings
        {
            WorkStartTime = configuration["Attendance:WorkStartTime"] ?? "08:00",
            LateGraceMinutes = int.TryParse(configuration["Attendance:LateGraceMinutes"], out var graceMinutes) ? graceMinutes : 15
        });
        var faceCameraIndex = int.TryParse(configuration["FaceVerification:CameraIndex"], out var cameraIndex) ? cameraIndex : 0;
        services.AddSingleton(new FaceVerificationSettings
        {
            CameraIndex = faceCameraIndex,
            SampleTarget = int.TryParse(configuration["FaceVerification:SampleTarget"], out var sampleTarget) ? sampleTarget : 20,
            SampleWidth = int.TryParse(configuration["FaceVerification:SampleWidth"], out var sampleWidth) ? sampleWidth : 200,
            SampleHeight = int.TryParse(configuration["FaceVerification:SampleHeight"], out var sampleHeight) ? sampleHeight : 200,
            CaptureIntervalMs = int.TryParse(configuration["FaceVerification:CaptureIntervalMs"], out var captureInterval) ? captureInterval : 400,
            MaxAttempts = int.TryParse(configuration["FaceVerification:MaxAttempts"], out var maxAttempts) ? maxAttempts : 3,
            DistanceThreshold = double.TryParse(configuration["FaceVerification:DistanceThreshold"], out var threshold) ? threshold : 70
        });
        services.AddSingleton(new BarcodeScannerSettings
        {
            CameraIndex = int.TryParse(configuration["BarcodeScanner:CameraIndex"], out var barcodeCameraIndex) ? barcodeCameraIndex : faceCameraIndex,
            DecodeIntervalMs = int.TryParse(configuration["BarcodeScanner:DecodeIntervalMs"], out var decodeInterval) ? decodeInterval : 250,
            DuplicateCooldownMs = int.TryParse(configuration["BarcodeScanner:DuplicateCooldownMs"], out var duplicateCooldown) ? duplicateCooldown : 1500
        });
        services.AddSingleton<IBarcodeDecoder, BarcodeDecoder>();
        services.AddSingleton<FaceDetectionService>();
        services.AddTransient<IFaceVerificationService, FaceVerificationService>();
        services.AddSingleton<ICameraService, CameraService>();
        services.AddSingleton<ICurrentUserService, DevelopmentCurrentUserService>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();
        _serviceProvider = services.BuildServiceProvider();

        using (var scope = _serviceProvider.CreateScope())
        {
            using var context = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
            context.Database.Migrate();
        }

        _serviceProvider.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
