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

        // Thông tin user đang sửa
        public UserInfoResponse? UserInfo { get; set; }

        // Form chỉnh sửa thông tin
        [BindProperty]
        public UpdateUserRequest Input { get; set; } = new();

        // Form đổi role
        [BindProperty]
        public string? NewRole { get; set; }

        // Thông báo
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
            NewRole = UserInfo.Role;

            return Page();
        }

        // Xử lý cập nhật thông tin
        public async Task<IActionResult> OnPostUpdateInfoAsync(string id)
        {
            UserInfo = await _userService.GetUserInfoAsync(id);
            if (UserInfo == null)
            {
                ErrorMessage = "Không tìm thấy người dùng.";
                return Page();
            }

            var result = await _userService.UpdateUserInfoAsync(id, Input);
            if (result)
            {
                SuccessMessage = "Cập nhật thông tin thành công!";
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
                ErrorMessage = "Cập nhật thất bại.";
            }

            NewRole = UserInfo?.Role;
            return Page();
        }

        // Xử lý đổi role
        public async Task<IActionResult> OnPostChangeRoleAsync(string id)
        {
            UserInfo = await _userService.GetUserInfoAsync(id);
            if (UserInfo == null)
            {
                ErrorMessage = "Không tìm thấy người dùng.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(NewRole))
            {
                ErrorMessage = "Vui lòng chọn role mới.";
                Input = new UpdateUserRequest
                {
                    FullName = UserInfo.FullName,
                    Bio = UserInfo.Bio,
                    PhoneNumber = UserInfo.PhoneNumber
                };
                return Page();
            }

            var result = await _userService.ChangeUserRoleAsync(id, NewRole);
            if (result)
            {
                SuccessMessage = $"Đổi role thành '{NewRole}' thành công!";
            }
            else
            {
                ErrorMessage = "Đổi role thất bại. Kiểm tra role có hợp lệ.";
            }

            // Reload
            UserInfo = await _userService.GetUserInfoAsync(id);
            Input = new UpdateUserRequest
            {
                FullName = UserInfo?.FullName,
                Bio = UserInfo?.Bio,
                PhoneNumber = UserInfo?.PhoneNumber
            };
            NewRole = UserInfo?.Role;

            return Page();
        }
    }
}
