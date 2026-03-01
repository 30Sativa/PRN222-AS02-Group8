using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnlineLearningPlatform.RazorPages.Pages.Auth
{
    public class RegisterConfirmationModel : PageModel
    {
        public string Email { get; set; }
        public void OnGet(string email)
        {
            Email = email;
        }
    }
}
