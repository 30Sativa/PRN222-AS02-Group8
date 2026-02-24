
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
