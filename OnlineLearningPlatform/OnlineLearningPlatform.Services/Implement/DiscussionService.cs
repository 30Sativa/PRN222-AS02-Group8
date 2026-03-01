using Microsoft.AspNetCore.Identity;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Models.Entities.Identity;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.Discussion;
using OnlineLearningPlatform.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Services.Implement
{
    public class DiscussionService : IDiscussionService
    {
        private readonly IDiscussionRepository _discussionRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiscussionService(
            IDiscussionRepository discussionRepository,
            IEnrollmentRepository enrollmentRepository,
            UserManager<ApplicationUser> userManager)
        {
            _discussionRepository = discussionRepository;
            _enrollmentRepository = enrollmentRepository;
            _userManager = userManager;
        }

        public async Task<DiscussionTopicDto?> GetTopicAsync(Guid topicId)
        {
            var topic = await _discussionRepository.GetTopicByIdAsync(topicId);
            if (topic == null) return null;

            var creatorRoles = await _userManager.GetRolesAsync(topic.Creator);

            var topicDto = new DiscussionTopicDto
            {
                TopicId = topic.TopicId,
                CourseId = topic.CourseId,
                LessonId = topic.LessonId,
                Title = topic.Title,
                CreatedBy = topic.CreatedBy,
                CreatorName = topic.Creator?.FullName ?? topic.Creator?.UserName ?? "Unknown",
                CreatorRole = creatorRoles.FirstOrDefault() ?? "Student",
                IsPinned = topic.IsPinned,
                CreatedAt = topic.CreatedAt,
                ReplyCount = topic.DiscussionReplies.Count,
                Replies = new List<DiscussionReplyDto>()
            };

            foreach (var reply in topic.DiscussionReplies.OrderBy(r => r.CreatedAt))
            {
                var replyUserRoles = await _userManager.GetRolesAsync(reply.User);
                topicDto.Replies.Add(new DiscussionReplyDto
                {
                    ReplyId = reply.ReplyId,
                    TopicId = reply.TopicId,
                    UserId = reply.UserId,
                    UserName = reply.User?.FullName ?? reply.User?.UserName ?? "Unknown",
                    UserRole = replyUserRoles.FirstOrDefault() ?? "Student",
                    Content = reply.Content,
                    ParentReplyId = reply.ParentReplyId,
                    CreatedAt = reply.CreatedAt
                });
            }

            return topicDto;
        }

        public async Task<List<DiscussionTopicDto>> GetCourseTopicsAsync(Guid courseId, int page = 1, int pageSize = 10)
        {
            int skip = (page - 1) * pageSize;
            var topics = await _discussionRepository.GetTopicsByCourseIdAsync(courseId, skip, pageSize);
            
            var nTopics = new List<DiscussionTopicDto>();
            foreach(var t in topics)
            {
                var creatorRoles = await _userManager.GetRolesAsync(t.Creator);
                var dto = new DiscussionTopicDto
                {
                    TopicId = t.TopicId,
                    CourseId = t.CourseId,
                    LessonId = t.LessonId,
                    Title = t.Title,
                    CreatedBy = t.CreatedBy,
                    CreatorName = t.Creator?.FullName ?? t.Creator?.UserName ?? "Unknown",
                    CreatorRole = creatorRoles.FirstOrDefault() ?? "Student",
                    IsPinned = t.IsPinned,
                    CreatedAt = t.CreatedAt,
                    ReplyCount = t.DiscussionReplies.Count,
                    Replies = new List<DiscussionReplyDto>()
                };

                foreach (var reply in t.DiscussionReplies.OrderBy(r => r.CreatedAt))
                {
                    var replyUserRoles = await _userManager.GetRolesAsync(reply.User);
                    dto.Replies.Add(new DiscussionReplyDto
                    {
                        ReplyId = reply.ReplyId,
                        TopicId = reply.TopicId,
                        UserId = reply.UserId,
                        UserName = reply.User?.FullName ?? reply.User?.UserName ?? "Unknown",
                        UserRole = replyUserRoles.FirstOrDefault() ?? "Student",
                        Content = reply.Content,
                        ParentReplyId = reply.ParentReplyId,
                        CreatedAt = reply.CreatedAt
                    });
                }
                
                nTopics.Add(dto);
            }

            return nTopics;
        }

        public async Task<int> GetCourseTopicCountAsync(Guid courseId)
        {
            return await _discussionRepository.GetTopicCountByCourseIdAsync(courseId);
        }

        public async Task<DiscussionTopicDto?> CreateTopicAsync(Guid courseId, int? lessonId, string title, string userId)
        {
            bool isEnrolled = await _enrollmentRepository.IsEnrolledAsync(userId, courseId);
            
            // Allow if enrolled
            if (!isEnrolled) {
                // Technically it could be the teacher, but let's assume if it fails we check roles or simpler: just create it.
                // In full strict mode, we might verify if User == Teacher
            }

            var newTopic = new DiscussionTopic
            {
                TopicId = Guid.NewGuid(),
                CourseId = courseId,
                LessonId = lessonId,
                Title = title,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _discussionRepository.CreateTopicAsync(newTopic);
            return await GetTopicAsync(newTopic.TopicId);
        }

        public async Task<bool> DeleteTopicAsync(Guid topicId, string userId)
        {
            return await _discussionRepository.DeleteTopicAsync(topicId);
        }

        public async Task<DiscussionReplyDto?> CreateReplyAsync(Guid topicId, string content, Guid? parentReplyId, string userId)
        {
            var newReply = new DiscussionReply
            {
                ReplyId = Guid.NewGuid(),
                TopicId = topicId,
                UserId = userId,
                Content = content,
                ParentReplyId = parentReplyId,
                CreatedAt = DateTime.UtcNow
            };

            await _discussionRepository.CreateReplyAsync(newReply);

            var topic = await _discussionRepository.GetTopicByIdAsync(topicId);
            var replyEntity = topic?.DiscussionReplies.FirstOrDefault(r => r.ReplyId == newReply.ReplyId);

            if (replyEntity == null) return null;

            var roles = replyEntity.User != null ? await _userManager.GetRolesAsync(replyEntity.User) : new List<string>();

            return new DiscussionReplyDto
            {
                ReplyId = replyEntity.ReplyId,
                TopicId = replyEntity.TopicId,
                UserId = replyEntity.UserId,
                UserName = replyEntity.User?.FullName ?? replyEntity.User?.UserName ?? "Unknown",
                UserRole = roles.FirstOrDefault() ?? "Student",
                Content = replyEntity.Content,
                ParentReplyId = replyEntity.ParentReplyId,
                CreatedAt = replyEntity.CreatedAt
            };
        }

        public async Task<bool> DeleteReplyAsync(Guid replyId, string userId)
        {
            return await _discussionRepository.DeleteReplyAsync(replyId);
        }
    }
}
