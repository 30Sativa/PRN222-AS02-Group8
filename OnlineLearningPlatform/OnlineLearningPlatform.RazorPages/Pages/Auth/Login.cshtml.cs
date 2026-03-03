using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities.Identity;
using OnlineLearningPlatform.Services.DTOs.Auth.Request;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel(IAuthService authService, UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        [BindProperty]
        public LoginRequest LoginRequest { get; set; } = new();

        public IActionResult OnGet()
        {
            // Nếu đã đăng nhập rồi thì không cho vào trang Login nữa
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(Roles.Admin))
                {
                    return RedirectToPage("/Dashboard", new { area = "Admin" });
                }

                if (User.IsInRole(Roles.Teacher))
                {
                    return RedirectToPage("/Dashboard", new { area = "Teacher" });
                }

                if (User.IsInRole(Roles.Student))
                {
                    return RedirectToPage("/Dashboard", new { area = "Student" });
                }
            }

            // Hiển thị thông báo thành công nếu vừa xác nhận email
            if (TempData["EmailConfirmed"] != null && (bool)TempData["EmailConfirmed"])
            {
                ViewData["EmailConfirmedMessage"] = "Email confirmed successfully! You can now log in.";
            }

            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _authService.LoginAsync(LoginRequest);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(LoginRequest.Email);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, Roles.Admin))
                {
                    return RedirectToPage("/Dashboard", new { area = "Admin" });
                }

                if (await _userManager.IsInRoleAsync(user, Roles.Teacher))
                {
                    return RedirectToPage("/Dashboard", new { area = "Teacher" });
                }

                if (await _userManager.IsInRoleAsync(user, Roles.Student))
                {
                    return RedirectToPage("/Dashboard", new { area = "Student" });
                }
            }

            return RedirectToPage("/Auth/Index");
        }
    }
}
