namespace OnlineLearningPlatform.Services.DTOs.Notification
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? TargetUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }
}
