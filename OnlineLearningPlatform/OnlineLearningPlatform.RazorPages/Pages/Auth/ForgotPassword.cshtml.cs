using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using OnlineLearningPlatform.Models.Entities.Identity;
using OnlineLearningPlatform.Services.Interface;
using System.Text;

namespace OnlineLearningPlatform.RazorPages.Pages.Auth
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public ForgotPasswordModel(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        [BindProperty]
        public string Email { get; set; }

        public bool IsSuccess { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập email.");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                // Vẫn hiển thị thành công để bảo mật (không tiết lộ email có tồn tại hay không)
                IsSuccess = true;
                return Page();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = WebEncoders.Base64UrlEncode(
                Encoding.UTF8.GetBytes(token));

            var callbackUrl = Url.Page(
                "/Auth/ResetPassword",
                pageHandler: null,
                values: new { email = Email, token = encodedToken },
                protocol: Request.Scheme);

            await _emailService.SendEmailAsync(
                Email,
                "Reset Password",
                $"Click <a href='{callbackUrl}'>here</a> to reset your password.");

            IsSuccess = true;
            return Page();
        }
    }
}
