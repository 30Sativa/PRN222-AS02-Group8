using OnlineLearningPlatform.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface ICourseRepository
    {
        Task<IEnumerable<Course>> GetCoursesWithSectionsAndQuizzesAsync();
        Task<IEnumerable<Course>> GetPublishedCoursesWithEnrollmentsAsync();
    }
}
