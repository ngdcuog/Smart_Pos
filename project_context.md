# Project Context: SmartPOS - WPF Sales Management System

> File này dùng làm context/prompt nền cho AI Agent (Claude Code, Cursor, v.v.) khi hỗ trợ code project. Dán toàn bộ nội dung này vào đầu phiên làm việc hoặc lưu làm file tham chiếu trong repo (ví dụ `CONTEXT.md` hoặc `.claude/project.md`).

## 1. Bối cảnh môn học

- **Môn học:** PRN212 (FPT University) — Windows Presentation Foundation (WPF)
- **Yêu cầu bắt buộc:** Kiến trúc **MVVM**
- **Yêu cầu gợi ý cần tích hợp ít nhất vài mục:** record video, chat với AI (gọi API), chat AI móc vào CSDL riêng (RAG), quét QR/barcode nhập liệu, nhận diện hình ảnh/camera
- **Hình thức:** project nhóm, tự chọn topic/công nghệ miễn trong WPF

## 2. Đề tài đã chọn: Phần mềm bán hàng (POS) tích hợp

Ứng dụng desktop WPF quản lý bán hàng cho cửa hàng bán lẻ, gồm:
1. **Chấm công nhân viên bằng QR/mã nhân viên**, có xác thực khuôn mặt local 1:1 là tính năng nâng cao
2. **Bán hàng bằng quét mã vạch** sản phẩm
3. **Quản lý kho/sản phẩm**
4. **Trợ lý AI phân tích doanh thu** — chat AI trả lời dựa trên dữ liệu bán hàng thật trong CSDL (RAG kiểu SQL-context, không phải vector DB)

## 3. Tech stack bắt buộc

| Layer | Công nghệ |
|---|---|
| UI | WPF, .NET 8 |
| Kiến trúc | MVVM — dùng `CommunityToolkit.Mvvm` cho ObservableObject/RelayCommand |
| ORM/CSDL | Entity Framework Core (Code First) + SQL Server |
| Face Verification | OpenCvSharp4 + OpenCvSharp4.runtime.win — Haar Cascade cho detection, LBPH cho xác thực local 1:1. Không dùng Azure Face API |
| Barcode | ZXing.Net (đọc qua webcam) hoặc raw keyboard input nếu dùng máy quét vật lý USB |
| AI Chat | Gọi REST API tới OpenAI hoặc Gemini (dùng HttpClient, không cần SDK nặng) |
| In hóa đơn/PDF | QuestPDF hoặc WPF PrintDialog |
| Chart | LiveCharts2 |

## 4. Cấu trúc thư mục dự kiến

```
/Models
  Employee.cs
  FaceSample.cs
  Attendance.cs
  Product.cs
  Category.cs
  Order.cs
  OrderDetail.cs
  Customer.cs
  StockTransaction.cs

/Services
  FaceVerificationService.cs
  BarcodeService.cs
  AttendanceService.cs
  ProductService.cs
  OrderService.cs
  AIChatService.cs
  AppDbContext.cs

/ViewModels
  AttendanceViewModel.cs
  POSViewModel.cs
  ProductManagementViewModel.cs
  ReportViewModel.cs
  AIChatViewModel.cs
  MainViewModel.cs

/Views
  AttendanceView.xaml
  POSView.xaml
  ProductManagementView.xaml
  ReportView.xaml
  AIChatView.xaml
  MainWindow.xaml

/Helpers
  RelayCommand.cs   (nếu không dùng CommunityToolkit.Mvvm)
  BitmapConverter.cs  (convert OpenCV Mat -> WPF WriteableBitmap)
```

## 5. Schema CSDL (EF Core Code First)

```
Employee(EmployeeId PK, FullName, Email, Phone, Role[Admin|Cashier], PasswordHash, IsActive)
FaceSample(FaceSampleId PK, EmployeeId FK, ImagePath, CreatedDate)
Attendance(AttendanceId PK, EmployeeId FK, CheckInTime, CheckOutTime, Date, Status[OnTime|Late])
Category(CategoryId PK, CategoryName)
Product(ProductId PK, CategoryId FK, ProductName, Barcode[unique], CostPrice, SellingPrice, StockQuantity, MinStockAlert, ImagePath)
Order(OrderId PK, EmployeeId FK, OrderDate, TotalAmount, DiscountAmount, FinalAmount, PaymentMethod)
OrderDetail(OrderDetailId PK, OrderId FK, ProductId FK, Quantity, UnitPrice, LineTotal)
Customer(CustomerId PK, Name, Phone, LoyaltyPoints)   -- optional, phase 2
StockTransaction(StockTransactionId PK, ProductId FK, Quantity, Type[Import|Export], TransactionDate)
```

Relationships: Employee 1—N Attendance, Employee 1—N FaceSample, Employee 1—N Order, Order 1—N OrderDetail, Product 1—N OrderDetail, Category 1—N Product, Product 1—N StockTransaction.

## 6. Business rules quan trọng

- **Chấm công:** QR/mã nhân viên là luồng nhận diện chính và fallback bắt buộc. Face verification local 1:1 là tính năng nâng cao, dùng OpenCvSharp/OpenCV + LBPH; không dùng Azure Face API.
- **Enrollment:** mỗi nhân viên cần 15-20 ảnh mẫu khi đăng ký lần đầu, crop + resize (200x200) + grayscale trước khi train.
- **Verification threshold:** dùng confidence/distance score của LBPH; nếu không đạt ngưỡng hoặc camera lỗi, chấm công bằng QR/mã nhân viên và lưu lý do fallback.
- **Barcode:** ưu tiên hỗ trợ cả 2 chế độ — quét qua webcam (ZXing.Net) và nhập trực tiếp qua máy quét vật lý (hoạt động như bàn phím, không cần xử lý ảnh).
- **AI Chat / RAG:** KHÔNG dùng vector database. Vì dữ liệu bán hàng có cấu trúc (structured), context cho AI được build bằng cách query SQL trực tiếp (ví dụ top sản phẩm bán chạy trong N ngày) rồi nhét kết quả vào prompt trước khi gọi API. AI không được tự bịa số liệu — luôn phải dựa trên dữ liệu query thật.
- **Checkout:** khi tạo Order, phải trừ StockQuantity tương ứng trong Product, và ghi transaction. Không cho checkout nếu StockQuantity không đủ.

## 7. Luồng nghiệp vụ chính (pseudocode tham chiếu)

**Chấm công:**
```
QR/mã nhân viên → xác định EmployeeId → (nâng cao) FaceVerificationService.Verify(EmployeeId, frame)
→ AttendanceService.CheckIn(employeeId) → hiển thị xác nhận
```

**Bán hàng:**
```
Scan barcode → ProductService.FindByBarcode(code) → add to CartItems (ObservableCollection)
→ TotalAmount tự cập nhật (computed property + binding)
→ Checkout → OrderService.CreateOrder(cart) → trừ tồn kho → in hóa đơn
```

**AI Chat:**
```
User question → AIChatService xác định loại câu hỏi → query SQL lấy data liên quan
→ build prompt = data + question → call OpenAI/Gemini API → return answer
```

## 8. Phân công nhóm (5 người, tham khảo)

1. Module chấm công (QR/mã nhân viên, camera integration, enrollment và face verification)
2. Module bán hàng (barcode, giỏ hàng, checkout, in hóa đơn)
3. Module quản lý sản phẩm/kho (CRUD, cảnh báo tồn kho thấp)
4. Module AI chat + báo cáo/biểu đồ
5. CSDL, kiến trúc chung MVVM, tích hợp module, testing

## 9. Việc cần AI Agent hỗ trợ (điều chỉnh theo giai đoạn thực tế)

- [ ] Scaffold project structure (Models, Services, ViewModels, Views) theo mục 4
- [ ] Viết EF Core DbContext + migrations theo schema mục 5
- [ ] Implement FaceVerificationService (enrollment + verify) dùng OpenCvSharp/LBPH
- [ ] Implement BarcodeService (ZXing.Net qua webcam + fallback keyboard input)
- [ ] Xây dựng POSViewModel với giỏ hàng, tính tổng, checkout
- [ ] Xây dựng AIChatService gọi API + build context từ SQL query
- [ ] Viết Views (XAML) tương ứng cho từng ViewModel, dùng data binding chuẩn MVVM (không code-behind logic nghiệp vụ)
- [ ] Viết unit test cho Services (đặc biệt OrderService — trừ tồn kho, tính tiền)

## 10. Ràng buộc kỹ thuật khi code

- Logic nghiệp vụ đặt trong Services, KHÔNG đặt trong code-behind (.xaml.cs) hay ViewModel trực tiếp.
- ViewModel chỉ gọi Service và expose data qua ObservableProperty/Command — không chứa business logic phức tạp.
- Camera/barcode xử lý trên background thread, tránh block UI thread (dùng `Dispatcher.Invoke` khi update UI từ thread khác).
- Tất cả text hiển thị cho người dùng cuối bằng tiếng Việt (UI ngôn ngữ chính là tiếng Việt).

## 11. Database development setup

- Dùng EF Core 8 với SQL Server LocalDB trong môi trường phát triển.
- Chuỗi kết nối không chứa mật khẩu nằm tại `appsettings.json`, key `ConnectionStrings:SmartPOS`.
- Ứng dụng gọi `Database.Migrate()` khi khởi động để áp dụng migration còn thiếu; không dùng `EnsureDeleted()` hay tự xóa CSDL.
- Migration được tạo/chạy từ thư mục project bằng `dotnet tool run dotnet-ef migrations add <TênMigration>` và `dotnet tool run dotnet-ef database update`.
- Các enum nghiệp vụ (`EmployeeRole`, `AttendanceStatus`, `PaymentMethod`, `StockTransactionType`) được lưu dưới dạng chuỗi để CSDL dễ đọc.
- Các service dùng `IDbContextFactory<AppDbContext>` để mỗi thao tác dữ liệu có DbContext ngắn hạn; tránh giữ tracking state trong toàn bộ vòng đời ứng dụng WPF.
- Trước khi có authentication, POS sử dụng `DevelopmentCurrentUserService` với Cashier seed `EmployeeId = 2`; đây là identity tạm thời duy nhất được phép cung cấp nhân viên cho OrderService.
