using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.Section;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.Services.Implement
{
    public class SectionService : ISectionService
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly ICourseRepository _courseRepository;

        public SectionService(ISectionRepository sectionRepository, ICourseRepository courseRepository)
        {
            _sectionRepository = sectionRepository;
            _courseRepository = courseRepository;
        }

        public async Task<List<Section>> GetByCourseAsync(Guid courseId)
        {
            return await _sectionRepository.GetByCourseAsync(courseId);
        }

        public async Task<Section?> GetByIdAsync(int sectionId)
        {
            return await _sectionRepository.GetByIdAsync(sectionId);
        }

        public async Task<SectionCommandResult> CreateAsync(Guid courseId, string title, int orderIndex, string teacherId)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Fail("Section title is required.");
            }

            var normalizedOrder = orderIndex <= 0 ? 1 : orderIndex;

            var course = await _courseRepository.GetByIdAndTeacherAsync(courseId, teacherId);
            if (course == null)
            {
                return Fail("Course not found or you do not have permission.");
            }

            var duplicatedOrder = await _sectionRepository.ExistsOrderIndexAsync(courseId, normalizedOrder);
            if (duplicatedOrder)
            {
                return Fail($"Order {normalizedOrder} already exists in this course. Please choose another order.");
            }

            var section = new Section
            {
                CourseId = courseId,
                Title = title.Trim(),
                OrderIndex = normalizedOrder
            };

            var created = await _sectionRepository.CreateAsync(section);
            return Success("Section created successfully.", created);
        }

        public async Task<SectionCommandResult> UpdateAsync(int sectionId, string title, int orderIndex, string teacherId)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return Fail("Section title is required.");
            }

            var isAllowed = await _sectionRepository.IsSectionInTeacherCourseAsync(sectionId, teacherId);
            if (!isAllowed)
            {
                return Fail("You do not have permission to update this section.");
            }

            var existing = await _sectionRepository.GetByIdAsync(sectionId);
            if (existing == null)
            {
                return Fail("Section not found.");
            }

            var normalizedOrder = orderIndex <= 0 ? 1 : orderIndex;
            var duplicatedOrder = await _sectionRepository.ExistsOrderIndexAsync(existing.CourseId, normalizedOrder, sectionId);
            if (duplicatedOrder)
            {
                return Fail($"Order {normalizedOrder} already exists in this course. Please choose another order.");
            }

            existing.Title = title.Trim();
            existing.OrderIndex = normalizedOrder;

            var updated = await _sectionRepository.UpdateAsync(existing);
            if (!updated)
            {
                return Fail("Unable to update section.");
            }

            return Success("Section updated successfully.", existing);
        }

        public async Task<SectionCommandResult> DeleteAsync(int sectionId, string teacherId)
        {
            var isAllowed = await _sectionRepository.IsSectionInTeacherCourseAsync(sectionId, teacherId);
            if (!isAllowed)
            {
                return Fail("You do not have permission to delete this section.");
            }

            var existing = await _sectionRepository.GetByIdAsync(sectionId);
            if (existing == null)
            {
                return Fail("Section not found.");
            }

            var deleted = await _sectionRepository.DeleteAsync(sectionId);
            if (!deleted)
            {
                return Fail("Unable to delete section.");
            }

            return Success("Section deleted successfully.", existing);
        }

        public async Task<SectionCommandResult> ReorderAsync(Guid courseId, List<int> orderedSectionIds, string teacherId)
        {
            var course = await _courseRepository.GetByIdAndTeacherAsync(courseId, teacherId);
            if (course == null)
            {
                return Fail("Course not found or you do not have permission.");
            }

            if (orderedSectionIds == null || !orderedSectionIds.Any())
            {
                return Fail("Section order list is required.");
            }

            var ok = await _sectionRepository.ReorderAsync(courseId, orderedSectionIds);
            if (!ok)
            {
                return Fail("Unable to reorder sections.");
            }

            return new SectionCommandResult
            {
                Success = true,
                Message = "Sections reordered successfully."
            };
        }

        private static SectionCommandResult Fail(string message)
        {
            return new SectionCommandResult { Success = false, Message = message };
        }

        private static SectionCommandResult Success(string message, Section section)
        {
            return new SectionCommandResult
            {
                Success = true,
                Message = message,
                Section = section
            };
        }
    }
}
