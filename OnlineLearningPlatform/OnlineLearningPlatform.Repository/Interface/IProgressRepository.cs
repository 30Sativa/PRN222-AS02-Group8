using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface IProgressRepository
    {
        /// <summary>
        /// Lấy progress của một bài học.
        /// </summary>
        Task<LessonProgress?> GetAsync(string userId, int lessonId);

        /// <summary>
        /// Lấy tất cả progress của user cho một khóa học (join qua Lesson → Section → Course).
        /// </summary>
        Task<List<LessonProgress>> GetByCourseAsync(string userId, Guid courseId);

        /// <summary>
        /// Tạo hoặc cập nhật progress (mark complete, update watchedSeconds).
        /// </summary>
        Task<LessonProgress> UpsertAsync(LessonProgress progress);

        /// <summary>
        /// Đánh dấu hoàn thành bài học.
        /// </summary>
        Task<bool> MarkCompleteAsync(string userId, int lessonId);

        /// <summary>
        /// Cập nhật số giây đã xem (cho video resume).
        /// </summary>
        Task<bool> UpdateWatchedSecondsAsync(string userId, int lessonId, int watchedSeconds);

        /// <summary>
        /// Đếm số lesson đã hoàn thành trong khóa học.
        /// </summary>
        Task<int> CountCompletedAsync(string userId, Guid courseId);

        /// <summary>
        /// Đếm tổng số lesson (không bị xóa) trong khóa học.
        /// </summary>
        Task<int> CountTotalLessonsAsync(Guid courseId);

        /// <summary>
        /// Lấy bài học chưa hoàn thành đầu tiên (theo order) – dùng cho resume.
        /// </summary>
        Task<int?> GetNextIncompleteLessonIdAsync(string userId, Guid courseId);
    }
}
