using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Pages.Quiz
{
    public class ManageQuestionsModel : PageModel
    {
        private readonly IQuizService _quizService;
        public ManageQuestionsModel(IQuizService quizService) => _quizService = quizService;

        [BindProperty] public OnlineLearningPlatform.Models.Entities.Quiz Quiz { get; set; } = default!;
        [BindProperty] public Question NewQuestion { get; set; } = new();
        [BindProperty] public string MainContent { get; set; } = "";
        [BindProperty] public string OptA { get; set; } = "";
        [BindProperty] public string OptB { get; set; } = "";
        [BindProperty] public string OptC { get; set; } = "";
        [BindProperty] public string OptD { get; set; } = "";

        public async Task OnGetAsync(int id) => Quiz = await _quizService.GetQuizDetailsAsync(id);

        public async Task<IActionResult> OnPostUpdateQuizAsync()
        {
            ModelState.Remove("Quiz.Lesson");
            ModelState.Remove("Quiz.Questions");
            if (!ModelState.IsValid) return Page();
            await _quizService.UpdateQuizAsync(Quiz);
            return RedirectToPage(new { id = Quiz.QuizId });
        }

        public async Task<IActionResult> OnPostDeleteQuizAsync(int quizId)
        {
            await _quizService.DeleteQuizAsync(quizId);
            return RedirectToPage("/Dashboard", new { area = "Teacher" });
        }

        public async Task<IActionResult> OnPostAddQuestionAsync(int id)
        {
            NewQuestion.Content = $"{MainContent}|{OptA}|{OptB}|{OptC}|{OptD}";
            NewQuestion.QuizId = id;
            NewQuestion.QuestionType = QuestionType.MultipleChoice;
            await _quizService.AddQuestionAsync(NewQuestion);
            return RedirectToPage(new { id = id });
        }

        public async Task<IActionResult> OnPostDeleteQuestionAsync(int questionId, int quizId)
        {
            await _quizService.DeleteQuestionAsync(questionId);
            return RedirectToPage(new { id = quizId });
        }
    }
}