using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface ISectionRepository
    {
        Task<List<Section>> GetByCourseAsync(Guid courseId);
        Task<Section?> GetByIdAsync(int sectionId);
        Task<Section> CreateAsync(Section section);
        Task<bool> UpdateAsync(Section section);
        Task<bool> DeleteAsync(int sectionId);
        Task<bool> IsSectionInTeacherCourseAsync(int sectionId, string teacherId);
        Task<bool> ExistsOrderIndexAsync(Guid courseId, int orderIndex, int? excludeSectionId = null);
        Task<bool> ReorderAsync(Guid courseId, List<int> orderedSectionIds);
    }
}
