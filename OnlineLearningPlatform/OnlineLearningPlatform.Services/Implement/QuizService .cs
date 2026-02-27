using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.Interface;
using OnlineLearningPlatform.Services.DTOs.Quiz;

namespace OnlineLearningPlatform.Services.Implementations
{
    public class QuizService : IQuizService
    {
        private readonly ApplicationDbContext _context;
        private readonly IQuizRepository _quizRepo;
        private readonly IQuestionRepository _questionRepo;
        private readonly IQuizAttemptRepository _attemptRepo;
        private readonly IQuizAnswerRepository _answerRepo;

        public QuizService(ApplicationDbContext context, IQuizRepository quizRepo, IQuestionRepository questionRepo, IQuizAttemptRepository attemptRepo, IQuizAnswerRepository answerRepo)
        {
            _context = context;
            _quizRepo = quizRepo;
            _questionRepo = questionRepo;
            _attemptRepo = attemptRepo;
            _answerRepo = answerRepo;
        }

        public async Task<IEnumerable<Course>> GetCoursesForInstructorAsync()
        {
            return await _context.Courses.Include(c => c.Sections).ThenInclude(s => s.Lessons).ThenInclude(l => l.Quizzes)
                .Where(c => !c.IsDeleted).ToListAsync();
        }

        public async Task<Quiz?> GetQuizDetailsAsync(int quizId) =>
            await _context.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.QuizId == quizId);

        public async Task CreateQuizAsync(Quiz quiz) { await _quizRepo.AddAsync(quiz); await _quizRepo.SaveChangesAsync(); }

        public async Task UpdateQuizAsync(Quiz quiz)
        {
            var exist = await _context.Quizzes.FindAsync(quiz.QuizId);
            if (exist != null) { exist.Title = quiz.Title; exist.PassingScore = quiz.PassingScore; exist.TimeLimitMinutes = quiz.TimeLimitMinutes; await _context.SaveChangesAsync(); }
        }

        public async Task DeleteQuizAsync(int quizId)
        {
            var quiz = await _context.Quizzes.Include(q => q.Questions).Include(q => q.QuizAttempts).ThenInclude(a => a.QuizAnswers).FirstOrDefaultAsync(q => q.QuizId == quizId);
            if (quiz == null) return;
            foreach (var att in quiz.QuizAttempts) _context.QuizAnswers.RemoveRange(att.QuizAnswers);
            _context.QuizAttempts.RemoveRange(quiz.QuizAttempts);
            _context.Questions.RemoveRange(quiz.Questions);
            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
        }

        public async Task AddQuestionAsync(Question question) { await _questionRepo.AddAsync(question); await _questionRepo.SaveChangesAsync(); }
        public async Task UpdateQuestionAsync(Question question) { _questionRepo.Update(question); await _questionRepo.SaveChangesAsync(); }
        public async Task DeleteQuestionAsync(int questionId) { _questionRepo.Delete(questionId); await _questionRepo.SaveChangesAsync(); }
        public async Task<IEnumerable<Quiz>> GetQuizzesByLessonAsync(int lessonId) => await _context.Quizzes.Include(q => q.Questions).Where(q => q.LessonId == lessonId).ToListAsync();

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
            await _attemptRepo.AddAsync(attempt); await _answerRepo.AddRangeAsync(quizAnswers); await _attemptRepo.SaveChangesAsync();
            return attempt;
        }
        public async Task<QuizAttempt?> GetAttemptResultAsync(Guid attemptId) => await _attemptRepo.GetByIdAsync(attemptId);
        public async Task<IEnumerable<QuizAttempt>> GetStudentHistoryAsync(string userId, int quizId) => await _attemptRepo.GetUserAttemptsAsync(userId, quizId);
    }
}