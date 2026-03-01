
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.Auth.Request;
using OnlineLearningPlatform.Services.Implement;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService authService;

        public LoginModel(IAuthService authService)
        {
            this.authService = authService;
        }
        [BindProperty]
        public LoginRequest LoginRequest { get; set; }

        public void OnGet()
        {
            // Hiển thị thông báo thành công nếu vừa xác nhận email
            if (TempData["EmailConfirmed"] != null && (bool)TempData["EmailConfirmed"])
            {
                ViewData["EmailConfirmedMessage"] = "Email confirmed successfully! You can now log in.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var result = await authService.LoginAsync(LoginRequest);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                return Page();
            }
                return RedirectToPage("/Auth/Index");
            
        }
    }

}
