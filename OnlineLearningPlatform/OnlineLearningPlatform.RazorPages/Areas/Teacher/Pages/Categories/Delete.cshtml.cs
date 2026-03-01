using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Categories
{
    [Authorize(Roles = "Teacher")]
    public class DeleteModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public DeleteModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public Category Category { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null) return NotFound();

            Category = category;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToPage("/Categories/Index", new { area = "Teacher" });
            }

            TempData["SuccessMessage"] = "Danh mục đã được xóa thành công.";
            return RedirectToPage("/Categories/Index", new { area = "Teacher" });
        }
    }
}
