using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.Interface;
using System.Net;

namespace OnlineLearningPlatform.RazorPages.Pages.Auth
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly IAuthService _authService;

        public ConfirmEmailModel(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Index");
            }

            token = WebUtility.UrlDecode(token);

            var result = await _authService.ConfirmEmailAsync(userId, token);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return Page();
            }

            return RedirectToPage("/Auth/Login");
        }
    }
}
