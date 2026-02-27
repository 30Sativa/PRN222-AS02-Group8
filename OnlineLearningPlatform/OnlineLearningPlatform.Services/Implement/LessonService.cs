using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.Lesson;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.Services.Implement
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ISectionRepository _sectionRepository;

        public LessonService(ILessonRepository lessonRepository, ISectionRepository sectionRepository)
        {
            _lessonRepository = lessonRepository;
            _sectionRepository = sectionRepository;
        }

        public async Task<List<Lesson>> GetBySectionAsync(int sectionId)
        {
            return await _lessonRepository.GetBySectionAsync(sectionId);
        }

        public async Task<Lesson?> GetByIdAsync(int lessonId)
        {
            return await _lessonRepository.GetByIdAsync(lessonId);
        }

        public async Task<LessonCommandResult> CreateAsync(Lesson lesson, string teacherId)
        {
            var validation = ValidateLesson(lesson);
            if (!validation.Success)
            {
                return validation;
            }

            var sectionAllowed = await _sectionRepository.IsSectionInTeacherCourseAsync(lesson.SectionId, teacherId);
            if (!sectionAllowed)
            {
                return Fail("You do not have permission to add lesson to this section.");
            }

            lesson.Title = lesson.Title.Trim();
            lesson.Content = lesson.Content?.Trim();

            var created = await _lessonRepository.CreateAsync(lesson);
            return Success("Lesson created successfully.", created);
        }

        public async Task<LessonCommandResult> UpdateAsync(Lesson lesson, string teacherId)
        {
            var validation = ValidateLesson(lesson);
            if (!validation.Success)
            {
                return validation;
            }

            var sectionAllowed = await _sectionRepository.IsSectionInTeacherCourseAsync(lesson.SectionId, teacherId);
            if (!sectionAllowed)
            {
                return Fail("You do not have permission to update lesson in this section.");
            }

            var existing = await _lessonRepository.GetByIdAsync(lesson.LessonId);
            if (existing == null)
            {
                return Fail("Lesson not found.");
            }

            existing.Title = lesson.Title.Trim();
            existing.LessonType = lesson.LessonType;
            existing.OrderIndex = lesson.OrderIndex;
            existing.IsPreview = lesson.IsPreview;
            existing.VideoStoragePath = lesson.VideoStoragePath;
            existing.VideoOriginalFileName = lesson.VideoOriginalFileName;
            existing.VideoDurationSeconds = lesson.VideoDurationSeconds;
            existing.VideoStatus = lesson.VideoStatus;
            existing.Content = lesson.Content?.Trim();

            var updated = await _lessonRepository.UpdateAsync(existing);
            if (!updated)
            {
                return Fail("Unable to update lesson.");
            }

            return Success("Lesson updated successfully.", existing);
        }

        public async Task<LessonCommandResult> DeleteAsync(int lessonId, string teacherId)
        {
            var allowed = await _lessonRepository.IsLessonInTeacherCourseAsync(lessonId, teacherId);
            if (!allowed)
            {
                return Fail("You do not have permission to delete this lesson.");
            }

            var existing = await _lessonRepository.GetByIdAsync(lessonId);
            if (existing == null)
            {
                return Fail("Lesson not found.");
            }

            var deleted = await _lessonRepository.DeleteAsync(lessonId);
            if (!deleted)
            {
                return Fail("Unable to delete lesson.");
            }

            return Success("Lesson deleted successfully.", existing);
        }

        private static LessonCommandResult ValidateLesson(Lesson lesson)
        {
            if (lesson.SectionId <= 0)
            {
                return Fail("SectionId is required.");
            }

            if (string.IsNullOrWhiteSpace(lesson.Title))
            {
                return Fail("Lesson title is required.");
            }

            if (lesson.OrderIndex < 0)
            {
                return Fail("Order index must be greater than or equal to 0.");
            }

            return new LessonCommandResult { Success = true };
        }

        private static LessonCommandResult Fail(string message)
        {
            return new LessonCommandResult { Success = false, Message = message };
        }

        private static LessonCommandResult Success(string message, Lesson lesson)
        {
            return new LessonCommandResult
            {
                Success = true,
                Message = message,
                Lesson = lesson
            };
        }
    }
}
