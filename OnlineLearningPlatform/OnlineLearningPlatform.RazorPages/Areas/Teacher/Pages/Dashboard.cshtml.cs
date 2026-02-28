using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.User.Response;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly IUserService _userService;

        public DashboardModel(IUserService userService)
        {
            _userService = userService;
        }

        // Thông tin giảng viên đang đăng nhập
        public UserInfoResponse? UserInfo { get; set; }

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                UserInfo = await _userService.GetUserInfoAsync(userId);
            }
        }
    }
}