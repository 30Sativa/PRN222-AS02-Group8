using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Pages.Quiz
{
    public class CreateModel : PageModel
    {
        private readonly IQuizService _quizService;

        public CreateModel(IQuizService quizService)
        {
            _quizService = quizService;
        }

        [BindProperty]
        public OnlineLearningPlatform.Models.Entities.Quiz Quiz { get; set; } = new();

        public void OnGet(int lessonId)
        {
            Quiz.LessonId = lessonId;
        }

        public async Task<IActionResult> OnPostAsync()
        {

            ModelState.Remove("Quiz.Lesson");
            ModelState.Remove("Quiz.Questions");

            if (!ModelState.IsValid)
            {

                return Page();
            }

            try
            {
                await _quizService.CreateQuizAsync(Quiz);

                return RedirectToPage("/Quiz/ManageQuestions", new { id = Quiz.QuizId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi lưu dữ liệu: " + ex.Message);
                return Page();
            }
        }
    }
}