using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly IQuizService _quizService;

        public DashboardModel(IQuizService quizService)
        {
            _quizService = quizService;
        }

        public IEnumerable<Course> Courses { get; set; } = new List<Course>();

        public async Task OnGetAsync()
        {
            Courses = await _quizService.GetCoursesForInstructorAsync();
        }
    }
}