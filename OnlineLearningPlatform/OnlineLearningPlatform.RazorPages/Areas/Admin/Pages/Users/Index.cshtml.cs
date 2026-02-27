using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        // Danh sách users hiển thị trên trang
        public List<UserInListResponse> Users { get; set; } = new();

        // Từ khóa tìm kiếm (nếu có)
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync()
        {
            var allUsers = await _userService.GetAllUsersAsync();

            // Lọc theo từ khóa nếu có
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.Trim().ToLower();
                allUsers = allUsers.Where(u =>
                    (u.FullName?.ToLower().Contains(term) ?? false) ||
                    (u.Email?.ToLower().Contains(term) ?? false) ||
                    (u.PhoneNumber?.ToLower().Contains(term) ?? false)
                ).ToList();
            }

            Users = allUsers;
        }
    }
}
