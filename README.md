# EduLearn - Online Learning Platform

## 📚 Giới thiệu

EduLearn là một nền tảng học trực tuyến được xây dựng bằng ASP.NET Core 8.0 với Razor Pages. Dự án này được phát triển bởi Group 8 cho môn học PRN222 - Assignment 02.

## ✨ Tính năng chính

### 🔐 Xác thực và Phân quyền
- Đăng ký tài khoản với xác nhận email
- Đăng nhập với email/password
- Đăng nhập bằng Google (OAuth)
- Quên mật khẩu và đặt lại mật khẩu
- Phân quyền theo vai trò: Admin, Teacher, Student

### 👥 Vai trò người dùng
- **Admin**: Quản lý hệ thống và người dùng
- **Teacher**: Tạo và quản lý khóa học
- **Student**: Tham gia và học các khóa học

### 🎨 Giao diện
- Thiết kế hiện đại, clean với Apple-inspired design system
- Responsive design, tối ưu cho mọi thiết bị
- UI/UX được tối ưu với animations và transitions mượt mà

## 🛠️ Công nghệ sử dụng

- **Backend Framework**: ASP.NET Core 8.0 (Razor Pages)
- **Database**: SQL Server với Entity Framework Core 8.0
- **Authentication**: ASP.NET Core Identity
- **Frontend**: HTML5, CSS3, JavaScript
- **Icons**: Bootstrap Icons
- **Architecture**: Layered Architecture (Models, Repository, Services, Presentation)

## 📁 Cấu trúc dự án

```
OnlineLearningPlatform/
├── OnlineLearningPlatform.Models/          # Entity models và DbContext
│   ├── Entities/Identity/                  # Identity entities
│   └── Migrations/                         # Database migrations
├── OnlineLearningPlatform.Repository/      # Data access layer
│   ├── Interface/                          # Repository interfaces
│   └── Implement/                          # Repository implementations
├── OnlineLearningPlatform.Services/         # Business logic layer
│   ├── DTOs/                               # Data Transfer Objects
│   ├── Interface/                          # Service interfaces
│   └── Implement/                          # Service implementations
└── OnlineLearningPlatform.RazorPages/      # Presentation layer
    ├── Areas/                              # Area-based organization
    │   ├── Admin/                          # Admin area
    │   ├── Teacher/                        # Teacher area
    │   └── Student/                        # Student area
    ├── Pages/                              # Razor Pages
    │   └── Auth/                           # Authentication pages
    └── wwwroot/                            # Static files (CSS, JS, images)
```

## 🚀 Cài đặt và Chạy dự án

### Yêu cầu hệ thống
- .NET 8.0 SDK hoặc cao hơn
- SQL Server (LocalDB hoặc SQL Server Express)
- Visual Studio 2022 hoặc VS Code

### Các bước cài đặt

1. **Clone repository**
   ```bash
   git clone <repository-url>
   cd PRN222-AS02-Group8
   ```

2. **Cấu hình Database**
   - Mở file `appsettings.json` trong `OnlineLearningPlatform.RazorPages`
   - Cập nhật ConnectionString phù hợp với SQL Server của bạn:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(local);Database=OnlineLearningPlatformDb;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False"
   }
   ```

3. **Chạy Migrations**
   ```bash
   cd OnlineLearningPlatform/OnlineLearningPlatform.RazorPages
   dotnet ef database update --project ../OnlineLearningPlatform.Models
   ```

4. **Chạy ứng dụng**
   ```bash
   dotnet run
   ```
   Hoặc nhấn F5 trong Visual Studio

5. **Truy cập ứng dụng**
   - Mở trình duyệt và truy cập: `https://localhost:7088` hoặc `http://localhost:5000`

## 👤 Tài khoản Demo

Sau khi chạy migrations, hệ thống sẽ tự động seed các tài khoản demo:

- **Admin**: 
  - Email: `admin@gmail.com`
  - Password: `Admin@123`

- **Teacher**: 
  - Email: `teacher@gmail.com`
  - Password: `Teacher@123`

- **Student**: 
  - Email: `student@gmail.com`
  - Password: `Student@123`

## 📝 Các trang chính

### Authentication Pages
- `/Auth/Login` - Đăng nhập
- `/Auth/Register` - Đăng ký
- `/Auth/ForgotPassword` - Quên mật khẩu
- `/Auth/ResetPassword` - Đặt lại mật khẩu
- `/Auth/ConfirmEmail` - Xác nhận email
- `/Auth/RegisterConfirmation` - Xác nhận đăng ký

### Dashboard Pages
- `/Admin/Dashboard` - Dashboard Admin
- `/Teacher/Dashboard` - Dashboard Teacher
- `/Student/Dashboard` - Dashboard Student

## 🔧 Cấu hình

### Password Policy
Mặc định, hệ thống yêu cầu:
- Độ dài tối thiểu: 6 ký tự
- Không yêu cầu chữ hoa, chữ thường, số hoặc ký tự đặc biệt

Có thể thay đổi trong `Program.cs`:
```csharp
options.Password.RequiredLength = 6;
options.Password.RequireDigit = false;
// ...
```

### Email Configuration
Để gửi email xác nhận và reset password, cần cấu hình Email Service trong `appsettings.json`.

## 🤝 Đóng góp

Dự án này được phát triển bởi Group 8 cho môn học PRN222.

## 📄 License

Dự án này được tạo cho mục đích học tập.

## 👨‍💻 Nhóm phát triển

Group 8 - PRN222 Assignment 02

---

**Lưu ý**: Đây là dự án học tập, không sử dụng cho mục đích thương mại.
