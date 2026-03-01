using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Pages.Auth
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly IAuthService _authService;

        public ConfirmEmailModel(IAuthService authService)
        {
            _authService = authService;
        }

        public string Message { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string userId, string token)
        {
            // Nếu không có tham số, hiển thị thông báo yêu cầu click link
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                Message = "Invalid confirmation link. Please check your email and click the confirmation link.";
                return Page();
            }

            // Xác nhận email
            var result = await _authService.ConfirmEmailAsync(userId, token);

            if (!result.Success)
            {
                Message = result.Message;
                return Page();
            }

            // Thành công - redirect về Login với thông báo thành công
            TempData["EmailConfirmed"] = true;
            return RedirectToPage("/Auth/Login");
        }
    }
}
