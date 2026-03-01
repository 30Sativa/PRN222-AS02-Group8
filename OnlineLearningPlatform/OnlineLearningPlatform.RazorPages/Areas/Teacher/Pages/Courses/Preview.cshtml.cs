using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.Discussion;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Courses
{
    [Authorize(Roles = "Teacher,Admin")]
    public class PreviewModel : PageModel
    {
        private readonly ISectionService _sectionService;
        private readonly ILessonService _lessonService;
        private readonly IDiscussionService _discussionService;
        private readonly ICourseService _courseService;

        public PreviewModel(
            ICourseService courseService,
            ISectionService sectionService,
            ILessonService lessonService,
            IDiscussionService discussionService)
        {
            _courseService = courseService;
            _sectionService = sectionService;
            _lessonService = lessonService;
            _discussionService = discussionService;
        }

        public Course Course { get; set; } = default!;
        public List<Section> Sections { get; set; } = new();
        public Dictionary<int, List<Lesson>> LessonsBySection { get; set; } = new();
        public Lesson? CurrentLesson { get; set; }
        public List<DiscussionTopicDto> Topics { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid courseId, int? lessonId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Challenge();

            // Admin có thể preview bất kỳ course nào, Teacher chỉ xem của chính mình
            Course? course;
            if (User.IsInRole("Admin"))
            {
                course = await _courseService.GetByIdForAdminAsync(courseId);
            }
            else
            {
                course = await _courseService.GetMyCourseByIdAsync(courseId, userId);
            }

            if (course == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khóa học hoặc bạn không có quyền xem.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            Course = course;

            // Load sections + lessons riêng để đảm bảo đủ dữ liệu (ko phụ thuộc vào include chain)
            Sections = await _sectionService.GetByCourseAsync(courseId);
            foreach (var section in Sections)
            {
                var lessons = await _lessonService.GetBySectionAsync(section.SectionId);
                LessonsBySection[section.SectionId] = lessons.Where(l => !l.IsDeleted).ToList();
            }

            // Gắn lessons back vào Section để sidebar có thể dùng
            foreach (var section in Sections)
            {
                if (LessonsBySection.TryGetValue(section.SectionId, out var lessons))
                {
                    section.Lessons = lessons;
                }
            }

            // Chọn bài hiện tại
            var allLessons = Sections
                .OrderBy(s => s.OrderIndex)
                .SelectMany(s => s.Lessons.OrderBy(l => l.OrderIndex))
                .Where(l => !l.IsDeleted)
                .ToList();

            if (lessonId != null)
                CurrentLesson = allLessons.FirstOrDefault(l => l.LessonId == lessonId.Value);

            CurrentLesson ??= allLessons.FirstOrDefault();

            // Load discussions (read-only)
            Topics = await _discussionService.GetCourseTopicsAsync(courseId, 1, 20);

            return Page();
        }
    }
}
