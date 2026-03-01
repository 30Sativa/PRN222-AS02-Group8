using Microsoft.AspNetCore.Identity;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Models.Entities.Identity;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.Notification;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.Services.Implement
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notifRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationService(
            INotificationRepository notifRepo,
            UserManager<ApplicationUser> userManager)
        {
            _notifRepo = notifRepo;
            _userManager = userManager;
        }

        public async Task<List<NotificationDto>> GetMyNotificationsAsync(string userId, int limit = 20)
        {
            var list = await _notifRepo.GetByUserAsync(userId, limit);
            return list.Select(ToDto).ToList();
        }

        public async Task<int> CountUnreadAsync(string userId)
        {
            return await _notifRepo.CountUnreadAsync(userId);
        }

        public async Task MarkAllReadAsync(string userId)
        {
            await _notifRepo.MarkAllReadAsync(userId);
        }

        public async Task MarkReadAsync(int notificationId)
        {
            await _notifRepo.MarkReadAsync(notificationId);
        }

        public async Task SendToUserAsync(string userId, string notificationType, string content, string? targetUrl = null)
        {
            var notif = new Notification
            {
                UserId = userId,
                Type = ParseType(notificationType),
                Content = content,
                TargetUrl = targetUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            await _notifRepo.CreateAsync(notif);
        }

        public async Task BroadcastSystemAsync(string content, string? targetUrl = null)
        {
            // Lấy tất cả user và gửi thông báo
            var users = _userManager.Users.ToList();
            var notifications = users.Select(u => new Notification
            {
                UserId = u.Id,
                Type = NotificationType.System,
                Content = content,
                TargetUrl = targetUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _notifRepo.BulkCreateAsync(notifications);
        }

        private static NotificationDto ToDto(Notification n) => new()
        {
            NotificationId = n.NotificationId,
            Type = n.Type.ToString(),
            Content = n.Content,
            TargetUrl = n.TargetUrl,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            TimeAgo = GetTimeAgo(n.CreatedAt)
        };

        private static NotificationType ParseType(string type) =>
            Enum.TryParse<NotificationType>(type, out var result) ? result : NotificationType.System;

        private static string GetTimeAgo(DateTime utcTime)
        {
            var local = utcTime.ToLocalTime();
            var diff = DateTime.Now - local;

            if (diff.TotalMinutes < 1) return "Vừa xong";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} ngày trước";
            return local.ToString("dd/MM/yyyy");
        }
    }
}
