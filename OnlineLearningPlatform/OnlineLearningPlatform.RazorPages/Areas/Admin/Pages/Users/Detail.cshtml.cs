using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.User.Response;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Areas.Admin.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class DetailModel : PageModel
    {
        private readonly IUserService _userService;

        public DetailModel(IUserService userService)
        {
            _userService = userService;
        }

        // Thông tin user hiển thị trên trang
        public UserInfoResponse? UserInfo { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // Kiểm tra id hợp lệ
            if (string.IsNullOrWhiteSpace(id))
                return RedirectToPage("/Users/Index");

            UserInfo = await _userService.GetUserInfoAsync(id);
            return Page();
        }
    }
}
