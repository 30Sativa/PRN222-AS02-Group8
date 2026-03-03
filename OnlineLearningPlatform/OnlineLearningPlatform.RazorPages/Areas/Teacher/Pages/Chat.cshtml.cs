using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.Chat;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages
{
    [Authorize(Roles = "Teacher")]
    public class ChatModel : PageModel
    {
        private readonly IChatService _chatService;

        public ChatModel(IChatService chatService)
        {
            _chatService = chatService;
        }

        public List<ChatConversationDto> Conversations { get; set; } = new();
        public List<ChatMessageDto> Messages { get; set; } = new();
        public Guid? ActiveConversationId { get; set; }
        public string? ActiveOtherUserName { get; set; }
        public string? ActiveCourseTitle { get; set; }
        public string CurrentUserId { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(Guid? conversationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Auth/Login");

            CurrentUserId = userId;
            Conversations = await _chatService.GetMyConversationsAsync(userId, "Teacher");

            if (conversationId.HasValue)
            {
                if (!await _chatService.CanAccessConversationAsync(conversationId.Value, userId))
                    return RedirectToPage();

                ActiveConversationId = conversationId;
                Messages = await _chatService.GetMessagesAsync(conversationId.Value, userId);

                var conv = Conversations.FirstOrDefault(c => c.ConversationId == conversationId.Value);
                if (conv != null)
                {
                    ActiveOtherUserName = conv.OtherUserName;
                    ActiveCourseTitle = conv.CourseTitle;
                }
            }

            return Page();
        }
    }
}
