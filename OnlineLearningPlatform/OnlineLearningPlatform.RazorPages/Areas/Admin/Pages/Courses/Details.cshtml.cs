using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;
using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.RazorPages.Areas.Admin.Pages.Courses
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly ICourseService _courseService;

        public DetailsModel(ICourseService courseService)
        {
            _courseService = courseService;
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
            return RedirectToPage("/Courses/Details", new { area = "Admin", id });
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
            return RedirectToPage("/Courses/Details", new { area = "Admin", id });
        }
    }
}
