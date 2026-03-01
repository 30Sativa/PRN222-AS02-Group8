using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.User.Request;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages
{
    [Authorize(Roles = "Student")]
    public class EditProfileModel : PageModel
    {
        private readonly IUserService _userService;

        public EditProfileModel(IUserService userService)
        {
            _userService = userService;
        }

        // Input binding form
        [BindProperty]
        public UpdateUserRequest Input { get; set; } = new();

        // Email hiện tại (chỉ hiển thị, ko cho sửa)
        public string? CurrentEmail { get; set; }

        // Thông báo
        [TempData]
        public string? SuccessMessage { get; set; }
        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return;

            var userInfo = await _userService.GetUserInfoAsync(userId);
            if (userInfo == null) return;

            // Fill form với dữ liệu hiện tại
            CurrentEmail = userInfo.Email;
            Input = new UpdateUserRequest
            {
                FullName = userInfo.FullName,
                Bio = userInfo.Bio,
                PhoneNumber = userInfo.PhoneNumber
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                ErrorMessage = "Phiên đăng nhập hết hạn.";
                return RedirectToPage("/Auth/Login");
            }

            // Lấy email để hiển thị lại
            var userInfo = await _userService.GetUserInfoAsync(userId);
            CurrentEmail = userInfo?.Email;

            var result = await _userService.UpdateUserInfoAsync(userId, Input);

            if (result)
            {
                SuccessMessage = "Cập nhật hồ sơ thành công!";
            }
            else
            {
                ErrorMessage = "Cập nhật thất bại, vui lòng thử lại.";
            }

            return Page();
        }
    }
}
