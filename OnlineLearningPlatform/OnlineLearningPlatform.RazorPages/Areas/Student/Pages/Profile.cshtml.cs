using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.User.Response;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages
{
    [Authorize(Roles = "Student")]
    public class ProfileModel : PageModel
    {
        private readonly IUserService _userService;

        public ProfileModel(IUserService userService)
        {
            _userService = userService;
        }

        // Thông tin student đang đăng nhập
        public UserInfoResponse? UserInfo { get; set; }

        public async Task OnGetAsync()
        {
            // Lấy userId từ Claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                UserInfo = await _userService.GetUserInfoAsync(userId);
            }
        }
    }
}
