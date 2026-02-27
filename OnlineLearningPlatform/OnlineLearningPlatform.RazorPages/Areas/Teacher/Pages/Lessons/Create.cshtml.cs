using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Lessons
{
    [Authorize(Roles = "Teacher")]
    public class CreateModel : PageModel
    {
        private readonly ISectionService _sectionService;
        private readonly ICourseService _courseService;
        private readonly ILessonService _lessonService;

        public CreateModel(ISectionService sectionService, ICourseService courseService, ILessonService lessonService)
        {
            _sectionService = sectionService;
            _courseService = courseService;
            _lessonService = lessonService;
        }

        [BindProperty(SupportsGet = true)]
        public int SectionId { get; set; }

        public Guid CourseId { get; set; }

        [BindProperty]
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [BindProperty]
        public LessonType LessonType { get; set; } = LessonType.Reading;

        [BindProperty]
        public int OrderIndex { get; set; } = 1;

        [BindProperty]
        public bool IsPreview { get; set; }

        [BindProperty]
        public string? Content { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            var section = await _sectionService.GetByIdAsync(SectionId);
            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found. You must create/select a valid section first.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            var course = await _courseService.GetMyCourseByIdAsync(section.CourseId, teacherId);
            if (course == null)
            {
                TempData["ErrorMessage"] = "You do not have permission for this section.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            CourseId = section.CourseId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            var section = await _sectionService.GetByIdAsync(SectionId);
            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found. You must create/select a valid section first.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            var course = await _courseService.GetMyCourseByIdAsync(section.CourseId, teacherId);
            if (course == null)
            {
                TempData["ErrorMessage"] = "You do not have permission for this section.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            CourseId = section.CourseId;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var lesson = new Lesson
            {
                SectionId = SectionId,
                Title = Title,
                LessonType = LessonType,
                OrderIndex = OrderIndex <= 0 ? 1 : OrderIndex,
                IsPreview = IsPreview,
                Content = Content
            };

            var result = await _lessonService.CreateAsync(lesson, teacherId);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Courses/Details", new { area = "Teacher", id = section.CourseId });
        }
    }
}
