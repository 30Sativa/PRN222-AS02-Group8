using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.Course.Request;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Courses
{
    [Authorize(Roles = "Teacher")]
    public class CreateModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly ICategoryService _categoryService;

        public CreateModel(ICourseService courseService, ICategoryService categoryService)
        {
            _courseService = courseService;
            _categoryService = categoryService;
        }

        [BindProperty]
        public CourseUpsertRequest Input { get; set; } = new();

        public List<Category> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            Categories = await _categoryService.GetAllAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Categories = await _categoryService.GetAllAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            // Ensure Price defaults to 0 if not provided
            if (Input.Price < 0) Input.Price = 0;

            var result = await _courseService.CreateAsync(teacherId, Input);
            if (!result.Success || result.Course == null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["SuccessMessage"] = "🎉 Khóa học đã được tạo thành công! Bây giờ hãy thêm nội dung (sections & lessons) cho khóa học.";
            return RedirectToPage("/Courses/Details", new { area = "Teacher", id = result.Course.CourseId });
        }
    }
}
