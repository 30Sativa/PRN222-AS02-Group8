using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.RazorPages.Hubs;
using OnlineLearningPlatform.Services.Interface;
using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.RazorPages.Areas.Admin.Pages.Courses
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ICourseService _courseService;
        private readonly IHubContext<DataHub> _hub;

        public DetailsModel(ICourseService courseService, IHubContext<DataHub> hub)
        {
            _courseService = courseService;
            _hub = hub;
        }

        public Course? Course { get; set; }

        [BindProperty]
        [Display(Name = "Rejection Reason")]
        [Required(ErrorMessage = "Rejection reason is required.")]
        public string RejectionReason { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Course = await _courseService.GetByIdForAdminAsync(id);
            if (Course == null)
            {
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToPage("/Courses/Index", new { area = "Admin" });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid id)
        {
            var result = await _courseService.ApproveAsync(id);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            if (result.Success)
            {
                // Broadcast realtime: course status changed → Admin list & Teacher course list update immediately
                await _hub.Clients.All.SendAsync("CourseStatusChanged", new
                {
                    courseId = id,
                    status = "Published",
                    statusLabel = "✓ Xuất bản"
                });
            }

            return RedirectToPage("/Courses/Index", new { area = "Admin" });
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid id)
        {
            if (string.IsNullOrWhiteSpace(RejectionReason))
            {
                ModelState.AddModelError(nameof(RejectionReason), "Rejection reason is required.");
                Course = await _courseService.GetByIdForAdminAsync(id);
                return Page();
            }

            var result = await _courseService.RejectAsync(id, RejectionReason);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

            if (result.Success)
            {
                // Broadcast realtime: course rejected → Admin list & Teacher list update badge
                await _hub.Clients.All.SendAsync("CourseStatusChanged", new
                {
                    courseId = id,
                    status = "Rejected",
                    statusLabel = "✕ Từ chối"
                });
            }

            return RedirectToPage("/Courses/Index", new { area = "Admin" });
        }
    }
}
