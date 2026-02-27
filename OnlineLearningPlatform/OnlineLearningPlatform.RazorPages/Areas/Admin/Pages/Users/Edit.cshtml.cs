using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.User.Request;
using OnlineLearningPlatform.Services.DTOs.User.Response;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Areas.Admin.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IUserService _userService;

        public EditModel(IUserService userService)
        {
            _userService = userService;
        }

        // Thông tin user đang sửa (dùng hiển thị email, role)
        public UserInfoResponse? UserInfo { get; set; }

        // Input binding form
        [BindProperty]
        public UpdateUserRequest Input { get; set; } = new();

        // Thông báo thành công / lỗi
        [TempData]
        public string? SuccessMessage { get; set; }
        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return RedirectToPage("/Users/Index");

            UserInfo = await _userService.GetUserInfoAsync(id);
            if (UserInfo == null)
                return Page();

            // Fill form với dữ liệu hiện tại
            Input = new UpdateUserRequest
            {
                FullName = UserInfo.FullName,
                Bio = UserInfo.Bio,
                PhoneNumber = UserInfo.PhoneNumber
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            // Lấy lại thông tin user để hiển thị nếu form lỗi
            UserInfo = await _userService.GetUserInfoAsync(id);

            if (UserInfo == null)
            {
                ErrorMessage = "Không tìm thấy người dùng.";
                return Page();
            }

            // Gọi service update
            var result = await _userService.UpdateUserInfoAsync(id, Input);

            if (result)
            {
                SuccessMessage = "Cập nhật thông tin thành công!";
                // Reload lại thông tin mới
                UserInfo = await _userService.GetUserInfoAsync(id);
                Input = new UpdateUserRequest
                {
                    FullName = UserInfo?.FullName,
                    Bio = UserInfo?.Bio,
                    PhoneNumber = UserInfo?.PhoneNumber
                };
            }
            else
            {
                ErrorMessage = "Cập nhật thất bại, vui lòng thử lại.";
            }

            return Page();
        }
    }
}
