using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.Progress;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.Services.Implement
{
    public class ProgressService : IProgressService
    {
        private readonly IProgressRepository _progressRepo;
        private readonly IEnrollmentRepository _enrollmentRepo;

        public ProgressService(IProgressRepository progressRepo, IEnrollmentRepository enrollmentRepo)
        {
            _progressRepo = progressRepo;
            _enrollmentRepo = enrollmentRepo;
        }

        public async Task<CourseProgressDto> GetCourseProgressAsync(string userId, Guid courseId)
        {
            var completed = await _progressRepo.CountCompletedAsync(userId, courseId);
            var total = await _progressRepo.CountTotalLessonsAsync(courseId);

            // Lấy tên khóa học
            var enrollment = await _enrollmentRepo.GetByUserAndCourseAsync(userId, courseId);
            var courseTitle = enrollment?.Course?.Title ?? string.Empty;

            return new CourseProgressDto
            {
                CourseId = courseId,
                CourseTitle = courseTitle,
                TotalLessons = total,
                CompletedLessons = completed
            };
        }

        public async Task<MarkCompleteResult> MarkLessonCompleteAsync(string userId, int lessonId, Guid courseId)
        {
            // Kiểm tra đã enroll chưa
            var isEnrolled = await _enrollmentRepo.IsEnrolledAsync(userId, courseId);
            if (!isEnrolled)
                return MarkCompleteResult.Fail("Bạn chưa ghi danh khóa học này.");

            // Đánh dấu hoàn thành
            await _progressRepo.MarkCompleteAsync(userId, lessonId);

            // Tính lại %
            var completed = await _progressRepo.CountCompletedAsync(userId, courseId);
            var total = await _progressRepo.CountTotalLessonsAsync(courseId);
            var percent = total == 0 ? 0 : Math.Round((double)completed / total * 100, 1);
            var isCourseCompleted = total > 0 && completed >= total;

            return MarkCompleteResult.Ok(percent, isCourseCompleted, completed, total);
        }

        public async Task UpdateWatchedSecondsAsync(string userId, int lessonId, int watchedSeconds)
        {
            // Validate: chỉ cho phép nếu watchedSeconds hợp lệ
            if (watchedSeconds < 0) return;

            await _progressRepo.UpdateWatchedSecondsAsync(userId, lessonId, watchedSeconds);
        }

        public async Task<int?> GetResumeLessonIdAsync(string userId, Guid courseId)
        {
            return await _progressRepo.GetNextIncompleteLessonIdAsync(userId, courseId);
        }

        public async Task<bool> IsLessonCompletedAsync(string userId, int lessonId)
        {
            var progress = await _progressRepo.GetAsync(userId, lessonId);
            return progress?.IsCompleted ?? false;
        }

        public async Task<HashSet<int>> GetCompletedLessonIdsAsync(string userId, Guid courseId)
        {
            var progresses = await _progressRepo.GetByCourseAsync(userId, courseId);
            return progresses
                .Where(p => p.IsCompleted)
                .Select(p => p.LessonId)
                .ToHashSet();
        }
    }
}
