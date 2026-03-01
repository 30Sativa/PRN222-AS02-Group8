using OnlineLearningPlatform.Services.DTOs.Discussion;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IDiscussionService
    {
        Task<DiscussionTopicDto?> GetTopicAsync(Guid topicId);
        Task<List<DiscussionTopicDto>> GetCourseTopicsAsync(Guid courseId, int page = 1, int pageSize = 10);
        Task<int> GetCourseTopicCountAsync(Guid courseId);
        
        Task<DiscussionTopicDto?> CreateTopicAsync(Guid courseId, int? lessonId, string title, string userId);
        Task<bool> DeleteTopicAsync(Guid topicId, string userId);

        Task<DiscussionReplyDto?> CreateReplyAsync(Guid topicId, string content, Guid? parentReplyId, string userId);
        Task<bool> DeleteReplyAsync(Guid replyId, string userId);
    }
}
