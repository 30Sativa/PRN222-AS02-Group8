using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Areas.Admin.Pages
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly IUserService _userService;

        public DashboardModel(IUserService userService)
        {
            _userService = userService;
        }

        // Hiển thị tổng số users trên Dashboard
        public int TotalUsers { get; set; }

        public async Task OnGetAsync()
        {
            var users = await _userService.GetAllUsersAsync();
            TotalUsers = users.Count;
        }
    }
}
