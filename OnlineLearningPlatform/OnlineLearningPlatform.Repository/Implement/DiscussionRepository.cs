using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Repository.Implement
{
    public class DiscussionRepository : IDiscussionRepository
    {
        private readonly ApplicationDbContext _context;

        public DiscussionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DiscussionTopic?> GetTopicByIdAsync(Guid topicId)
        {
            return await _context.DiscussionTopics
                .Include(t => t.Creator)
                .Include(t => t.DiscussionReplies.Where(r => !r.IsDeleted))
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(t => t.TopicId == topicId && !t.IsDeleted);
        }

        public async Task<List<DiscussionTopic>> GetTopicsByCourseIdAsync(Guid courseId, int skip, int take)
        {
            return await _context.DiscussionTopics
                .Include(t => t.Creator)
                .Include(t => t.DiscussionReplies.Where(r => !r.IsDeleted)) // To get reply count
                .Where(t => t.CourseId == courseId && !t.IsDeleted)
                .OrderByDescending(t => t.IsPinned)
                .ThenByDescending(t => t.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<int> GetTopicCountByCourseIdAsync(Guid courseId)
        {
            return await _context.DiscussionTopics
                .CountAsync(t => t.CourseId == courseId && !t.IsDeleted);
        }

        public async Task<DiscussionTopic> CreateTopicAsync(DiscussionTopic topic)
        {
            _context.DiscussionTopics.Add(topic);
            await _context.SaveChangesAsync();
            return topic;
        }

        public async Task<bool> UpdateTopicAsync(DiscussionTopic topic)
        {
            _context.DiscussionTopics.Update(topic);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTopicAsync(Guid topicId)
        {
            var topic = await _context.DiscussionTopics.FindAsync(topicId);
            if (topic == null) return false;

            topic.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<DiscussionReply>> GetRepliesByTopicIdAsync(Guid topicId)
        {
            return await _context.DiscussionReplies
                .Include(r => r.User)
                .Where(r => r.TopicId == topicId && !r.IsDeleted)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<DiscussionReply> CreateReplyAsync(DiscussionReply reply)
        {
            _context.DiscussionReplies.Add(reply);
            await _context.SaveChangesAsync();
            return reply;
        }

        public async Task<bool> DeleteReplyAsync(Guid replyId)
        {
            var reply = await _context.DiscussionReplies.FindAsync(replyId);
            if (reply == null) return false;

            reply.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
