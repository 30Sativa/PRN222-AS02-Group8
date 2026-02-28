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

        public static MarkCompleteResult Ok(double percent, bool isCourseCompleted)
            => new()
            {
                Success = true,
                Message = "Đã đánh dấu hoàn thành.",
                PercentComplete = percent,
                IsCourseCompleted = isCourseCompleted
            };

        public static MarkCompleteResult Fail(string message)
            => new() { Success = false, Message = message };
    }
}
