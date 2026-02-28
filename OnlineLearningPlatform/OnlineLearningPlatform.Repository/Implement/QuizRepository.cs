using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Repository.Implement
{
    public class QuizRepository : IQuizRepository
    {
        private readonly ApplicationDbContext _context;
        public QuizRepository(ApplicationDbContext context) => _context = context;

        public async Task<Quiz?> GetByIdAsync(int id) => await _context.Quizzes.FindAsync(id);

        public async Task<Quiz?> GetQuizWithQuestionsAsync(int quizId) =>
            await _context.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.QuizId == quizId);

        public async Task<Quiz?> GetFullQuizForDeleteAsync(int quizId) =>
            await _context.Quizzes.Include(q => q.Questions).Include(q => q.QuizAttempts).ThenInclude(a => a.QuizAnswers)
                .FirstOrDefaultAsync(q => q.QuizId == quizId);

        public async Task<IEnumerable<Quiz>> GetByLessonAsync(int lessonId) =>
            await _context.Quizzes.Include(q => q.Questions).Where(q => q.LessonId == lessonId).ToListAsync();

        public async Task AddAsync(Quiz quiz) => await _context.Quizzes.AddAsync(quiz);
        public void Update(Quiz quiz) => _context.Quizzes.Update(quiz);
        public void Delete(Quiz quiz) => _context.Quizzes.Remove(quiz);
        public void DeleteRangeQuestions(IEnumerable<Question> questions) => _context.Questions.RemoveRange(questions);
        public void DeleteRangeAttempts(IEnumerable<QuizAttempt> attempts) => _context.QuizAttempts.RemoveRange(attempts);
        public void DeleteRangeAnswers(IEnumerable<QuizAnswer> answers) => _context.QuizAnswers.RemoveRange(answers);
        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}

