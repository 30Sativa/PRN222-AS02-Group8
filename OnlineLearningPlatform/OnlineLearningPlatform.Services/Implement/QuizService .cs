using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.Quiz;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.Services.Implementations
{
    public class QuizService : IQuizService
    {
        private readonly IQuizRepository _quizRepo;
        private readonly IQuestionRepository _questionRepo;
        private readonly IQuizAttemptRepository _attemptRepo;
        private readonly IQuizAnswerRepository _answerRepo;
        private readonly ICourseRepository _courseRepo;

        public QuizService(IQuizRepository quizRepo, IQuestionRepository questionRepo, IQuizAttemptRepository attemptRepo, IQuizAnswerRepository answerRepo, ICourseRepository courseRepo)
        {
            _quizRepo = quizRepo;
            _questionRepo = questionRepo;
            _attemptRepo = attemptRepo;
            _answerRepo = answerRepo;
            _courseRepo = courseRepo;
        }

        public async Task<IEnumerable<Course>> GetCoursesForInstructorAsync() => await _courseRepo.GetCoursesWithSectionsAndQuizzesAsync();

        public async Task<Quiz?> GetQuizDetailsAsync(int quizId) => await _quizRepo.GetQuizWithQuestionsAsync(quizId);

        public async Task CreateQuizAsync(Quiz quiz) { await _quizRepo.AddAsync(quiz); await _quizRepo.SaveChangesAsync(); }

        public async Task UpdateQuizAsync(Quiz quiz)
        {
            var exist = await _quizRepo.GetByIdAsync(quiz.QuizId);
            if (exist != null)
            {
                exist.Title = quiz.Title; exist.PassingScore = quiz.PassingScore; exist.TimeLimitMinutes = quiz.TimeLimitMinutes;
                _quizRepo.Update(exist); await _quizRepo.SaveChangesAsync();
            }
        }

        public async Task DeleteQuizAsync(int quizId)
        {
            var quiz = await _quizRepo.GetFullQuizForDeleteAsync(quizId);
            if (quiz == null) return;
            foreach (var att in quiz.QuizAttempts) _quizRepo.DeleteRangeAnswers(att.QuizAnswers);
            _quizRepo.DeleteRangeAttempts(quiz.QuizAttempts);
            _quizRepo.DeleteRangeQuestions(quiz.Questions);
            _quizRepo.Delete(quiz);
            await _quizRepo.SaveChangesAsync();
        }

        public async Task AddQuestionAsync(Question question) { await _questionRepo.AddAsync(question); await _questionRepo.SaveChangesAsync(); }
        public async Task UpdateQuestionAsync(Question question) { _questionRepo.Update(question); await _questionRepo.SaveChangesAsync(); }
        public async Task DeleteQuestionAsync(int questionId) { _questionRepo.Delete(questionId); await _questionRepo.SaveChangesAsync(); }
        public async Task<IEnumerable<Quiz>> GetQuizzesByLessonAsync(int lessonId) => await _quizRepo.GetByLessonAsync(lessonId);

        public async Task<bool> CanStudentAttemptQuizAsync(string userId, int quizId, int? maxAttempts)
        {
            if (maxAttempts == null) return true;
            var attempts = await _attemptRepo.GetUserAttemptsAsync(userId, quizId);
            return attempts.Count() < maxAttempts;
        }

        public async Task<QuizAttempt> SubmitQuizAsync(QuizSubmissionDto submission)
        {
            var quiz = await _quizRepo.GetByIdAsync(submission.QuizId);
            var questions = await _questionRepo.GetByQuizIdAsync(submission.QuizId);
            int correctCount = 0;
            var attemptId = Guid.NewGuid();
            var quizAnswers = new List<QuizAnswer>();
            foreach (var q in questions)
            {
                var ans = submission.Answers.FirstOrDefault(a => a.QuestionId == q.QuestionId);
                bool isCorrect = ans != null && ans.UserSelectedAnswer.Trim().ToUpper() == q.CorrectAnswer.Trim().ToUpper();
                if (isCorrect) correctCount++;
                quizAnswers.Add(new QuizAnswer { AnswerId = Guid.NewGuid(), AttemptId = attemptId, QuestionId = q.QuestionId, UserAnswer = ans?.UserSelectedAnswer ?? "", IsCorrect = isCorrect });
            }
            double score = questions.Count() > 0 ? (double)correctCount / questions.Count() * 100 : 0;
            var attempt = new QuizAttempt { AttemptId = attemptId, QuizId = submission.QuizId, UserId = submission.UserId, Score = score, IsPassed = score >= quiz.PassingScore, AttemptedAt = DateTime.UtcNow };
            await _attemptRepo.AddAsync(attempt); await _answerRepo.AddRangeAsync(quizAnswers); await _quizRepo.SaveChangesAsync();
            return attempt;
        }

        public async Task<QuizAttempt?> GetAttemptResultAsync(Guid attemptId) => await _attemptRepo.GetByIdAsync(attemptId);
        public async Task<IEnumerable<QuizAttempt>> GetStudentHistoryAsync(string userId, int quizId) => await _attemptRepo.GetUserAttemptsAsync(userId, quizId);
    }
}