using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

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

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostBroadcastAsync()
        {
            if (string.IsNullOrWhiteSpace(BroadcastMessage))
            {
                ModelState.AddModelError("BroadcastMessage", "Vui lòng nhập nội dung thông báo.");
                return Page();
            }

            // Gửi thông báo hệ thống tới tất cả users
            await _notificationService.BroadcastSystemAsync(BroadcastMessage, TargetUrl);

            SuccessMessage = "Đã gửi thông báo hệ thống thành công!";
            BroadcastMessage = string.Empty;
            TargetUrl = string.Empty;

            return Page();
        }
    }
}
