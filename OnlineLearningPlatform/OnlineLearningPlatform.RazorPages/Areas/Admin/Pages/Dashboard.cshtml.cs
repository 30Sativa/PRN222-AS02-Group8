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

        // Stats hiển thị trên Dashboard
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalAdmins { get; set; }

        public async Task OnGetAsync()
        {
            // Lấy dữ liệu stats từ BE
            TotalUsers = await _userService.CountAllUsersAsync();
            TotalStudents = await _userService.CountUsersByRoleAsync("Student");
            TotalTeachers = await _userService.CountUsersByRoleAsync("Teacher");
            TotalAdmins = await _userService.CountUsersByRoleAsync("Admin");
        }
    }
}
