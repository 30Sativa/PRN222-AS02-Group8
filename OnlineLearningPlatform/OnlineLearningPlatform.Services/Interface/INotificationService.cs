using OnlineLearningPlatform.Services.DTOs.Notification;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetMyNotificationsAsync(string userId, int limit = 20);
        Task<int> CountUnreadAsync(string userId);
        Task MarkAllReadAsync(string userId);
        Task MarkReadAsync(int notificationId);

        /// <summary>
        /// Gửi thông báo đến một user cụ thể.
        /// </summary>
        Task SendToUserAsync(string userId, string notificationType, string content, string? targetUrl = null);

        /// <summary>
        /// Gửi thông báo hệ thống đến tất cả user (Announcement).
        /// </summary>
        Task BroadcastSystemAsync(string content, string? targetUrl = null);
    }
}
