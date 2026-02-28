using OnlineLearningPlatform.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface IEnrollmentRepository
    {
        Task AddAsync(Enrollment enrollment);
        Task<bool> IsEnrolledAsync(string userId, Guid courseId);
        Task SaveChangesAsync();
    }
}
