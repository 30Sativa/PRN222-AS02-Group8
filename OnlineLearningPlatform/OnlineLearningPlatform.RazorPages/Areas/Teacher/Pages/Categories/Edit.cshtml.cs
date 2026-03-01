using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatform.RazorPages.Hubs;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Categories
{
    [Authorize(Roles = "Teacher")]
    public class EditModel : PageModel
    {
        private readonly ICategoryService _categoryService;
        private readonly IHubContext<DataHub> _hub;

        public EditModel(ICategoryService categoryService, IHubContext<DataHub> hub)
        {
            _categoryService = categoryService;
            _hub = hub;
        }

        [BindProperty]
        public int CategoryId { get; set; }

        [BindProperty]
        public string CategoryName { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();

            CategoryId = category.CategoryId;
            CategoryName = category.CategoryName;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                ModelState.AddModelError(nameof(CategoryName), "Tên danh mục không được để trống.");
                return Page();
            }

            var result = await _categoryService.UpdateAsync(CategoryId, CategoryName);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            // Broadcast realtime: danh mục được cập nhật → Teacher list tự đổi tên row
            await _hub.Clients.All.SendAsync("CategoryUpdated", new
            {
                categoryId = CategoryId,
                categoryName = CategoryName
            });

            TempData["SuccessMessage"] = "Danh mục đã được cập nhật thành công.";
            return RedirectToPage("/Categories/Index", new { area = "Teacher" });
        }
    }
}
