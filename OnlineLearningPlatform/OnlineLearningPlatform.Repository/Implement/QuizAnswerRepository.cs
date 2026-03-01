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
    public class QuizAnswerRepository : IQuizAnswerRepository
    {
        private readonly ApplicationDbContext _context;

        public QuizAnswerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddRangeAsync(IEnumerable<QuizAnswer> answers)
            => await _context.QuizAnswers.AddRangeAsync(answers);

        public async Task<IEnumerable<QuizAnswer>> GetByAttemptIdAsync(Guid attemptId)
        {
            return await _context.QuizAnswers
                .Where(a => a.AttemptId == attemptId)
                .ToListAsync();
        }
    }
}

