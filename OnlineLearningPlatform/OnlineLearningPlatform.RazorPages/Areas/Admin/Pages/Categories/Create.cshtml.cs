using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Areas.Admin.Pages.Categories
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public CreateModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [BindProperty]
        public string CategoryName { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var result = await _categoryService.CreateAsync(CategoryName);
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
