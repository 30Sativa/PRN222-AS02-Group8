using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Courses
{
    [Authorize(Roles = "Teacher")]
    public class DetailsModel(
        ICourseService courseService, 
        ISectionService sectionService, 
        ILessonService lessonService,
        IReviewService reviewService,
        IDiscussionService discussionService,
        OnlineLearningPlatform.Repository.Interface.IEnrollmentRepository enrollmentRepository) : PageModel
    {
        public Course? Course { get; set; }

        public List<Section> Sections { get; set; } = new();

        public Dictionary<int, List<Lesson>> LessonsBySection { get; set; } = new();

        public List<OnlineLearningPlatform.Services.DTOs.Review.ReviewDto> Reviews { get; set; } = new();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }

        public List<OnlineLearningPlatform.Services.DTOs.Discussion.DiscussionTopicDto> Topics { get; set; } = new();
        
        public List<Enrollment> Enrollments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            Course = await courseService.GetMyCourseByIdAsync(id, teacherId);
            if (Course == null)
            {
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            Sections = await sectionService.GetByCourseAsync(Course.CourseId);

            foreach (var section in Sections)
            {
                var lessons = await lessonService.GetBySectionAsync(section.SectionId);
                LessonsBySection[section.SectionId] = lessons;
            }

            Reviews = await reviewService.GetCourseReviewsAsync(id, 1, 100);
            var stats = await reviewService.GetCourseRatingStatsAsync(id);
            AverageRating = stats.average;
            ReviewCount = stats.count;

            Topics = await discussionService.GetCourseTopicsAsync(id, 1, 100);
            
            Enrollments = await enrollmentRepository.GetByCourseIdAsync(id);

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteSectionAsync(Guid id, int sectionId)
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            var result = await sectionService.DeleteAsync(sectionId, teacherId);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToPage(new { area = "Teacher", id });
        }

        public async Task<IActionResult> OnPostDeleteLessonAsync(Guid id, int lessonId)
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            var result = await lessonService.DeleteAsync(lessonId, teacherId);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            return RedirectToPage(new { area = "Teacher", id });
        }
    }
}