using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Pages.Lessons
{
    [Authorize(Roles = "Teacher,Student")]
    public class DetailModel(ILessonService lessonService) : PageModel
    {
        public Lesson? Lesson { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Lesson = await lessonService.GetByIdAsync(id);
            if (Lesson != null)
            {
                return Page();
            }

            TempData["ErrorMessage"] = "Lesson not found.";
            return RedirectToPage("/Auth/Index");
        }
    }
}