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
    public class EditModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly ICategoryService _categoryService;

        public EditModel(ICourseService courseService, ICategoryService categoryService)
        {
            _courseService = courseService;
            _categoryService = categoryService;
        }

        [BindProperty]
        public CourseUpsertRequest Input { get; set; } = new();

        public List<Category> Categories { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Categories = await _categoryService.GetAllAsync();

            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            var course = await _courseService.GetMyCourseByIdAsync(id, teacherId);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            Input = new CourseUpsertRequest
            {
                CourseId = course.CourseId,
                CourseCode = course.CourseCode,
                Title = course.Title,
                Slug = course.Slug,
                Description = course.Description,
                CategoryId = course.CategoryId,
                Price = course.Price,
                DiscountPrice = course.DiscountPrice,
                Language = course.Language,
                Level = course.Level,
                ThumbnailUrl = course.ThumbnailUrl
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
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

            Input.CourseId = id;
            var result = await _courseService.UpdateAsync(teacherId, Input);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Courses/Index", new { area = "Teacher" });
        }
    }
}
