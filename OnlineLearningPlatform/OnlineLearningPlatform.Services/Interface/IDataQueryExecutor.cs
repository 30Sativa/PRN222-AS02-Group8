using OnlineLearningPlatform.Services.DTOs.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IDataQueryExecutor
    {
        Task<QueryResult> ExecuteIntentAsync(QueryIntent intent, string userId);
    }
}
