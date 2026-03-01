using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface ICertificateRepository
    {
        Task<Certificate?> GetByUserAndCourseAsync(string userId, Guid courseId);
        Task<List<Certificate>> GetByUserAsync(string userId);
        Task<Certificate> CreateAsync(Certificate certificate);
        Task<bool> ExistsAsync(string userId, Guid courseId);
        Task<List<Certificate>> GetAllAsync();
    }
}
