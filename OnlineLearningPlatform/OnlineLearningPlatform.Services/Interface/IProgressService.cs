using OnlineLearningPlatform.Services.DTOs.Progress;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IProgressService
    {
        /// <summary>
        /// Tính % hoàn thành khóa học của user.
        /// </summary>
        Task<CourseProgressDto> GetCourseProgressAsync(string userId, Guid courseId);

        /// <summary>
        /// Đánh dấu bài học hoàn thành + trả về % mới.
        /// </summary>
        Task<MarkCompleteResult> MarkLessonCompleteAsync(string userId, int lessonId, Guid courseId);

        /// <summary>
        /// Cập nhật số giây video đã xem (dùng cho resume video).
        /// </summary>
        Task UpdateWatchedSecondsAsync(string userId, int lessonId, int watchedSeconds);

        /// <summary>
        /// Lấy lessonId tiếp theo chưa hoàn thành (resume learning).
        /// </summary>
        Task<int?> GetResumeLessonIdAsync(string userId, Guid courseId);

        /// <summary>
        /// Kiểm tra bài học đã hoàn thành chưa.
        /// </summary>
        Task<bool> IsLessonCompletedAsync(string userId, int lessonId);

        /// <summary>
        /// Lấy danh sách lessonId đã hoàn thành trong khóa – dùng cho UI.
        /// </summary>
        Task<HashSet<int>> GetCompletedLessonIdsAsync(string userId, Guid courseId);
    }
}
