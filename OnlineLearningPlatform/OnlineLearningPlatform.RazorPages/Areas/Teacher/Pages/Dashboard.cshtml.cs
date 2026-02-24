using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages
{
    [Authorize(Roles = "Teacher")]
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
