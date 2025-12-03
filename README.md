# Human Resource Management System (HRM) - Dự án cá nhân
## Giới thiệu
Đây là hệ thống Quản lý Nhân sự và Tuyển dụng toàn diện được xây dựng trên nền tảng ASP.NET Core. Dự án tập trung giải quyết bài toán cốt lõi của doanh nghiệp: 
Tự động hóa quy trình tuyển dụng (từ lúc ứng viên nộp CV đến khi được duyệt), quản lý hồ sơ nhân viên, hợp đồng lao động và quy trình chấm công - tính lương chính xác. 
Hệ thống tích hợp thông báo thời gian thực và Email tự động giúp tăng cường trải nghiệm người dùng.
## Tính năng chính

### 1. Phân hệ Tuyển dụng (Recruitment & ATS)
* **Quản lý vị trí & Tin tuyển dụng:** HR đăng tải, chỉnh sửa các vị trí tuyển dụng (Job Posting) và yêu cầu chi tiết.
* **Nộp hồ sơ trực tuyến:** Ứng viên (Candidate) xem danh sách việc làm, tạo hồ sơ và nộp CV vào vị trí mong muốn.
* **Quy trình duyệt hồ sơ:**
    * Quản lý trạng thái: *Chờ duyệt → Phỏng vấn → Trúng tuyển → Từ chối*.
    * **Thông báo tự động:** Khi HR thay đổi trạng thái hồ sơ, hệ thống lập tức gửi Thông báo (SignalR) về tài khoản ứng viên và gửi Email kết quả tương ứng.

### 2. Quản lý Nhân sự & Hợp đồng
* **Hồ sơ nhân viên:** Lưu trữ tập trung thông tin cá nhân, bằng cấp. Nhân viên có thể tự cập nhật thông tin cá nhân (địa chỉ, sđt...).
* **Cơ cấu tổ chức:** Quản lý danh sách Phòng ban (Departments) và phân bổ nhân viên vào các vị trí phù hợp.
* **Quản lý Hợp đồng (Contracts):**
    * Lưu trữ chi tiết hợp đồng (Thử việc, Chính thức, Thời vụ).
    * **Cảnh báo hết hạn:** Hệ thống tự động quét và gửi Email nhắc nhở tới nhân viên và HR khi hợp đồng sắp hết hạn.
    * **Thông báo cập nhật:** Khi HR cập nhật thông tin hợp đồng, nhân viên sẽ nhận được Thông báo (SignalR).

### 3. Lương & Chấm công (Payroll)
* **Chấm công (Attendance):** Ghi nhận dữ liệu chấm công hàng ngày.
* **Tính lương:** Tự động tính toán: *Lương cơ bản + Phụ cấp - Bảo hiểm/Thuế* dựa trên ngày công thực tế.
* **Chốt lương & Phiếu lương:**
    * HR thực hiện chốt lương hàng tháng.
    * Nhân viên nhận thông báo (SignalR) ngay khi lương được chốt và có thể xem chi tiết Phiếu lương (Payslip) trên hệ thống.

### 4. Cổng thông tin Nhân viên (Self-Service)
* **Dashboard cá nhân:** Nơi nhân viên/ứng viên theo dõi mọi hoạt động.
* **Tiện ích:**
    * Xem lịch sử ứng tuyển và trạng thái hồ sơ.
    * Tra cứu thông tin hợp đồng lao động.
    * Xem lịch sử lương và chi tiết phiếu lương hàng tháng.
    * Nhận thông báo thời gian thực từ hệ thống.

### 5. Hệ thống Thông báo (Notification System)
* **Realtime (SignalR):** Đẩy thông báo tức thì cho các sự kiện: *Có ứng viên mới, Thay đổi trạng thái ứng tuyển, Cập nhật hợp đồng, Chốt lương.*
* **Email Service:** Gửi mail tự động cho các sự kiện quan trọng: *Kết quả phỏng vấn, Cảnh báo hết hạn hợp đồng.*

## Công nghệ sử dụng
- **Backend:** ASP.NET Core 9.0
- **Database:** SQL Server / Entity Framework Core
- **Realtime:** SignalR (Thông báo nội bộ)
- **Email Service:** SMTP (Gửi kết quả tuyển dụng, Phiếu lương)
- **Authentication:** RBAC với ASP.NET Core Identity
- **Documentation:** Swagger
- **Deployment:** Local
## Cấu trúc dự án
```text
DotNetHRM/
├── Areas/             # Phân hệ riêng biệt (Admin/Employee)
├── Controllers/       # Xử lý luồng điều hướng chính
├── Data/              # Database Context (DbContext)
├── DTOs/              # Đối tượng chuyển đổi dữ liệu (Data Transfer Objects)
├── Enums/             # Định nghĩa trạng thái, loại hợp đồng...
├── Migrations/        # Lịch sử thay đổi cấu trúc Database
├── Models/            # Các thực thể cơ sở dữ liệu (Domain Entities)
├── Repositories/      # Lớp truy cập dữ liệu (Repository Pattern)
├── Service/           # Xử lý nghiệp vụ (Gửi Email, Thông báo SignalR)
├── Views/             # Giao diện người dùng (Razor Views)
├── wwwroot/           # File tĩnh (CSS, JS, Ảnh)
├── appsettings.json   # Cấu hình hệ thống
└── Program.cs         # Điểm khởi chạy ứng dụng
```
## Cài đặt và chạy dự án
## Yêu cầu hệ thống
<ul>
  <li><strong>.NET 9.0 SDK</strong></li>
  <li><strong>SQL Server 2022 (hoặc bất kỳ cơ sở dữ liệu nào hỗ trợ EF Core)</strong></li>
  <li><strong>Visual Studio 2022 hoặc bất kỳ IDE nào hỗ trợ .NET</strong></li>
</ul>

## Các bước cài đặt
1. **Clone repository:**
```text
git clone https://github.com/levinh369/dotnet--hrm.git
cd dotnet--hrm
```
2. **Khôi phục các packages:**
```text
dotnet restore
```
3. **Đổi tên appsettings.template thành appsettings.json và cấu hình các thông tin sau trong appsettings.json:**
```text
{
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "MyDB": "Server=localhost;Database=QLNhanVien;User Id=sa;Password=XXX;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "EmailSettings": {
    "From": "XXX@gmail.com",
    "SenderName": "VerTex HR",
    "Password": "XXX",
    "Host": "smtp.gmail.com",
    "Port": 587
  }
}
```
Có thể tham khảo schema của database tại file data.sql

4. **Chạy ứng dụng:**
```text
dotnet run
```
