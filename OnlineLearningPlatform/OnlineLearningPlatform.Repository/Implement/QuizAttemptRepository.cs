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
    public class QuizAttemptRepository : IQuizAttemptRepository
    {
        private readonly ApplicationDbContext _context;

        public QuizAttemptRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<QuizAttempt?> GetByIdAsync(Guid attemptId)
        {
            return await _context.QuizAttempts
                .Include(a => a.QuizAnswers)
                .Include(a => a.Quiz)
                .FirstOrDefaultAsync(a => a.AttemptId == attemptId);
        }

        public async Task<IEnumerable<QuizAttempt>> GetUserAttemptsAsync(string userId, int quizId)
        {
            return await _context.QuizAttempts
                .Where(a => a.UserId == userId && a.QuizId == quizId)
                .OrderByDescending(a => a.AttemptedAt)
                .ToListAsync();
        }

        public async Task AddAsync(QuizAttempt attempt) => await _context.QuizAttempts.AddAsync(attempt);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }

}
