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
    public class EditModel(ISectionService sectionService, IHubContext<DataHub> hub) : PageModel
    {
        [BindProperty]
        public int SectionId { get; set; }

        [BindProperty]
        public Guid CourseId { get; set; }

        [BindProperty]
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [BindProperty]
        [Range(1, int.MaxValue)]
        public int OrderIndex { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return Challenge();
            }

            var section = await sectionService.GetByIdAsync(id);
            if (section == null)
            {
                TempData["ErrorMessage"] = "Section not found.";
                return RedirectToPage("/Courses/Index", new { area = "Teacher" });
            }

            SectionId = section.SectionId;
            CourseId = section.CourseId;
            Title = section.Title;
            OrderIndex = section.OrderIndex;

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

            var result = await sectionService.UpdateAsync(SectionId, Title, OrderIndex, teacherId);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            // Broadcast realtime: section được cập nhật → Teacher Course Details tự đổi tên section
            await hub.Clients.All.SendAsync("SectionUpdated", new
            {
                courseId = CourseId,
                sectionId = SectionId,
                title = Title,
                orderIndex = OrderIndex
            });

            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("/Courses/Details", new { area = "Teacher", id = CourseId });
        }
    }
}