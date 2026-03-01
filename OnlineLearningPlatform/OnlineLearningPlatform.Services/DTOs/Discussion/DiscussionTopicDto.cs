using System;
using System.Collections.Generic;

namespace OnlineLearningPlatform.Services.DTOs.Discussion
{
    public class DiscussionTopicDto
    {
        public Guid TopicId { get; set; }
        public Guid CourseId { get; set; }
        public int? LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string CreatorName { get; set; } = string.Empty;
        public string CreatorRole { get; set; } = "Student";
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ReplyCount { get; set; }
        
        public List<DiscussionReplyDto> Replies { get; set; } = new List<DiscussionReplyDto>();
    }
}
