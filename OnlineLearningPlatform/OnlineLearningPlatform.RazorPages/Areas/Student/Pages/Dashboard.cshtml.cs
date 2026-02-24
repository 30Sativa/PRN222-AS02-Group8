using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages
{
    [Authorize(Roles = "Student")]
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
