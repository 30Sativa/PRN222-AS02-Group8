using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities.Identity;
using OnlineLearningPlatform.Services.DTOs.AiChat;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages
{
    public class AiChatModel : PageModel
    {
        private readonly IAiChatService _aiChatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public List<AiMessageDto> Messages { get; set; } = new();
        public List<AiConversationSummaryDto> Conversations { get; set; } = new();
        public Guid? CurrentConversationId { get; set; }

        public AiChatModel(IAiChatService aiChatService, UserManager<ApplicationUser> userManager)
        {
            _aiChatService = aiChatService;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(Guid? conversationId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Auth/Login");

            Conversations = await _aiChatService.GetConversationsAsync(user.Id);

            if (conversationId.HasValue)
            {
                CurrentConversationId = conversationId;
                Messages = await _aiChatService.GetHistoryAsync(user.Id, conversationId.Value);
            }

            return Page();
        }
    }
}
