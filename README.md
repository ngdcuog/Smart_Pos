# SmartPOS

SmartPOS là ứng dụng desktop quản lý bán hàng viết bằng **WPF + .NET 8 + Entity Framework Core + SQL Server LocalDB**. Dự án phục vụ môn PRN212 và đang có các chức năng: bán hàng, quản lý sản phẩm và kho, nhân viên, chấm công có xác thực khuôn mặt thử nghiệm, dashboard, báo cáo, trợ lý AI, quét mã vạch bằng camera và xuất hóa đơn PDF.

## Chức năng hiện có

| Khu vực | Chức năng chính |
| --- | --- |
| Bán hàng | Tìm/chọn sản phẩm, nhập hoặc quét barcode, giỏ hàng, giảm giá, thanh toán tiền mặt/chuyển khoản, trừ tồn kho, xuất hóa đơn PDF. |
| Sản phẩm | CRUD sản phẩm, danh mục, giá, tồn kho, ảnh sản phẩm, barcode đơn vị bán lẻ và barcode thùng. |
| Kho hàng | Theo dõi tồn, cảnh báo sắp hết/hết hàng, nhập theo thùng hoặc lẻ, lịch sử giao dịch kho. |
| Nhân viên | Quản lý thông tin, vai trò, trạng thái hoạt động và đăng ký lại khuôn mặt. |
| Chấm công | Check-in/check-out; mã nhân viên/QR là luồng dự phòng, khuôn mặt là xác thực thử nghiệm 1:1. |
| Dashboard & Báo cáo | KPI, doanh thu theo ngày, cảnh báo tồn, sản phẩm bán chạy, đơn hàng gần đây. |
| Trợ lý AI | Hỏi dữ liệu doanh thu, sản phẩm, tồn kho và đơn hàng qua Gemini (cần API key riêng). |

## Yêu cầu máy tính

- Windows 10/11 64-bit.
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
- Visual Studio 2022 (khuyến nghị) với workload **.NET desktop development**.
- SQL Server Express LocalDB (được cài cùng Visual Studio ở đa số máy).
- Git.
- Webcam nếu test đăng ký/xác thực khuôn mặt hoặc quét barcode bằng camera.

Kiểm tra LocalDB trong PowerShell:

```powershell
sqllocaldb info
```

Nếu thấy `MSSQLLocalDB` thì có thể chạy dự án. Nếu không có, mở Visual Studio Installer và cài **SQL Server Express LocalDB** trong Individual components.

## Clone và chạy ứng dụng

```powershell
git clone https://github.com/ngdcuog/Smart_Pos.git
cd Smart_Pos
dotnet restore .\SmartPOS.sln
dotnet build .\SmartPOS.sln
dotnet run --project .\SmartPOS\SmartPOS.csproj
```

Hoặc mở file `SmartPOS.sln` bằng Visual Studio, đặt project **SmartPOS** làm Startup Project, sau đó nhấn `F5`.

Lần chạy đầu, ứng dụng tự động:

1. Kết nối LocalDB theo connection string trong `SmartPOS/appsettings.json`.
2. Chạy toàn bộ EF Core migrations.
3. Tạo database `SmartPOSDb` và seed dữ liệu demo.

Không cần chạy `Update-Database` thủ công trong luồng bình thường.

## Cấu hình cục bộ cho từng thành viên

Không sửa hoặc commit `appsettings.json` chứa cấu hình chung. Mỗi người tạo file riêng, đã được `.gitignore`:

```powershell
Copy-Item .\SmartPOS\appsettings.Local.example.json .\SmartPOS\appsettings.Local.json
```

Mở `SmartPOS/appsettings.Local.json` và cấu hình theo máy của mình:

```json
{
  "AI": {
    "ApiKey": "DAN_GEMINI_API_KEY_CUA_BAN"
  },
  "FaceVerification": {
    "CameraIndex": 0
  },
  "BarcodeScanner": {
    "CameraIndex": 0
  }
}
```

- `CameraIndex: 0` thường là webcam laptop. Nếu máy có Iriun, DroidCam hoặc webcam USB, thử `1`, `2`... đến khi đúng camera.
- Không commit `appsettings.Local.json` và không gửi API key lên GitHub/nhóm chat.
- Nếu không cấu hình AI, ứng dụng vẫn chạy; chỉ tab **Trợ lý AI** báo chưa có API key khi gửi câu hỏi.
- Có thể dùng biến môi trường `SMARTPOS_AI_API_KEY` thay cho `AI:ApiKey`.

## Hóa đơn PDF

Sau thanh toán thành công, ứng dụng tự tạo PDF và hiển thị nút **Mở hóa đơn PDF** trong màn Bán hàng. Mở file bằng trình đọc PDF trên Windows rồi chọn Print để in.

Mặc định, khi chạy từ source code, hóa đơn được lưu tại:

```text
SmartPOS\Invoices\
```

Thư mục này được tạo tự động và không được commit. Đường dẫn được xác định động từ vị trí clone nên không phụ thuộc ổ đĩa hay tên tài khoản Windows. Nếu chạy bản publish không có file `SmartPOS.sln`, thư mục `Invoices` sẽ nằm cạnh file thực thi.

## Dữ liệu demo để test nhanh

Database có sẵn nhân viên và sản phẩm demo. Ứng dụng hiện dùng tài khoản phát triển `NV0002 - Trần Quốc Bảo` để thao tác bán hàng.

Một số barcode bán lẻ:

| Sản phẩm | Barcode |
| --- | --- |
| Nước suối Aquafina 500ml | `8934588012221` |
| Coca-Cola lon 330ml | `8934588012222` |
| Trà xanh C2 360ml | `8934588012223` |
| Mì Hảo Hảo tôm chua cay | `8934588012224` |
| Bánh Oreo Original | `8934588012225` |

Ví dụ test thanh toán:

1. Vào **Bán hàng**.
2. Nhập `8934588012221` ở ô quét mã và chọn **Thêm mã**, hoặc chọn trực tiếp sản phẩm.
3. Điều chỉnh số lượng/giảm giá nếu cần.
4. Chọn **Thanh toán**.
5. Kiểm tra thông báo thành công, nút **Mở hóa đơn PDF**, tồn kho và Dashboard/Báo cáo.

### Test quét barcode bằng camera

1. Đảm bảo `BarcodeScanner:CameraIndex` trong `appsettings.Local.json` trỏ đến camera đúng.
2. Vào **Bán hàng** và chọn **Bật camera quét mã**.
3. Đưa barcode bán lẻ vào khung hình.
4. Ứng dụng tự thêm sản phẩm vào giỏ; cùng một mã được chống quét lặp trong thời gian ngắn.
5. Barcode thùng được chặn tại POS, vì bán hàng thực hiện theo đơn vị lẻ.

### Test nhập kho theo thùng

1. Vào **Kho hàng** và chọn sản phẩm có thông tin đóng gói.
2. Chọn đơn vị nhập thùng (ví dụ Coca-Cola: 1 thùng = 24 lon) rồi nhập số thùng.
3. Xác nhận nhập kho.
4. Kiểm tra tồn kho tăng theo số đơn vị lẻ quy đổi và lịch sử giao dịch kho.

### Test khuôn mặt và chấm công

1. Vào **Nhân viên**, chọn một nhân viên và chọn **Đăng ký khuôn mặt**.
2. Mở camera đúng, lấy đủ mẫu và lưu đăng ký.
3. Vào **Chấm công**, nhập mã nhân viên rồi chọn **Xác thực khuôn mặt**.
4. Thử check-in/check-out sau khi xác thực thành công.
5. Nếu camera/xác thực không khả dụng, chọn **Dùng mã nhân viên** để đi theo luồng dự phòng mã/QR.

> Xác thực khuôn mặt là tính năng thử nghiệm. Nó không thay thế luồng chấm công bằng mã nhân viên/QR.

## Chạy kiểm thử

```powershell
cd Smart_Pos
dotnet test .\SmartPOS.sln
```

Bộ test sử dụng database LocalDB test riêng và bao phủ các luồng đơn hàng, tồn kho, báo cáo, AI configuration và xuất hóa đơn PDF.

## Reset dữ liệu demo (tùy chọn)

> Lệnh dưới đây xóa toàn bộ dữ liệu database `SmartPOSDb` trên máy hiện tại.

```powershell
cd Smart_Pos\SmartPOS
dotnet tool install --global dotnet-ef --version 8.*
dotnet ef database drop --force
dotnet run
```

Ở lần chạy tiếp theo, ứng dụng sẽ tạo lại database, migrations và seed data.

## Quy ước làm việc nhóm

1. Luôn pull trước khi bắt đầu:

   ```powershell
   git pull origin main
   ```

2. Mỗi chức năng dùng branch riêng, ví dụ `feature/invoice-history` hoặc `fix/camera-index`.
3. Không commit `bin/`, `obj/`, `Invoices/`, `.vs/`, `appsettings.Local.json` hoặc API key.
4. Trước khi push, chạy:

   ```powershell
   dotnet build .\SmartPOS.sln
   dotnet test .\SmartPOS.sln
   ```

5. Mô tả rõ trong commit: phần thay đổi, cách test và ảnh hưởng migration nếu có.
6. Nếu có migration mới, commit cả file migration và file `AppDbContextModelSnapshot.cs` tương ứng.

## Cấu trúc chính

```text
SmartPOS/
├── Data/             # AppDbContext, EF Core configuration và seed data
├── Migrations/       # EF Core migrations
├── Models/           # Entity và enum domain
├── Services/         # Business services: POS, kho, báo cáo, AI, face, invoice...
├── ViewModels/       # MVVM view models
├── Views/            # WPF views
├── Styles/           # Shared visual styles
├── SmartPOS.Tests/   # Automated tests
└── appsettings.json  # Cấu hình dùng chung, không chứa secret
```

## Xử lý sự cố thường gặp

| Vấn đề | Cách xử lý |
| --- | --- |
| Không kết nối được LocalDB | Kiểm tra `sqllocaldb info`, cài SQL Server Express LocalDB, sau đó chạy lại app. |
| Camera sai hoặc không mở | Đóng ứng dụng đang chiếm webcam (Iriun/Camera/Teams), đổi `CameraIndex` trong `appsettings.Local.json`, rồi khởi động lại app. |
| AI báo chưa cấu hình | Thêm API key vào `appsettings.Local.json` hoặc đặt biến môi trường `SMARTPOS_AI_API_KEY`. |
| Không thấy hóa đơn | Thực hiện một thanh toán thành công; thư mục `Invoices` tự được tạo. |
| UI/dữ liệu cũ sau pull | Đóng app, chạy lại `dotnet build`, rồi mở app để migrations tự chạy. |

---

Repository: <https://github.com/ngdcuog/Smart_Pos>
