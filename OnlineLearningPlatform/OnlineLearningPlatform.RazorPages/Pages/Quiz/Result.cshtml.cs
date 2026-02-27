using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface; 

namespace OnlineLearningPlatform.RazorPages.Pages.Quiz
{
    public class ResultModel : PageModel
    {
        private readonly IQuizService _quizService;

        public ResultModel(IQuizService quizService)
        {
            _quizService = quizService;
        }

        public QuizAttempt Attempt { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid attemptId)
        {
            Attempt = await _quizService.GetAttemptResultAsync(attemptId);

            if (Attempt == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}