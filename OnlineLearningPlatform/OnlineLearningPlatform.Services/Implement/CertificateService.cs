using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.Certificate;
using OnlineLearningPlatform.Services.Interface;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OnlineLearningPlatform.Services.Implement
{
    public class CertificateService : ICertificateService
    {
        private readonly ICertificateRepository _certRepo;
        private readonly IProgressRepository _progressRepo;
        private readonly ICourseRepository _courseRepo;

        public CertificateService(
            ICertificateRepository certRepo,
            IProgressRepository progressRepo,
            ICourseRepository courseRepo)
        {
            _certRepo = certRepo;
            _progressRepo = progressRepo;
            _courseRepo = courseRepo;

            // QuestPDF Community License (miễn phí)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<IssueCertificateResult> TryIssueCertificateAsync(string userId, Guid courseId)
        {
            // Kiểm tra đã cấp chưa
            if (await _certRepo.ExistsAsync(userId, courseId))
                return IssueCertificateResult.Already();

            // Kiểm tra 100% hoàn thành
            var completed = await _progressRepo.CountCompletedAsync(userId, courseId);
            var total = await _progressRepo.CountTotalLessonsAsync(courseId);

            if (total == 0 || completed < total)
                return IssueCertificateResult.Fail("Chưa hoàn thành 100% khóa học.");

            // Lấy thông tin course
            var course = await _courseRepo.GetByIdAsync(courseId);
            if (course == null)
                return IssueCertificateResult.Fail("Khóa học không tồn tại.");

            // Tạo mã chứng chỉ unique
            var code = $"CERT-{course.CourseCode}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

            var cert = new Certificate
            {
                CertificateId = Guid.NewGuid(),
                UserId = userId,
                CourseId = courseId,
                CertificateCode = code,
                IssuedAt = DateTime.UtcNow
            };

            await _certRepo.CreateAsync(cert);

            // Reload với navigation properties
            var saved = await _certRepo.GetByUserAndCourseAsync(userId, courseId);

            return IssueCertificateResult.Ok(ToDto(saved!));
        }

        public async Task<List<CertificateDto>> GetMyCertificatesAsync(string userId)
        {
            var list = await _certRepo.GetByUserAsync(userId);
            return list.Select(ToDto).ToList();
        }

        public async Task<CertificateDto?> GetByCodeAsync(string code)
        {
            // Find certificate by code via GetAllAsync (simple approach)
            var all = await _certRepo.GetAllAsync();
            var cert = all.FirstOrDefault(c => c.CertificateCode == code);
            return cert == null ? null : ToDto(cert);
        }

        public async Task<CertificateDto?> GetByUserAndCourseAsync(string userId, Guid courseId)
        {
            var cert = await _certRepo.GetByUserAndCourseAsync(userId, courseId);
            return cert == null ? null : ToDto(cert);
        }

        public async Task<List<CertificateDto>> GetAllAsync()
        {
            var list = await _certRepo.GetAllAsync();
            return list.Select(ToDto).ToList();
        }

        public Task<byte[]> GeneratePdfAsync(string certificateCode)
        {
            // Tìm certificate để lấy data — dùng sync wrapper vì QuestPDF là sync
            // Gọi async GetByCodeAsync trong calling thread
            return Task.Run(async () =>
            {
                var dto = await GetByCodeAsync(certificateCode);
                if (dto == null)
                    throw new InvalidOperationException("Chứng chỉ không tồn tại.");

                return GeneratePdfBytes(dto);
            });
        }

        // ========== PDF Generation với QuestPDF ==========

        private byte[] GeneratePdfBytes(CertificateDto dto)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(0);
                    
                    // Change to Tahoma or Segoe UI for better Vietnamese support and bold weights
                    page.DefaultTextStyle(x => x.FontFamily("Tahoma"));

                    page.Content().Element(ComposeContent(dto));
                });
            }).GeneratePdf();
        }

        private static Action<IContainer> ComposeContent(CertificateDto dto)
        {
            return container =>
            {
                var bgContainerColor = "#ffffff";
                var blueBrand = "#0066cc";
                var darkText = "#1d1d1f";
                var grayText = "#86868b";
                var lightGrayBorder = "#e5e5ea";

                container
                    .Background(bgContainerColor)
                    // Border effect for whole certificate
                    .Padding(20)
                    .Border(4)
                    .BorderColor("#d1d1d6") // Subtle border color
                    .Padding(40)
                    .Column(col =>
                    {
                        // ---- Top Graphic Detail ----
                        col.Item()
                            .AlignCenter()
                            .Width(60)
                            .Height(4)
                            .Background(blueBrand);

                        col.Item().Height(30);

                        // ---- Logo / Brand ----
                        col.Item()
                            .AlignCenter()
                            .Text("EduLearn")
                            .FontSize(28)
                            .FontColor(blueBrand)
                            .Bold();

                        col.Item().Height(8);

                        col.Item()
                            .AlignCenter()
                            .Text("BẰNG CHỨNG NHẬN HOÀN THÀNH KHÓA HỌC")
                            .FontSize(16)
                            .FontColor(darkText)
                            .SemiBold()
                            .LetterSpacing(1);

                        col.Item().Height(40);

                        // ---- Main heading ----
                        col.Item()
                            .AlignCenter()
                            .Text("Cấp cho học viên")
                            .FontSize(14)
                            .FontColor(grayText);

                        col.Item().Height(16);

                        col.Item()
                            .AlignCenter()
                            .Text(dto.UserFullName.ToUpper()) // Uppercase name looks better
                            .FontSize(42)
                            .FontColor(blueBrand) // Highlight user name
                            .Bold();

                        col.Item().Height(20);

                        col.Item()
                            .AlignCenter()
                            .Text("vì đã hoàn thành xuất sắc khóa học")
                            .FontSize(14)
                            .FontColor(grayText);

                        col.Item().Height(20);

                        // ---- Course title ----
                        col.Item()
                            .AlignCenter()
                            .Text(dto.CourseTitle)
                            .FontSize(26)
                            .FontColor(darkText)
                            .Bold();

                        col.Item().Height(40);

                        // ---- Signatures and Info row ----
                        col.Item()
                            .PaddingHorizontal(40)
                            .Row(row =>
                            {
                                // Left: Instructor Signature
                                row.RelativeItem()
                                    .Column(left =>
                                    {
                                        left.Item().Text("Giảng viên hướng dẫn")
                                            .FontSize(12).FontColor(grayText).Italic();
                                        left.Item().Height(24);
                                        left.Item().Text(dto.TeacherName)
                                            .FontSize(18).FontColor(darkText).SemiBold();
                                        left.Item().Width(120).Height(1).Background(lightGrayBorder);
                                    });

                                // Center: Medal/Badge icon place
                                row.RelativeItem()
                                    .AlignCenter()
                                    .Column(center =>
                                    {
                                        center.Item()
                                            // Fallback emoji / large text
                                            .Text("🏆")
                                            .FontSize(48);
                                    });

                                // Right: Date Info
                                row.RelativeItem()
                                    .AlignRight()
                                    .Column(right =>
                                    {
                                        right.Item().AlignRight().Text("Ngày cấp")
                                            .FontSize(12).FontColor(grayText);
                                        right.Item().Height(28);
                                        right.Item().AlignRight().Text(dto.IssuedAt.ToLocalTime().ToString("dd/MM/yyyy"))
                                            .FontSize(16).FontColor(darkText).SemiBold();
                                        right.Item().Width(100).Height(1).Background(lightGrayBorder);
                                    });
                            });

                        // Spacer to push footer down
                        col.Item().ExtendVertical();

                        // ---- Certificate code & Verification ----
                        col.Item()
                            .AlignCenter()
                            .Text($"Mã xác minh: {dto.CertificateCode} • EduLearn.vn")
                            .FontSize(10)
                            .FontColor(grayText)
                            .LetterSpacing(1);
                    });
            };
        }



        private static CertificateDto ToDto(Certificate c) => new()
        {
            CertificateId = c.CertificateId,
            UserId = c.UserId,
            UserFullName = c.User?.FullName ?? "N/A",
            UserEmail = c.User?.Email ?? "",
            CourseId = c.CourseId,
            CourseTitle = c.Course?.Title ?? "N/A",
            TeacherName = c.Course?.Teacher?.FullName ?? "N/A",
            CertificateCode = c.CertificateCode,
            IssuedAt = c.IssuedAt
        };
    }
}
