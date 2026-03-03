using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineLearningPlatform.Models.Entities.Identity;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Controllers
{
    [ApiController]
    [Route("api/aichat")]
    [Authorize]
    public class AiChatController : ControllerBase
    {
        private readonly IAiChatService _aiChatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AiChatController(IAiChatService aiChatService, UserManager<ApplicationUser> userManager)
        {
            _aiChatService = aiChatService;
            _userManager = userManager;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] AiChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { success = false, error = "Message is required." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                var result = await _aiChatService.ChatAsync(
                    user.Id,
                    request.Message,
                    request.ConversationId);

                return Ok(new
                {
                    success = true,
                    conversationId = result.ConversationId,
                    assistantMessage = result.AssistantMessage.Content,
                    usedSql = result.UsedSql,
                    sqlQuery = result.SqlQuery
                });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, error = ex.Message });
            }
        }

        [HttpPost("new")]
        public async Task<IActionResult> NewConversation()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var id = await _aiChatService.CreateConversationAsync(user.Id);
            return Ok(new { conversationId = id });
        }
    }

    public class AiChatRequest
    {
        public Guid? ConversationId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
