using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.AiChat;
using OnlineLearningPlatform.Services.DTOs.Chat;
using OnlineLearningPlatform.Services.Interface;
using OnlineLearningPlatform.Services.Settings;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OnlineLearningPlatform.Services.Implement
{
    public class AiChatService : IAiChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly GeminiSettings _geminiSettings;
        private readonly IDataQueryExecutor _queryExecutor;
        private readonly ILogger<AiChatService> _logger;

        // Schema đầy đủ (bạn cần điền nội dung thực tế)
        private const string DB_SCHEMA = @"..."; // Thay bằng schema thật

        public AiChatService(
            ApplicationDbContext context,
            IOptions<GeminiSettings> geminiOptions,
            IDataQueryExecutor queryExecutor,
            ILogger<AiChatService> logger)
        {
            _context = context;
            _geminiSettings = geminiOptions.Value;
            _queryExecutor = queryExecutor;
            _logger = logger;
        }

        public async Task<AiChatResponseDto> ChatAsync(string userId, string userMessage, Guid? conversationId = null)
        {
            // 1. Lấy hoặc tạo conversation
            AiConversation conversation;
            if (conversationId.HasValue)
            {
                conversation = await _context.AiConversations
                    .FirstOrDefaultAsync(c => c.ConversationId == conversationId.Value && c.UserId == userId)
                    ?? throw new InvalidOperationException("Conversation not found.");
            }
            else
            {
                conversation = new AiConversation
                {
                    ConversationId = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AiConversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            // 2. Lưu tin nhắn user
            var userMsg = new AiMessage
            {
                MessageId = Guid.NewGuid(),
                ConversationId = conversation.ConversationId,
                Role = AiMessageRole.User,
                Content = userMessage,
                CreatedAt = DateTime.UtcNow
            };
            _context.AiMessages.Add(userMsg);
            await _context.SaveChangesAsync();

            // 3. Lấy lịch sử (tối đa 10 tin nhắn gần nhất)
            var history = await _context.AiMessages
                .Where(m => m.ConversationId == conversation.ConversationId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            // 4. Gọi AI với logic mới
            string assistantContent;
            QueryIntent? queryIntent = null;
            bool usedData = false;

            try
            {
                (assistantContent, queryIntent, usedData) = await ProcessWithAIAsync(userId, userMessage, history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý AI cho user {UserId}", userId);
                assistantContent = "Xin lỗi, tôi gặp lỗi khi xử lý yêu cầu. Vui lòng thử lại sau.";
            }

            // 5. Lưu phản hồi AI
            var assistantMsg = new AiMessage
            {
                MessageId = Guid.NewGuid(),
                ConversationId = conversation.ConversationId,
                Role = AiMessageRole.Assistant,
                Content = assistantContent,
                CreatedAt = DateTime.UtcNow
            };
            _context.AiMessages.Add(assistantMsg);
            await _context.SaveChangesAsync();

            return new AiChatResponseDto
            {
                ConversationId = conversation.ConversationId,
                UserMessage = new AiMessageDto { MessageId = userMsg.MessageId, Role = "user", Content = userMsg.Content, CreatedAt = userMsg.CreatedAt },
                AssistantMessage = new AiMessageDto { MessageId = assistantMsg.MessageId, Role = "assistant", Content = assistantContent, CreatedAt = assistantMsg.CreatedAt },
                UsedSql = usedData,
                SqlQuery = queryIntent?.Intent
            };
        }

        private async Task<(string response, QueryIntent? intent, bool usedData)> ProcessWithAIAsync(
            string userId, string userMessage, List<AiMessage> history)
        {
            var googleAI = new GoogleAI(apiKey: _geminiSettings.ApiKey);
            var model = googleAI.GenerativeModel(model: _geminiSettings.Model);

            // Bước 1: Xác định intent
            var intent = await DetermineIntentAsync(model, userId, userMessage, history);

            QueryResult? queryResult = null;
            if (intent != null)
            {
                queryResult = await _queryExecutor.ExecuteIntentAsync(intent, userId);
                if (queryResult.ErrorMessage != null)
                {
                    _logger.LogWarning("Lỗi khi thực thi intent {Intent}: {Error}", intent.Intent, queryResult.ErrorMessage);
                }
            }

            // Bước 2: Sinh câu trả lời
            string finalAnswer = await GenerateAnswerAsync(model, userMessage, history, queryResult, intent != null);
            bool usedData = queryResult != null && !queryResult.IsEmpty && queryResult.ErrorMessage == null;
            return (finalAnswer, intent, usedData);
        }

        /// <summary>
        /// Xác định ý định truy vấn từ câu hỏi của người dùng
        /// </summary>
        private async Task<QueryIntent?> DetermineIntentAsync(GenerativeModel model, string userId, string userMessage, List<AiMessage> history)
        {
            var histBuilder = new StringBuilder();
            foreach (var msg in history.TakeLast(4))
            {
                histBuilder.AppendLine($"{msg.Role}: {msg.Content}");
            }

            string prompt = $@"Bạn là trợ lý AI cho nền tảng học trực tuyến. Nhiệm vụ của bạn là phân tích câu hỏi của người dùng và xác định xem họ có cần dữ liệu từ database không. Nếu CÓ, hãy trả về một JSON thể hiện ý định truy vấn. Nếu KHÔNG, trả về ""NO_QUERY"".

Database schema (chỉ để tham khảo):
{DB_SCHEMA}

Lịch sử hội thoại gần nhất:
{histBuilder}

Câu hỏi hiện tại: ""{userMessage}""

UserId hiện tại: {userId}

Các intent được hỗ trợ:
- list_courses: lấy danh sách khóa học
- count_courses: đếm số khóa học
- list_enrollments: lấy danh sách ghi danh (có thể lọc theo user)
- (có thể mở rộng thêm)

Tham số phổ biến:
- level: Beginner, Intermediate, Advanced
- category: tên danh mục
- teacherid: id giáo viên
- minprice, maxprice: số thập phân
- my: true/false (cho list_enrollments)

Quy tắc:
- Nếu câu hỏi liên quan đến bản thân người dùng (khóa học của tôi, tiến độ của tôi,...), hãy thêm tham số ""my"": true.
- Luôn giới hạn số lượng kết quả phù hợp (mặc định 10).
- Sắp xếp theo createdAt giảm dần nếu không có yêu cầu khác.

Trả về JSON hợp lệ, ví dụ:
{{ ""intent"": ""list_courses"", ""parameters"": {{ ""level"": ""Beginner"" }}, ""limit"": 5 }}
Hoặc
{{ ""intent"": ""list_enrollments"", ""parameters"": {{ ""my"": true }}, ""limit"": 10 }}

Nếu không cần query, trả về ""NO_QUERY"".
";

            try
            {
                var response = await model.GenerateContent(prompt);
                string content = response.Text?.Trim() ?? "NO_QUERY";

                // Xóa markdown code block nếu có
                content = Regex.Replace(content, @"```(?:json)?\s*", "", RegexOptions.IgnoreCase);
                content = content.TrimEnd('`').Trim();

                if (content.Equals("NO_QUERY", StringComparison.OrdinalIgnoreCase))
                    return null;

                var intent = JsonSerializer.Deserialize<QueryIntent>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (intent != null && !string.IsNullOrEmpty(intent.Intent))
                {
                    if (!intent.Limit.HasValue)
                        intent.Limit = 10;
                    return intent;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể parse intent từ AI response");
            }
            return null;
        }

        /// <summary>
        /// Sinh câu trả lời cuối cùng dựa trên dữ liệu (nếu có)
        /// </summary>
        private async Task<string> GenerateAnswerAsync(
            GenerativeModel model,
            string userMessage,
            List<AiMessage> history,
            QueryResult? queryResult,
            bool wasQueryAttempted)
        {
            var histBuilder = new StringBuilder();
            foreach (var msg in history.TakeLast(4))
            {
                histBuilder.AppendLine($"{msg.Role}: {msg.Content}");
            }

            string prompt;

            if (queryResult != null && !queryResult.IsEmpty && queryResult.ErrorMessage == null)
            {
                string dataTable = FormatQueryResultAsText(queryResult);
                prompt = $@"Bạn là trợ lý AI thân thiện của nền tảng học trực tuyến.

Lịch sử:
{histBuilder}

Câu hỏi: ""{userMessage}""

Dữ liệu từ database (đây là thông tin chính xác duy nhất bạn được phép dùng):
{dataTable}

Yêu cầu:
- Trả lời bằng tiếng Việt, tự nhiên, ngắn gọn.
- CHỈ dùng dữ liệu trong bảng trên. KHÔNG được bịa thêm thông tin.
- Nếu là danh sách, hãy liệt kê rõ ràng.
- KHÔNG nhắc đến database, SQL hay cách lấy dữ liệu.
- Nếu có nhiều kết quả, hãy tóm tắt hoặc liệt kê một cách dễ đọc.
";
            }
            else if (wasQueryAttempted)
            {
                string reason = queryResult?.ErrorMessage != null
                    ? "gặp lỗi kỹ thuật"
                    : "không có dữ liệu phù hợp";
                prompt = $@"Bạn là trợ lý AI.

Lịch sử:
{histBuilder}

Câu hỏi: ""{userMessage}""

Tình trạng: Đã tìm kiếm trong hệ thống nhưng {reason}.

Yêu cầu:
- Thông báo thẳng thắn rằng không tìm thấy dữ liệu.
- KHÔNG bịa ra thông tin.
- Gợi ý người dùng thử lại với từ khóa khác hoặc liên hệ hỗ trợ nếu cần.
- Giọng điệu thân thiện, không quá bi quan.
";
            }
            else
            {
                prompt = $@"Bạn là trợ lý AI cho nền tảng học trực tuyến.

Lịch sử:
{histBuilder}

Câu hỏi: ""{userMessage}""

Hướng dẫn:
- Trả lời ngắn gọn, hữu ích bằng tiếng Việt.
- Nếu cần thông tin cụ thể từ database, hãy hướng dẫn người dùng hỏi rõ hơn (ví dụ: ""Bạn muốn xem khóa học nào? "").
- Không bịa đặt thông tin về khóa học, giá cả, hay người dùng.
";
            }

            try
            {
                var response = await model.GenerateContent(prompt);
                return response.Text?.Trim() ?? "Xin lỗi, tôi không thể trả lời lúc này.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi sinh câu trả lời từ AI");
                return "Xin lỗi, tôi gặp lỗi khi xử lý. Vui lòng thử lại sau.";
            }
        }

        /// <summary>
        /// Chuyển QueryResult thành text dễ đọc cho AI
        /// </summary>
        private string FormatQueryResultAsText(QueryResult result)
        {
            if (result == null || result.IsEmpty)
                return "(Không có dữ liệu)";

            var sb = new StringBuilder();
            sb.AppendLine("Kết quả truy vấn:");
            sb.AppendLine(string.Join(" | ", result.Columns));
            foreach (var row in result.Rows)
            {
                sb.AppendLine(string.Join(" | ", row));
            }
            return sb.ToString();
        }

        // Các method public khác giữ nguyên
        public async Task<List<AiMessageDto>> GetHistoryAsync(string userId, Guid conversationId)
        {
            var conv = await _context.AiConversations
                .FirstOrDefaultAsync(c => c.ConversationId == conversationId && c.UserId == userId);
            if (conv == null) return new List<AiMessageDto>();

            return await _context.AiMessages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new AiMessageDto
                {
                    MessageId = m.MessageId,
                    Role = m.Role == AiMessageRole.User ? "user" : "assistant",
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                }).ToListAsync();
        }

        public async Task<List<AiConversationSummaryDto>> GetConversationsAsync(string userId)
        {
            var convs = await _context.AiConversations
                .Where(c => c.UserId == userId)
                .Include(c => c.AiMessages)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return convs.Select(c =>
            {
                var last = c.AiMessages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
                return new AiConversationSummaryDto
                {
                    ConversationId = c.ConversationId,
                    CreatedAt = c.CreatedAt,
                    LastMessage = last?.Content?.Length > 80 ? last.Content[..80] + "..." : last?.Content,
                    LastMessageAt = last?.CreatedAt
                };
            }).ToList();
        }

        public async Task<Guid> CreateConversationAsync(string userId)
        {
            var conv = new AiConversation
            {
                ConversationId = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.AiConversations.Add(conv);
            await _context.SaveChangesAsync();
            return conv.ConversationId;
        }
    }
}