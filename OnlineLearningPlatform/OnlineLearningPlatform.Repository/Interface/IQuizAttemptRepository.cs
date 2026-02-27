using OnlineLearningPlatform.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface IQuizAttemptRepository
    {
        Task<QuizAttempt?> GetByIdAsync(Guid attemptId);
        Task<IEnumerable<QuizAttempt>> GetUserAttemptsAsync(string userId, int quizId);
        Task AddAsync(QuizAttempt attempt);
        Task SaveChangesAsync();
    }
}
