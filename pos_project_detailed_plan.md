# Phần mềm bán hàng (POS) tích hợp chấm công QR/mã nhân viên, xác thực khuôn mặt + quét mã vạch + AI

*Đề cương chi tiết cho project PRN212 (WPF/C#, MVVM)*

## 1. Tổng quan hệ thống

**Tên gợi ý:** SmartPOS / RetailSuite

**3 nhóm người dùng:**
- **Quản lý (Admin/Manager):** quản lý sản phẩm, nhân viên, xem báo cáo, chat AI phân tích doanh thu
- **Thu ngân (Cashier):** bán hàng, quét mã vạch, in hóa đơn
- **Hệ thống chấm công:** đứng riêng, chạy nền, ghi nhận giờ vào/ra

**3 module chính + 1 module AI:**
1. Chấm công (QR/mã nhân viên + Face Verification nâng cao)
2. Bán hàng (Barcode Scanning + Checkout)
3. Quản lý kho/sản phẩm
4. Trợ lý AI (RAG trên dữ liệu bán hàng)

## 2. ERD — Cấu trúc CSDL (SQL Server)

**Bảng chính:**

```
Employee
- EmployeeId (PK)
- FullName, Email, Phone
- Role (Admin/Cashier)
- PasswordHash
- IsActive

FaceSample
- FaceSampleId (PK)
- EmployeeId (FK)
- ImagePath (ảnh mẫu đã chuẩn hóa)
- CreatedDate

Attendance
- AttendanceId (PK)
- EmployeeId (FK)
- CheckInTime, CheckOutTime
- Date
- Status (OnTime/Late)

Category
- CategoryId (PK)
- CategoryName

Product
- ProductId (PK)
- CategoryId (FK)
- ProductName, Barcode (unique index)
- CostPrice, SellingPrice
- StockQuantity, MinStockAlert
- ImagePath

Order
- OrderId (PK)
- EmployeeId (FK) -- ai bán
- OrderDate
- TotalAmount, DiscountAmount, FinalAmount
- PaymentMethod

OrderDetail
- OrderDetailId (PK)
- OrderId (FK), ProductId (FK)
- Quantity, UnitPrice, LineTotal

Customer (tùy chọn, nếu muốn có tích điểm)
- CustomerId (PK)
- Name, Phone, LoyaltyPoints

StockTransaction (nhập kho)
- StockTransactionId (PK)
- ProductId (FK), Quantity, Type (Import/Export)
- TransactionDate
```

**Quan hệ chính:**
Employee 1—N Attendance | Employee 1—N FaceSample | Employee 1—N Order | Order 1—N OrderDetail | Product 1—N OrderDetail | Category 1—N Product

## 3. Kiến trúc MVVM chi tiết

```
/Models
  Employee.cs, FaceSample.cs, Attendance.cs
  Product.cs, Category.cs
  Order.cs, OrderDetail.cs, Customer.cs

/Services (business logic, tách khỏi ViewModel)
  FaceVerificationService.cs   -> OpenCvSharp: phát hiện mặt, đăng ký mẫu, xác thực LBPH 1:1
  BarcodeService.cs             -> đọc mã vạch qua webcam (ZXing.Net) hoặc input máy quét
  AttendanceService.cs          -> ghi nhận check-in/out, tính công
  ProductService.cs             -> CRUD sản phẩm, kiểm tra tồn kho
  OrderService.cs                -> xử lý giỏ hàng, tính tiền, tạo hóa đơn
  AIChatService.cs               -> gọi API (OpenAI/Gemini), build context từ DB (RAG)
  DatabaseService.cs (EF Core DbContext)

/ViewModels
  AttendanceViewModel.cs         -> camera stream, enrollment, danh sách chấm công hôm nay
  POSViewModel.cs                 -> giỏ hàng hiện tại, quét mã, tính tổng, checkout
  ProductManagementViewModel.cs   -> CRUD sản phẩm, cảnh báo tồn kho
  ReportViewModel.cs              -> doanh thu, biểu đồ
  AIChatViewModel.cs              -> gửi câu hỏi, nhận trả lời, lịch sử chat
  MainViewModel.cs                -> điều hướng giữa các màn hình (Navigation)

/Views
  AttendanceView.xaml
  POSView.xaml
  ProductManagementView.xaml
  ReportView.xaml
  AIChatView.xaml
  MainWindow.xaml (shell chứa navigation)

/Helpers
  RelayCommand.cs (ICommand implementation)
  BitmapConverter.cs (convert Mat/OpenCV -> WriteableBitmap để hiển thị WPF)
```

**Data binding mẫu (POS):**
```csharp
public class POSViewModel : INotifyPropertyChanged
{
    public ObservableCollection<OrderDetailItem> CartItems { get; set; }
    public decimal TotalAmount => CartItems.Sum(x => x.LineTotal);
    public ICommand ScanBarcodeCommand { get; }
    public ICommand CheckoutCommand { get; }
    // ...
}
```

## 4. Luồng nghiệp vụ chính

### A. Chấm công đầu ca
```
Nhân viên quét QR / nhập mã nhân viên -> xác định EmployeeId
-> (nâng cao) FaceVerificationService.Verify(EmployeeId, frame)
-> đạt ngưỡng hoặc người dùng chọn fallback QR/mã -> AttendanceService ghi CheckInTime
-> hiển thị "Chào [Tên], check-in lúc [giờ]"
```

### B. Bán hàng
```
Thu ngân quét mã vạch sản phẩm -> BarcodeService đọc mã
-> ProductService tìm sản phẩm theo Barcode -> thêm vào CartItems (ObservableCollection)
-> tự động cập nhật TotalAmount (binding)
-> thu ngân bấm Checkout -> OrderService tạo Order + OrderDetail, trừ tồn kho
-> in hóa đơn (dùng PrintDialog của WPF hoặc export PDF)
```

### C. Chat AI phân tích (RAG đơn giản)
```
Quản lý gõ câu hỏi: "Tuần này bán chạy nhất mặt hàng gì?"
-> AIChatService trước tiên query CSDL lấy dữ liệu liên quan
   (ví dụ: SELECT TOP 5 sản phẩm theo SUM(Quantity) trong 7 ngày gần nhất)
-> Đóng gói kết quả query thành context, gửi kèm câu hỏi vào prompt gọi API AI
-> AI trả lời dựa trên context đó, không bịa số liệu
-> Hiển thị câu trả lời trong AIChatView
```

Đây chính là RAG version đơn giản — không cần vector database, chỉ cần query SQL trực tiếp làm context vì dữ liệu có cấu trúc rõ ràng (khác với RAG cho văn bản tự do).

## 5. Tech stack đề xuất

| Thành phần | Công nghệ |
|---|---|
| UI Framework | WPF (.NET 8) |
| Kiến trúc | MVVM (CommunityToolkit.Mvvm để giảm boilerplate) |
| CSDL | SQL Server + EF Core (Code First) |
| Face Verification | OpenCvSharp4 + OpenCvSharp4.runtime.win (Haar Cascade detection + LBPH verification local) |
| Barcode | ZXing.Net (đọc qua webcam) hoặc input trực tiếp nếu dùng máy quét vật lý |
| AI API | OpenAI API hoặc Gemini API |
| In hóa đơn | WPF PrintDialog hoặc QuestPDF để export PDF |
| Biểu đồ báo cáo | LiveCharts2 (thư viện chart cho WPF) |

## 6. Đề xuất phân công nhóm (nếu làm nhóm)

| Vai trò | Module |
|---|---|
| Người 1 | Module chấm công (QR/mã nhân viên, camera, enrollment, face verification) |
| Người 2 | Module bán hàng (barcode, giỏ hàng, checkout) |
| Người 3 | Module quản lý sản phẩm/kho |
| Người 4 | Module AI chat + báo cáo |
| Người 5 | CSDL, kiến trúc chung, tích hợp, testing |

## 7. Rủi ro cần lưu ý

- **Face verification:** đây là tính năng nâng cao, không phải cơ chế bảo mật thực tế vì chưa có liveness detection. Ưu tiên QR/mã nhân viên để xác định người chấm công; camera chỉ xác thực 1:1 với mẫu của người đó.
- **Camera:** ánh sáng yếu, góc nghiêng làm giảm độ chính xác. Fallback bắt buộc: chấm công QR/mã nhân viên và lưu lý do khi xác thực khuôn mặt thất bại sau 3 lần.
- **Ngưỡng dừng:** nếu prototype chưa mở được webcam, đăng ký mẫu và xác thực ổn định trong môi trường demo trước cuối tuần 3, dừng face verification; hoàn thiện QR/mã nhân viên, POS và các module lõi.
- **Barcode qua webcam:** đọc chậm/không ổn định hơn máy quét vật lý — nếu có điều kiện, khuyến khích dùng máy quét mã vạch USB (giá rẻ, hoạt động như bàn phím, không cần code xử lý ảnh).
- **API AI:** cần quản lý chi phí gọi API, nên giới hạn số lần gọi hoặc cache câu trả lời phổ biến.
- **Thời gian:** module camera/face verification tốn thời gian nhất, nên làm prototype sớm và chỉ tích hợp sau khi đạt tiêu chí kiểm chứng.

## Phụ lục: Chi tiết Face Verification với OpenCvSharp (LBPH)

### Hai bước tách biệt: Detection vs Verification

**Face Detection** — "Có khuôn mặt trong ảnh không, ở đâu?"
- Dùng model có sẵn, load thẳng, không train
- Haar Cascade (`haarcascade_frontalface_default.xml`) — nhẹ, nhanh, chạy tốt trên CPU yếu
- DNN-based (SSD, YuNet) — chính xác hơn, vẫn chạy CPU được

**Face Verification** — "Khuôn mặt này có đúng với nhân viên đã xác định bằng QR/mã hay không?"
- Cần "train" LBPH, nhưng nhẹ hơn nhiều so với deep learning
- Không dùng camera để tự nhận diện toàn bộ nhân viên (1:N) trong phạm vi MVP

### Thuật toán LBPH (Local Binary Patterns Histograms)

1. Chia ảnh khuôn mặt thành các ô lưới nhỏ (grid cells)
2. Với mỗi pixel, so sánh độ sáng với 8 pixel xung quanh -> mã hóa thành pattern nhị phân
3. Tính histogram của các pattern này trong từng ô -> ghép lại thành vector đặc trưng đại diện cho khuôn mặt đó
4. Khi nhận diện: so sánh vector đặc trưng của khuôn mặt mới với các vector đã lưu (Chi-Square hoặc Euclidean distance) -> ai gần nhất là kết quả

Ưu điểm: chạy được trên CPU thường, không cần GPU, train nhanh (vài giây với vài chục ảnh), ít nhạy với thay đổi ánh sáng hơn Eigenfaces/Fisherfaces.

### Luồng Enrollment (đăng ký mẫu)

```
1. Nhân viên đứng trước camera
2. App chụp 15-20 ảnh khuôn mặt (các góc/biểu cảm hơi khác nhau)
3. Với mỗi ảnh: Face Detection -> crop vùng mặt -> resize chuẩn (200x200) -> grayscale
4. Train/cập nhật model LBPH từ danh sách ảnh và label = ID nhân viên
5. Lưu model đã train (.yml) cục bộ; CSDL chỉ lưu metadata và đường dẫn ảnh mẫu
```

Khi có nhân viên mới -> train lại với toàn bộ dữ liệu, hoặc dùng `Update()` để train tăng dần.

### Luồng xác thực lúc chấm công

```
1. Nhân viên quét QR hoặc nhập mã nhân viên để xác định EmployeeId
2. Camera chụp ảnh nhân viên đứng trước máy
2. Face Detection tìm vùng mặt
3. Crop, resize, grayscale giống lúc enrollment
4. Gọi Verify(EmployeeId, ảnh) -> trả về confidence/distance score
5. Nếu distance dưới ngưỡng cho phép -> xác nhận nhân viên, ghi vào bảng Attendance
6. Nếu không đạt -> báo "không xác thực được, vui lòng thử lại hoặc chấm công bằng QR/mã nhân viên"
```

### Code mẫu (khung sườn)

```csharp
// Pseudocode: OpenCvSharp + LBPH. Chi tiết API được xác nhận khi làm prototype.
// Enrollment: phát hiện mặt -> crop, grayscale, resize 200x200 -> lưu 15-20 ảnh mẫu/nhân viên.
// Train/update: tạo model LBPH từ toàn bộ ảnh mẫu và lưu model .yml cục bộ.
// Verification: QR/mã cung cấp EmployeeId, sau đó chỉ chấp nhận kết quả LBPH khớp EmployeeId
// và có distance nhỏ hơn threshold đã hiệu chỉnh bằng dữ liệu demo.
```

### Hạn chế cần lưu ý

- Độ chính xác giảm khi: ánh sáng yếu/ngược sáng, góc mặt nghiêng quá 30 độ, đeo khẩu trang/kính
- Với nhóm nhỏ (10-30 nhân viên) LBPH phù hợp để demo có kiểm soát; số lượng lớn hơn nên cân nhắc thuật toán mạnh hơn (vượt scope môn WPF)
- Cần xử lý trường hợp camera không detect được mặt; QR/mã nhân viên luôn là fallback chính thức
- Không tuyên bố chấm công bằng khuôn mặt có chống giả mạo, vì project không triển khai liveness detection

## Ghi chú: Azure Face API — không dùng được cho project này

Free tier Azure Face API (30.000 giao dịch/tháng) có tồn tại, nhưng tính năng **Face Identification/Verification** (chính là thứ cần để chấm công) thuộc diện **Limited Access**:
- Yêu cầu nộp form xin duyệt riêng, chỉ khả dụng ở gói Standard/Enterprise (không có ở Free tier)
- Review mất khoảng 5-10 ngày làm việc, không đảm bảo được duyệt (chủ yếu dành cho khách hàng có account manager của Microsoft)

=> Không khả thi cho deadline môn học. Dùng OpenCvSharp + OpenCV (local, LBPH) để làm prototype miễn phí, không phụ thuộc cloud API.
