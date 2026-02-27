using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Areas.Admin.Pages.Categories
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public EditModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty]
        public int CategoryId { get; set; }

        [BindProperty]
        public string CategoryName { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            CategoryId = category.CategoryId;
            CategoryName = category.CategoryName;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _categoryService.UpdateAsync(CategoryId, CategoryName);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Categories/Index", new { area = "Admin" });
        }
    }
}
