using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.Section;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface ISectionService
    {
        Task<List<Section>> GetByCourseAsync(Guid courseId);
        Task<Section?> GetByIdAsync(int sectionId);
        Task<SectionCommandResult> CreateAsync(Guid courseId, string title, int orderIndex, string teacherId);
        Task<SectionCommandResult> UpdateAsync(int sectionId, string title, int orderIndex, string teacherId);
        Task<SectionCommandResult> DeleteAsync(int sectionId, string teacherId);
        Task<SectionCommandResult> ReorderAsync(Guid courseId, List<int> orderedSectionIds, string teacherId);
    }
}
