using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.User.Request;
using OnlineLearningPlatform.Services.DTOs.User.Response;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Areas.Admin.Pages.Users
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IUserService _userService;

        public IndexModel(IUserService userService)
        {
            _userService = userService;
        }

        // Kết quả search
        public List<UserInListResponse> Users { get; set; } = new();

        // Tổng số kết quả (cho phân trang)
        public int TotalCount { get; set; }

        // Tổng số trang
        public int TotalPages { get; set; }

        // Search params (bind từ query string)
        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Role { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        // Thông báo
        [TempData]
        public string? SuccessMessage { get; set; }
        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            // Gọi BE search đa tiêu chí + phân trang
            var request = new SearchUserRequest
            {
                Keyword = Keyword,
                Role = Role,
                PageNumber = PageNumber < 1 ? 1 : PageNumber,
                PageSize = PageSize
            };

            var result = await _userService.SearchUsersAsync(request);
            Users = result.Users;
            TotalCount = result.TotalCount;
            TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
        }

        // Admin xóa user
        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                ErrorMessage = "ID người dùng không hợp lệ.";
                return RedirectToPage();
            }

            var result = await _userService.DeleteUserAsync(id);
            if (result)
            {
                SuccessMessage = "Xóa người dùng thành công!";
            }
            else
            {
                ErrorMessage = "Xóa thất bại. Người dùng không tồn tại.";
            }

            return RedirectToPage(new { Keyword, Role, PageNumber });
        }
    }
}
