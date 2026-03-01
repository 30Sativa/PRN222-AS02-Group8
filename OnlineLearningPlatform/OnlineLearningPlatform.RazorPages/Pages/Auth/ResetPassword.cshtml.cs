using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using OnlineLearningPlatform.Models.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OnlineLearningPlatform.RazorPages.Pages.Auth
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Token { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string NewPassword { get; set; }

        public void OnGet(string email, string token)
        {
            Email = email;
            Token = token;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Token) || string.IsNullOrEmpty(NewPassword))
            {
                ModelState.AddModelError("", "Invalid request. Please use the link from your email.");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                // Không tiết lộ thông tin user không tồn tại để bảo mật
                ModelState.AddModelError("", "Invalid or expired reset link.");
                return Page();
            }

            try
            {
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Token));

                var result = await _userManager.ResetPasswordAsync(
                    user, decodedToken, NewPassword);

                if (result.Succeeded)
                    return RedirectToPage("/Auth/Login");

                // Hiển thị lỗi validation
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Invalid or expired reset token.");
            }

            return Page();
        }
    }
}
