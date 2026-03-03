using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;

namespace OnlineLearningPlatform.Repository.Implement
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ChatConversation?> GetConversationAsync(string studentId, string teacherId, Guid courseId)
        {
            return await _context.ChatConversations
                .Include(c => c.Student)
                .Include(c => c.Teacher)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.StudentId == studentId
                                       && c.TeacherId == teacherId
                                       && c.CourseId == courseId);
        }

        public async Task<ChatConversation?> GetConversationByIdAsync(Guid conversationId)
        {
            return await _context.ChatConversations
                .Include(c => c.Student)
                .Include(c => c.Teacher)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId);
        }

        public async Task<ChatConversation> CreateConversationAsync(ChatConversation conversation)
        {
            _context.ChatConversations.Add(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        public async Task<List<ChatConversation>> GetConversationsByStudentAsync(string studentId)
        {
            return await _context.ChatConversations
                .Include(c => c.Teacher)
                .Include(c => c.Course)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .Where(c => c.StudentId == studentId)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }

        public async Task<List<ChatConversation>> GetConversationsByTeacherAsync(string teacherId)
        {
            return await _context.ChatConversations
                .Include(c => c.Student)
                .Include(c => c.Course)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                .Where(c => c.TeacherId == teacherId)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(Guid conversationId)
        {
            return await _context.ChatMessages
                .Include(m => m.Sender)
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<ChatMessage> SaveMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
            // Load sender navigation
            await _context.Entry(message).Reference(m => m.Sender).LoadAsync();
            return message;
        }

        public async Task MarkMessagesReadAsync(Guid conversationId, string readerId)
        {
            var unread = await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId
                         && m.SenderId != readerId
                         && !m.IsRead)
                .ToListAsync();

            foreach (var msg in unread)
                msg.IsRead = true;

            if (unread.Any())
                await _context.SaveChangesAsync();
        }

        public async Task UpdateConversationTimestampAsync(Guid conversationId)
        {
            var conv = await _context.ChatConversations.FindAsync(conversationId);
            if (conv != null)
            {
                conv.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
