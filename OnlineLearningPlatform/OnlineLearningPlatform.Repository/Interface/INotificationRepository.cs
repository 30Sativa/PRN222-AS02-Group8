using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetByUserAsync(string userId, int limit = 20);
        Task<int> CountUnreadAsync(string userId);
        Task<Notification> CreateAsync(Notification notification);
        Task MarkAllReadAsync(string userId);
        Task MarkReadAsync(int notificationId);
        Task BulkCreateAsync(List<Notification> notifications);

        /// <summary>Lấy mẫu broadcast hệ thống gần đây (gộp bản ghi trùng nội dung/phút).</summary>
        Task<List<Notification>> GetRecentSystemBroadcastsForAdminAsync(int maxDistinct = 50);
    }
}
