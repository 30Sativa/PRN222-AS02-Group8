using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatform.RazorPages.Hubs;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Categories
{
    [Authorize(Roles = "Teacher")]
    public class CreateModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        private readonly IHubContext<DataHub> _hub;

        public CreateModel(ICategoryService categoryService, IHubContext<DataHub> hub)
        {
            _categoryService = categoryService;
            _hub = hub;
        }

        [BindProperty]
        public string CategoryName { get; set; } = string.Empty;

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                ModelState.AddModelError(nameof(CategoryName), "Tên danh mục không được để trống.");
                return Page();
            }

            var result = await _categoryService.CreateAsync(CategoryName);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            // Broadcast realtime: danh mục mới → Teacher category list tự thêm row
            await _hub.Clients.All.SendAsync("CategoryCreated", new
            {
                categoryId = result.Category?.CategoryId ?? 0,
                categoryName = CategoryName
            });

            TempData["SuccessMessage"] = "Danh mục đã được tạo thành công.";
            return RedirectToPage("/Categories/Index", new { area = "Teacher" });
        }
    }
}
