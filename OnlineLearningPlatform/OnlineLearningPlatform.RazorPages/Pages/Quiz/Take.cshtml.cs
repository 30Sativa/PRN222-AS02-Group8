using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;
using OnlineLearningPlatform.Services.DTOs.Quiz;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Pages.Quiz
{
    public class TakeModel : PageModel
    {
        private readonly IQuizService _quizService;
        public TakeModel(IQuizService quizService) => _quizService = quizService;

        public OnlineLearningPlatform.Models.Entities.Quiz Quiz { get; set; } = default!;
        [BindProperty] public List<QuestionAnswerDto> UserAnswers { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Quiz = await _quizService.GetQuizDetailsAsync(id);
            if (Quiz == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Guest_User";

            bool canAttempt = await _quizService.CanStudentAttemptQuizAsync(userId, id, 5);
            if (!canAttempt)
            {
                TempData["ErrorMessage"] = "Bạn đã hết lượt làm bài này!";
                return RedirectToPage("/Dashboard", new { area = "Student" });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Guest_User";
            var submission = new QuizSubmissionDto { QuizId = id, UserId = userId, Answers = UserAnswers };
            var result = await _quizService.SubmitQuizAsync(submission);
            return RedirectToPage("./Result", new { attemptId = result.AttemptId });
        }
    }
}