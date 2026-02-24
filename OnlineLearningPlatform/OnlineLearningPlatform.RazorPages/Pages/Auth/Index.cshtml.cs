using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnlineLearningPlatform.RazorPages.Pages.Auth
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (User.IsInRole("Admin"))
                return RedirectToPage("/Dashboard", new { area = "Admin" });

            if (User.IsInRole("Student"))
                return RedirectToPage("/Dashboard", new { area = "Student" });

            if (User.IsInRole("Teacher"))
                return RedirectToPage("/Dashboard", new { area = "Teacher" });

            return Page();
        }
    }
}
