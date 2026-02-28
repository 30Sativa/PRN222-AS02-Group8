namespace OnlineLearningPlatform.Services.DTOs.Progress
{
    public class MarkCompleteResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// % hoàn thành mới sau khi đánh dấu.
        /// </summary>
        public double PercentComplete { get; set; }

        /// <summary>
        /// Khóa học đã hoàn thành 100% chưa.
        /// </summary>
        public bool IsCourseCompleted { get; set; }

        /// <summary>
        /// Số bài đã hoàn thành.
        /// </summary>
        public int CompletedLessons { get; set; }

        /// <summary>
        /// Tổng số bài.
        /// </summary>
        public int TotalLessons { get; set; }

        public static MarkCompleteResult Ok(double percent, bool isCourseCompleted, int completedLessons, int totalLessons)
            => new()
            {
                Success = true,
                Message = "Đã đánh dấu hoàn thành.",
                PercentComplete = percent,
                IsCourseCompleted = isCourseCompleted,
                CompletedLessons = completedLessons,
                TotalLessons = totalLessons
            };

        public static MarkCompleteResult Fail(string message)
            => new() { Success = false, Message = message };
    }
}
