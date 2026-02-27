using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Courses
{
    [Authorize(Roles = "Teacher")]
    public class DetailsModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly ISectionService _sectionService;
        private readonly ILessonService _lessonService;

        public DetailsModel(ICourseService courseService, ISectionService sectionService, ILessonService lessonService)
        {
            _courseService = courseService;
            _sectionService = sectionService;
            _lessonService = lessonService;
        }

        public Course? Course { get; set; }

        public List<Section> Sections { get; set; } = new();

        public Dictionary<int, List<Lesson>> LessonsBySection { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            Course = await _courseService.GetMyCourseByIdAsync(id, teacherId);
            if (Course == null)
            {
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            Sections = await _sectionService.GetByCourseAsync(Course.CourseId);

            foreach (var section in Sections)
            {
                var lessons = await _lessonService.GetBySectionAsync(section.SectionId);
                LessonsBySection[section.SectionId] = lessons;
            }

            return Page();
        }
    }
}
