using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.Notification;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Notifications
{
    public class IndexModel : PageModel
    {
        private readonly INotificationService _notifService;
        public List<NotificationDto> Notifications { get; set; } = new();

        public IndexModel(INotificationService notifService)
        {
            _notifService = notifService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Auth/Login");

            Notifications = await _notifService.GetMyNotificationsAsync(userId, 50); // Lấy 50 cái gần nhất
            return Page();
        }

        public async Task<IActionResult> OnPostMarkReadAsync(int id)
        {
            await _notifService.MarkReadAsync(id);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarkAllReadAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                await _notifService.MarkAllReadAsync(userId);
            }
            return RedirectToPage();
        }
    }
}
