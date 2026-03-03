using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.Chat;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.Services.Implement
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepo;
        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly ApplicationDbContext _context;

        public ChatService(IChatRepository chatRepo, IEnrollmentRepository enrollmentRepo, ApplicationDbContext context)
        {
            _chatRepo = chatRepo;
            _enrollmentRepo = enrollmentRepo;
            _context = context;
        }

        public async Task<Guid> GetOrCreateConversationAsync(string studentId, string teacherId, Guid courseId)
        {
            var existing = await _chatRepo.GetConversationAsync(studentId, teacherId, courseId);
            if (existing != null) return existing.ConversationId;

            var conv = new ChatConversation
            {
                StudentId = studentId,
                TeacherId = teacherId,
                CourseId = courseId
            };
            var created = await _chatRepo.CreateConversationAsync(conv);
            return created.ConversationId;
        }

        public async Task<List<ChatConversationDto>> GetMyConversationsAsync(string userId, string role)
        {
            List<ChatConversation> convs;
            if (role == "Teacher")
                convs = await _chatRepo.GetConversationsByTeacherAsync(userId);
            else
                convs = await _chatRepo.GetConversationsByStudentAsync(userId);

            return convs.Select(c =>
            {
                var lastMsg = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
                string otherUserId = role == "Teacher" ? c.StudentId : c.TeacherId;
                string otherUserName = role == "Teacher"
                    ? (c.Student?.FullName ?? c.Student?.UserName ?? "Student")
                    : (c.Teacher?.FullName ?? c.Teacher?.UserName ?? "Teacher");

                return new ChatConversationDto
                {
                    ConversationId = c.ConversationId,
                    OtherUserId = otherUserId,
                    OtherUserName = otherUserName,
                    CourseId = c.CourseId,
                    CourseTitle = c.Course?.Title ?? "",
                    LastMessage = lastMsg?.Content,
                    UpdatedAt = c.UpdatedAt,
                    UnreadCount = c.Messages.Count(m => !m.IsRead && m.SenderId != userId)
                };
            }).ToList();
        }

        public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid conversationId, string requesterId)
        {
            if (!await CanAccessConversationAsync(conversationId, requesterId))
                return new List<ChatMessageDto>();

            await _chatRepo.MarkMessagesReadAsync(conversationId, requesterId);
            var messages = await _chatRepo.GetMessagesAsync(conversationId);

            return messages.Select(m => new ChatMessageDto
            {
                MessageId = m.MessageId,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                SenderName = m.Sender?.FullName ?? m.Sender?.UserName ?? "Unknown",
                Content = m.Content,
                SentAt = m.SentAt,
                IsRead = m.IsRead,
                IsMine = m.SenderId == requesterId
            }).ToList();
        }

        public async Task<ChatMessageDto> SendMessageAsync(Guid conversationId, string senderId, string content)
        {
            var msg = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content.Trim()
            };

            var saved = await _chatRepo.SaveMessageAsync(msg);
            await _chatRepo.UpdateConversationTimestampAsync(conversationId);

            return new ChatMessageDto
            {
                MessageId = saved.MessageId,
                ConversationId = saved.ConversationId,
                SenderId = saved.SenderId,
                SenderName = saved.Sender?.FullName ?? saved.Sender?.UserName ?? "Unknown",
                Content = saved.Content,
                SentAt = saved.SentAt,
                IsRead = false,
                IsMine = false // sẽ được client set dựa theo senderId
            };
        }

        public async Task MarkReadAsync(Guid conversationId, string readerId)
        {
            await _chatRepo.MarkMessagesReadAsync(conversationId, readerId);
        }

        public async Task<bool> CanAccessConversationAsync(Guid conversationId, string userId)
        {
            var conv = await _chatRepo.GetConversationByIdAsync(conversationId);
            if (conv == null) return false;
            return conv.StudentId == userId || conv.TeacherId == userId;
        }

        public async Task<List<ChatConversationDto>> GetAvailableTeachersAsync(string studentId)
        {
            // Lấy các khóa học student đã mua (enrollment active)
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                .Where(e => e.UserId == studentId && e.IsActive)
                .ToListAsync();

            var result = new List<ChatConversationDto>();
            foreach (var e in enrollments)
            {
                var course = e.Course;
                if (course == null || course.Teacher == null) continue;

                // Kiểm tra đã có conversation chưa
                var existing = await _chatRepo.GetConversationAsync(studentId, course.TeacherId, course.CourseId);

                result.Add(new ChatConversationDto
                {
                    ConversationId = existing?.ConversationId ?? Guid.Empty,
                    OtherUserId = course.TeacherId,
                    OtherUserName = course.Teacher.FullName ?? course.Teacher.UserName ?? "Teacher",
                    CourseId = course.CourseId,
                    CourseTitle = course.Title,
                    LastMessage = null,
                    UpdatedAt = existing?.UpdatedAt ?? DateTime.MinValue
                });
            }

            return result;
        }
    }
}
