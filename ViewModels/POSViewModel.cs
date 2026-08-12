using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartPOS.Models.Enums;
using SmartPOS.Services;
using SmartPOS.Services.Dtos;
using System.Windows;
using System.Windows.Media;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
namespace SmartPOS.ViewModels;
public partial class POSViewModel(IProductService productService, IOrderService orderService, IInvoiceService invoiceService, ICurrentUserService currentUser,
    ICameraService cameraService, IBarcodeDecoder barcodeDecoder, BarcodeScannerSettings barcodeScannerSettings)
    : PlaceholderViewModel("Bán hàng", "Quét mã vạch, quản lý giỏ hàng và thanh toán.", string.Empty), IAsyncInitializable
{
    private readonly SemaphoreSlim cameraDecodeGate = new(1, 1);
    private DateTime lastCameraDecodeAt = DateTime.MinValue;
    private DateTime lastCameraBarcodeAt = DateTime.MinValue;
    private string? lastCameraBarcode;
    private bool isCameraHandlerAttached;
    public ObservableCollection<SaleProductItem> ProductResults { get; } = [];
    public ObservableCollection<CartItemViewModel> CartItems { get; } = [];
    public IReadOnlyList<PaymentOption> PaymentMethods { get; } = [new(PaymentMethod.Cash, "Tiền mặt"), new(PaymentMethod.BankTransfer, "Chuyển khoản")];
    [ObservableProperty] private string barcodeInput = string.Empty;
    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private string discountText = "0";
    [ObservableProperty] private PaymentOption? selectedPaymentMethod;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isAddingByBarcode;
    [ObservableProperty] private bool isCameraScanning;
    [ObservableProperty] private ImageSource? cameraPreview;
    [ObservableProperty] private string cameraStatusMessage = "Camera đang tắt. Bạn vẫn có thể quét bằng máy quét USB hoặc nhập mã.";
    [ObservableProperty] private bool isCameraStatusError;
    [ObservableProperty] private bool isCheckingOut;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string? successMessage;
    [ObservableProperty] private string? lastInvoicePath;
    [ObservableProperty] private string? barcodeFeedbackMessage;
    [ObservableProperty] private bool isBarcodeFeedbackError;
    [ObservableProperty] private bool hasCartItems;
    [ObservableProperty] private decimal totalAmount;
    [ObservableProperty] private decimal discountAmount;
    [ObservableProperty] private decimal finalAmount;
    public Visibility CartEmptyVisibility => HasCartItems ? Visibility.Collapsed : Visibility.Visible;
    public bool HasInvoicePdf => !string.IsNullOrWhiteSpace(LastInvoicePath);
    public string CameraToggleText => IsCameraScanning ? "Dừng camera" : "Bật camera quét mã";
    public async Task InitializeAsync() { SelectedPaymentMethod ??= PaymentMethods[0]; await LoadProductsAsync(); }
    [RelayCommand] private async Task LoadProductsAsync() { try { IsLoading = true; ErrorMessage = null; var items = await productService.GetSaleProductsAsync(SearchText); ProductResults.Clear(); foreach (var item in items) ProductResults.Add(item); } catch { ErrorMessage = "Không thể tải sản phẩm. Vui lòng thử lại."; } finally { IsLoading = false; } }
    [RelayCommand]
    private Task AddByBarcodeAsync() => ProcessBarcodeAsync(BarcodeInput, false);

    private async Task ProcessBarcodeAsync(string rawBarcode, bool isFromCamera)
    {
        if (IsAddingByBarcode) return;

        var barcode = rawBarcode.Trim();
        ErrorMessage = null;
        SuccessMessage = null;
        BarcodeFeedbackMessage = null;
        IsBarcodeFeedbackError = false;
        if (string.IsNullOrWhiteSpace(barcode))
        {
            ErrorMessage = "Vui lòng quét hoặc nhập mã vạch bán lẻ.";
            SetBarcodeFeedback(ErrorMessage, true);
            if (isFromCamera) SetCameraStatus(ErrorMessage, true);
            return;
        }

        try
        {
            IsAddingByBarcode = true;
            var product = await productService.GetSaleProductByBarcodeAsync(barcode);
            BarcodeInput = string.Empty;
            if (product is null)
            {
                ErrorMessage = await productService.IsCaseBarcodeAsync(barcode)
                    ? "Đây là mã vạch thùng. Vui lòng quét mã đơn vị bán lẻ để bán hàng."
                    : "Không tìm thấy sản phẩm với mã vạch này.";
                SetBarcodeFeedback(ErrorMessage, true);
                if (isFromCamera) SetCameraStatus(ErrorMessage, true);
                return;
            }

            AddProduct(product);
            if (string.IsNullOrWhiteSpace(ErrorMessage))
            {
                var quantity = CartItems.First(x => x.ProductId == product.ProductId).Quantity;
                SuccessMessage = $"Đã thêm {product.ProductName} · SL {quantity}.";
                SetBarcodeFeedback(SuccessMessage, false);
                if (isFromCamera) SetCameraStatus($"Đã quét {product.ProductName}. Đưa mã tiếp theo vào khung hình.", false);
            }
            else if (isFromCamera)
            {
                SetCameraStatus(ErrorMessage ?? "Không thể thêm sản phẩm này vào giỏ.", true);
            }
        }
        catch
        {
            ErrorMessage = "Không thể xử lý mã vạch. Vui lòng quét lại.";
            SetBarcodeFeedback(ErrorMessage, true);
            if (isFromCamera) SetCameraStatus(ErrorMessage, true);
        }
        finally
        {
            IsAddingByBarcode = false;
        }
    }

    [RelayCommand]
    private async Task ToggleCameraAsync()
    {
        if (IsCameraScanning)
        {
            await StopCameraAsync();
            return;
        }

        try
        {
            SetCameraStatus("Đang mở camera...", false);
            if (!isCameraHandlerAttached)
            {
                cameraService.FrameCaptured += OnCameraFrame;
                isCameraHandlerAttached = true;
            }
            await cameraService.OpenAsync(barcodeScannerSettings.CameraIndex);
            CameraPreview = null;
            IsCameraScanning = true;
            SetCameraStatus("Đưa mã vạch vào khung hình để quét. Mã được nhận tự động.", false);
        }
        catch
        {
            IsCameraScanning = false;
            SetCameraStatus("Không thể mở camera. Kiểm tra thiết bị hoặc CameraIndex trong cấu hình.", true);
        }
    }

    public async Task StopCameraAsync()
    {
        if (isCameraHandlerAttached)
        {
            cameraService.FrameCaptured -= OnCameraFrame;
            isCameraHandlerAttached = false;
        }
        await cameraService.StopAsync();
        IsCameraScanning = false;
        CameraPreview = null;
        SetCameraStatus("Camera đã tắt. Bạn vẫn có thể quét bằng máy quét USB hoặc nhập mã.", false);
    }

    private async void OnCameraFrame(Mat frame)
    {
        var hasDecodeLock = false;
        try
        {
            var preview = frame.ToBitmapSource();
            preview.Freeze();
            _ = Application.Current.Dispatcher.BeginInvoke(() => CameraPreview = preview);

            if (!IsCameraScanning || DateTime.UtcNow - lastCameraDecodeAt < TimeSpan.FromMilliseconds(barcodeScannerSettings.DecodeIntervalMs)
                || !cameraDecodeGate.Wait(0)) return;

            hasDecodeLock = true;
            lastCameraDecodeAt = DateTime.UtcNow;
            var barcode = await Task.Run(() => barcodeDecoder.Decode(frame));
            if (string.IsNullOrWhiteSpace(barcode)) return;
            if (string.Equals(barcode, lastCameraBarcode, StringComparison.Ordinal)
                && DateTime.UtcNow - lastCameraBarcodeAt < TimeSpan.FromMilliseconds(barcodeScannerSettings.DuplicateCooldownMs)) return;

            lastCameraBarcode = barcode;
            lastCameraBarcodeAt = DateTime.UtcNow;
            _ = Application.Current.Dispatcher.BeginInvoke(() => _ = ProcessBarcodeAsync(barcode, true));
        }
        catch
        {
            _ = Application.Current.Dispatcher.BeginInvoke(() => SetCameraStatus("Không thể đọc khung hình camera. Hãy thử lại hoặc dùng nhập mã.", true));
        }
        finally
        {
            if (hasDecodeLock) cameraDecodeGate.Release();
            frame.Dispose();
        }
    }
    [RelayCommand] private void AddProduct(SaleProductItem? product) { if (product is null) return; ErrorMessage = null; SuccessMessage = null; if (product.StockQuantity == 0) { ErrorMessage = $"{product.ProductName} đã hết hàng."; return; } var existing = CartItems.FirstOrDefault(x => x.ProductId == product.ProductId); if (existing is not null) { existing.UpdateAvailableStock(product.StockQuantity); if (!existing.Increase()) { ErrorMessage = "Số lượng trong giỏ đã đạt tồn kho hiện có."; return; } } else CartItems.Add(new CartItemViewModel(product)); RecalculateTotals(); }
    [RelayCommand] private void IncreaseQuantity(CartItemViewModel? item) { if (item?.Increase() == false) ErrorMessage = "Số lượng trong giỏ đã đạt tồn kho hiện có."; RecalculateTotals(); }
    [RelayCommand] private void DecreaseQuantity(CartItemViewModel? item) { if (item is null) return; if (!item.Decrease()) CartItems.Remove(item); RecalculateTotals(); }
    [RelayCommand] private void RemoveItem(CartItemViewModel? item) { if (item is not null) CartItems.Remove(item); RecalculateTotals(); }
    [RelayCommand] private void ClearCart() { CartItems.Clear(); DiscountText = "0"; ErrorMessage = null; SuccessMessage = null; RecalculateTotals(); }
    [RelayCommand] private async Task CheckoutAsync() { if (CartItems.Count == 0) { ErrorMessage = "Giỏ hàng đang trống."; return; } if (!decimal.TryParse(DiscountText, out var discount) || discount < 0) { ErrorMessage = "Giảm giá phải là số không âm."; return; } RecalculateTotals(); if (discount > TotalAmount) { ErrorMessage = "Giảm giá không được lớn hơn tạm tính."; return; } try { IsCheckingOut = true; ErrorMessage = null; SuccessMessage = null; LastInvoicePath = null; var receipt = await orderService.CreateOrderAsync(new CreateOrderRequest(currentUser.CurrentEmployeeId, discount, SelectedPaymentMethod?.Value ?? PaymentMethod.Cash, CartItems.Select(x => new CreateOrderItemRequest(x.ProductId, x.Quantity)).ToList())); CartItems.Clear(); DiscountText = "0"; RecalculateTotals(); BarcodeInput = string.Empty; try { var invoice = await invoiceService.GeneratePdfAsync(receipt.OrderId); LastInvoicePath = invoice.FilePath; SuccessMessage = $"Thanh toán thành công. Hóa đơn #{receipt.OrderId} đã lưu dưới dạng PDF."; } catch { SuccessMessage = $"Thanh toán thành công. Mã đơn: #{receipt.OrderId}. Không thể xuất PDF, bạn có thể tiếp tục bán hàng."; } await LoadProductsAsync(); } catch (OrderServiceException ex) { ErrorMessage = ex.Message; } catch (Exception ex) { ErrorMessage = $"Không thể hoàn tất thanh toán: {ex.Message}"; } finally { IsCheckingOut = false; } }
    [RelayCommand] private void OpenInvoice() { if (string.IsNullOrWhiteSpace(LastInvoicePath)) return; try { invoiceService.OpenPdf(LastInvoicePath); } catch { ErrorMessage = "Không thể mở hóa đơn PDF. Hãy kiểm tra lại thư mục Invoices."; } }
    partial void OnDiscountTextChanged(string value) => RecalculateTotals();
    partial void OnHasCartItemsChanged(bool value) => OnPropertyChanged(nameof(CartEmptyVisibility));
    partial void OnIsCameraScanningChanged(bool value) => OnPropertyChanged(nameof(CameraToggleText));
    partial void OnLastInvoicePathChanged(string? value) => OnPropertyChanged(nameof(HasInvoicePdf));
    private void RecalculateTotals() { TotalAmount = CartItems.Sum(x => x.LineTotal); DiscountAmount = decimal.TryParse(DiscountText, out var discount) && discount >= 0 ? discount : 0; FinalAmount = Math.Max(0, TotalAmount - DiscountAmount); HasCartItems = CartItems.Count > 0; }
    private void SetBarcodeFeedback(string? message, bool isError)
    {
        BarcodeFeedbackMessage = message;
        IsBarcodeFeedbackError = isError;
    }
    private void SetCameraStatus(string message, bool isError)
    {
        CameraStatusMessage = message;
        IsCameraStatusError = isError;
    }
}
public sealed record PaymentOption(PaymentMethod Value, string Name);
