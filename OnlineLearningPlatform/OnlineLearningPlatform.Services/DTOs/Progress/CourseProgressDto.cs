namespace OnlineLearningPlatform.Services.DTOs.Progress
{
    public class CourseProgressDto
    {
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }

        /// <summary>
        /// % hoàn thành (0 – 100).
        /// </summary>
        public double PercentComplete =>
            TotalLessons == 0 ? 0 : Math.Round((double)CompletedLessons / TotalLessons * 100, 1);

        public bool IsCompleted => TotalLessons > 0 && CompletedLessons >= TotalLessons;
    }
}
