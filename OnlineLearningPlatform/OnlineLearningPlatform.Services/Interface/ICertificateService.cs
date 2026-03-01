using OnlineLearningPlatform.Services.DTOs.Certificate;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface ICertificateService
    {
        /// <summary>
        /// Kiểm tra và cấp chứng chỉ nếu học viên đã hoàn thành 100% khóa học.
        /// </summary>
        Task<IssueCertificateResult> TryIssueCertificateAsync(string userId, Guid courseId);

        /// <summary>
        /// Lấy danh sách chứng chỉ của một học viên.
        /// </summary>
        Task<List<CertificateDto>> GetMyCertificatesAsync(string userId);

        /// <summary>
        /// Lấy thông tin chứng chỉ theo mã code (dùng cho verify public).
        /// </summary>
        Task<CertificateDto?> GetByCodeAsync(string code);

        /// <summary>
        /// Generate PDF bytes cho chứng chỉ.
        /// </summary>
        Task<byte[]> GeneratePdfAsync(string certificateCode);

        /// <summary>
        /// Lấy chứng chỉ theo userId và courseId.
        /// </summary>
        Task<CertificateDto?> GetByUserAndCourseAsync(string userId, Guid courseId);

        /// <summary>
        /// Lấy tất cả chứng chỉ (Admin).
        /// </summary>
        Task<List<CertificateDto>> GetAllAsync();
    }
}
