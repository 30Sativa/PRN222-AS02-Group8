using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Areas.Admin.Pages.Notifications
{
    public class IndexModel : PageModel
    {
        private readonly INotificationService _notificationService;

        public IndexModel(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [BindProperty]
        public string BroadcastMessage { get; set; } = string.Empty;

        [BindProperty]
        public string? TargetUrl { get; set; }

        public string SuccessMessage { get; set; } = string.Empty;

        public IReadOnlyList<Notification> Notifications { get; private set; } = Array.Empty<Notification>();

        public async Task OnGetAsync()
        {
            await LoadNotificationsAsync();
        }

        public async Task<IActionResult> OnPostBroadcastAsync()
        {
            if (string.IsNullOrWhiteSpace(BroadcastMessage))
            {
                ModelState.AddModelError("BroadcastMessage", "Vui lòng nhập nội dung thông báo.");
                await LoadNotificationsAsync();
                return Page();
            }

            await _notificationService.BroadcastSystemAsync(BroadcastMessage, TargetUrl);

            SuccessMessage = "Đã gửi thông báo hệ thống thành công!";
            BroadcastMessage = string.Empty;
            TargetUrl = string.Empty;

            await LoadNotificationsAsync();
            return Page();
        }

        private async Task LoadNotificationsAsync()
        {
            Notifications = await _notificationService.GetRecentSystemBroadcastsForAdminAsync();
        }
    }
}
