using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Categories
{
    [Authorize(Roles = "Teacher")]
    public class IndexModel : PageModel
    {
        private readonly ICategoryService _categoryService;

        public IndexModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public List<Category> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await _categoryService.GetAllAsync();
        }
    }
}
