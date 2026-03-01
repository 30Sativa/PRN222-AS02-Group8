using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatform.RazorPages.Hubs;
using OnlineLearningPlatform.Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Sections
{
    [Authorize(Roles = "Teacher")]
    public class CreateModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly ISectionService _sectionService;
        private readonly IHubContext<DataHub> _hub;

        public CreateModel(ICourseService courseService, ISectionService sectionService, IHubContext<DataHub> hub)
        {
            _courseService = courseService;
            _sectionService = sectionService;
            _hub = hub;
        }

        [BindProperty(SupportsGet = true)]
        public Guid CourseId { get; set; }

        [BindProperty]
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [BindProperty]
        public int OrderIndex { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync()
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            var course = await _courseService.GetMyCourseByIdAsync(CourseId, teacherId);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found. You must create/select a valid course first.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _sectionService.CreateAsync(CourseId, Title, OrderIndex, teacherId);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            // Broadcast realtime: section mới được thêm → Teacher Course Details page tự append section
            await _hub.Clients.All.SendAsync("SectionCreated", new
            {
                courseId = CourseId,
                sectionId = result.Section?.SectionId ?? 0,
                title = Title,
                orderIndex = OrderIndex
            });

            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Courses/Details", new { area = "Teacher", id = CourseId });
        }
    }
}
