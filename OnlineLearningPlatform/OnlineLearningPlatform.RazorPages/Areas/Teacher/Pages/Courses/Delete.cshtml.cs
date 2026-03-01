using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.RazorPages.Hubs;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Courses
{
    [Authorize(Roles = "Teacher")]
    public class DeleteModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly IHubContext<DataHub> _hub;

        public DeleteModel(ICourseService courseService, IHubContext<DataHub> hub)
        {
            _courseService = courseService;
            _hub = hub;
        }

        public Course? Course { get; set; }

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

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            var result = await _courseService.DeleteAsync(id, teacherId);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            if (result.Success)
            {
                // Broadcast realtime: khóa học bị xóa → tất cả list tự remove card/row ngay lập tức
                await _hub.Clients.All.SendAsync("CourseDeleted", new { courseId = id });
            }

            return RedirectToPage("/Courses/Index", new { area = "Teacher" });
        }
    }
}
