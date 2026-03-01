using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities.Identity;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Pages.Auth
{
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult OnPost(string provider)
        {
            var redirectUrl = Url.Page("/Auth/ExternalLogin", pageHandler: "Callback");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToPage("/Auth/Login");

            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false);

            if (signInResult.Succeeded)
                return RedirectToPage("/Auth/Index");

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var fullname = info.Principal.FindFirstValue(ClaimTypes.Name);
            var user = new ApplicationUser
            {
                UserName = email,
                FullName = fullname,
                Email = email,
                EmailConfirmed = true, // Email đã được Google xác nhận
                CreatedAt = DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                await _userManager.AddLoginAsync(user, info);
                await _userManager.AddToRoleAsync(user, Roles.Student);
                await _signInManager.SignInAsync(user, false);
                return RedirectToPage("/Auth/Index");
            }

            return RedirectToPage("/Auth/Login");
        }
    }
}