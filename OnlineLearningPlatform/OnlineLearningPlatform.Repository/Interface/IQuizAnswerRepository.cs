using OnlineLearningPlatform.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface IQuizAnswerRepository
    {
        Task AddRangeAsync(IEnumerable<QuizAnswer> answers);
        Task<IEnumerable<QuizAnswer>> GetByAttemptIdAsync(Guid attemptId);
    }
}
