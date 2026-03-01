using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.RazorPages.Hubs;
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
        private readonly IHubContext<DataHub> _hub;

        public EditModel(ICourseService courseService, ICategoryService categoryService, IHubContext<DataHub> hub)
        {
            _courseService = courseService;
            _categoryService = categoryService;
            _hub = hub;
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

            // Broadcast realtime: khóa học được cập nhật → Admin và Teacher list tự refresh thông tin
            await _hub.Clients.All.SendAsync("CourseUpdated", new
            {
                courseId = id,
                title = Input.Title,
                price = Input.Price,
                level = Input.Level.ToString(),
                language = Input.Language,
                thumbnailUrl = Input.ThumbnailUrl
            });

            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Courses/Index", new { area = "Teacher" });
        }
    }
}
