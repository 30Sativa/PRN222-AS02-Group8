using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthService _authService;

        public LogoutModel(IAuthService authService)
        {
            _authService = authService;
        }

        // Xử lý đăng xuất và redirect về Login
        public async Task<IActionResult> OnPostAsync()
        {
            await _authService.LogoutAsync();
            return RedirectToPage("/Auth/Login");
        }
    }
}
