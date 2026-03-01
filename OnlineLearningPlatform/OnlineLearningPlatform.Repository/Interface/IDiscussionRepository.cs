using OnlineLearningPlatform.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface IDiscussionRepository
    {
        Task<DiscussionTopic?> GetTopicByIdAsync(Guid topicId);
        Task<List<DiscussionTopic>> GetTopicsByCourseIdAsync(Guid courseId, int skip, int take);
        Task<int> GetTopicCountByCourseIdAsync(Guid courseId);
        Task<DiscussionTopic> CreateTopicAsync(DiscussionTopic topic);
        Task<bool> UpdateTopicAsync(DiscussionTopic topic);
        Task<bool> DeleteTopicAsync(Guid topicId);

        Task<List<DiscussionReply>> GetRepliesByTopicIdAsync(Guid topicId);
        Task<DiscussionReply> CreateReplyAsync(DiscussionReply reply);
        Task<bool> DeleteReplyAsync(Guid replyId);
    }
}
