using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using OnlineLearningPlatform.Models.Entities.Identity;
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
        public string NewPassword { get; set; }

        public void OnGet(string email, string token)
        {
            Email = email;
            Token = token;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
                return Page();

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Token));

            var result = await _userManager.ResetPasswordAsync(
                user, decodedToken, NewPassword);

            if (result.Succeeded)
                return RedirectToPage("/Auth/Login");

            return Page();
        }
    }
}
