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

        private const string DB_SCHEMA = @"
DATABASE SCHEMA - OnlineLearningPlatform:

**Bảng Users** (ApplicationUser)
- Id (string, GUID): Khóa chính
- FullName (string): Họ tên đầy đủ
- Email (string): Email
- UserName (string): Tên đăng nhập
- Role (string): Vai trò (Admin, Teacher, Student)
- CreatedAt (DateTime): Ngày tạo tài khoản

**Bảng Categories**
- CategoryId (int): Khóa chính
- CategoryName (string): Tên danh mục (Frontend, Backend, Mobile, AI, Data Science...)

**Bảng Courses**
- CourseId (GUID): Khóa chính
- CourseCode (string): Mã khóa học (duy nhất)
- Title (string): Tiêu đề
- Description (string?): Mô tả
- TeacherId (string): FK → Users.Id
- CategoryId (int?): FK → Categories.CategoryId
- Price (decimal): Giá gốc (0 = miễn phí)
- DiscountPrice (decimal?): Giá sau giảm
- Level (enum): Beginner, Intermediate, Advanced
- Status (enum): Pending, Published, Rejected
- IsFeatured (bool): Nổi bật
- TotalDuration (int): Tổng thời lượng (giây)
- CreatedAt (DateTime): Ngày tạo
- Teacher (Users): Navigation - thông tin giáo viên

**Bảng Sections**
- SectionId (int): Khóa chính
- CourseId (GUID): FK → Courses.CourseId
- Title (string): Tiêu đề chương
- OrderIndex (int): Thứ tự

**Bảng Lessons**
- LessonId (int): Khóa chính
- SectionId (int): FK → Sections.SectionId
- Title (string): Tiêu đề bài học
- VideoUrl (string?): URL video
- Content (string?): Nội dung
- Duration (int): Thời lượng (giây)
- OrderIndex (int): Thứ tự

**Bảng Enrollments** (Ghi danh)
- EnrollmentId (GUID): Khóa chính
- UserId (string): FK → Users.Id
- CourseId (GUID): FK → Courses.CourseId
- EnrolledAt (DateTime): Ngày ghi danh
- LastAccessedAt (DateTime?): Lần truy cập cuối
- IsActive (bool): Còn hiệu lực

**Bảng LessonProgresses**
- LessonProgressId (GUID): Khóa chính
- UserId (string): FK → Users.Id
- LessonId (int): FK → Lessons.LessonId
- IsCompleted (bool): Hoàn thành
- WatchedSeconds (int): Số giây đã xem
- LastWatchedAt (DateTime?): Lần xem cuối

**Bảng Reviews** (Đánh giá)
- ReviewId (int): Khóa chính
- UserId (string): FK → Users.Id
- CourseId (GUID): FK → Courses.CourseId
- Rating (int): 1-5 sao
- Comment (string?): Bình luận
- CreatedAt (DateTime): Ngày tạo

**Bảng Certificates**
- CertificateId (GUID): Khóa chính
- UserId (string): FK → Users.Id
- CourseId (GUID): FK → Courses.CourseId
- IssuedAt (DateTime): Ngày cấp
- CertificateUrl (string?): URL chứng chỉ

**Bảng Orders**
- OrderId (int): Khóa chính
- UserId (string): FK → Users.Id
- TotalAmount (decimal): Tổng tiền
- WalletUsed (decimal): Tiền từ ví đã dùng
- Status (enum): Pending, Completed, Failed, Refunded
- PaymentMethod (string?): VNPAY, WALLET...
- TransactionId (string?): Mã giao dịch
- CreatedAt (DateTime): Ngày tạo
- CompletedAt (DateTime?): Ngày hoàn thành

**Bảng Wallet** (Ví)
- WalletId (int): Khóa chính
- UserId (string): FK → Users.Id (duy nhất)
- Balance (decimal): Số dư VNĐ

**Bảng WalletTransactions**
- WalletTransactionId (int): Khóa chính
- WalletId (int): FK → Wallet.WalletId
- Amount (decimal): Số tiền
- Type (enum): Refund, Purchase, TopUp, Withdrawal
- Description (string): Mô tả
- CreatedAt (DateTime): Ngày tạo

**Bảng Quizzes**
- QuizId (int): Khóa chính
- LessonId (int): FK → Lessons.LessonId
- Title (string): Tiêu đề
- TimeLimit (int?): Thời gian (phút)
- PassingScore (int): Điểm đạt (%)

**Bảng QuizAttempts**
- AttemptId (GUID): Khóa chính
- QuizId (int): FK → Quizzes.QuizId
- UserId (string): FK → Users.Id
- Score (int): Điểm số (%)
- CompletedAt (DateTime?): Ngày hoàn thành
- IsPassed (bool): Đạt/Không đạt

**Bảng Notifications**
- NotificationId (int): Khóa chính
- UserId (string): FK → Users.Id
- Title (string): Tiêu đề
- Message (string): Nội dung
- IsRead (bool): Đã đọc
- CreatedAt (DateTime): Ngày tạo

**Enums quan trọng:**
- CourseLevel: Beginner, Intermediate, Advanced
- CourseStatus: Pending, Published, Rejected
- OrderStatus: Pending, Completed, Failed, Refunded, PartialRefunded
- WalletTransactionType: Refund, Purchase, TopUp, Withdrawal
";

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

            var history = await _context.AiMessages
                .Where(m => m.ConversationId == conversation.ConversationId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(10)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

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

            string finalAnswer = await GenerateAnswerAsync(model, userMessage, history, queryResult, intent != null);
            bool usedData = queryResult != null && !queryResult.IsEmpty && queryResult.ErrorMessage == null;
            return (finalAnswer, intent, usedData);
        }

        private async Task<QueryIntent?> DetermineIntentAsync(GenerativeModel model, string userId, string userMessage, List<AiMessage> history)
        {
            var histBuilder = new StringBuilder();
            foreach (var msg in history.TakeLast(4))
            {
                histBuilder.AppendLine($"{msg.Role}: {msg.Content}");
            }

            string prompt = $@"Bạn là trợ lý AI cho nền tảng học trực tuyến. Nhiệm vụ của bạn là phân tích câu hỏi của người dùng và xác định xem họ có cần dữ liệu từ database không.

Database schema:
{DB_SCHEMA}

Lịch sử hội thoại gần nhất:
{histBuilder}

Câu hỏi hiện tại: ""{userMessage}""

UserId hiện tại: {userId}

Các intent được hỗ trợ (LUÔN LUÔN trả về JSON intent khi câu hỏi liên quan đến dữ liệu):
- list_courses: danh sách khóa học (lọc theo: level, category, teacherid, minprice, maxprice, status=Published)
- count_courses: đếm số khóa học (lọc: level, category, status)
- list_enrollments: danh sách ghi danh (lọc: my=true cho user hiện tại, courseid, userid)
- count_enrollments: đếm số khóa đã enroll
- get_course_detail: chi tiết 1 khóa học (thamsố: courseid hoặc tìm theo title chứa keyword)
- list_reviews: đánh giá khóa học (thamsố: courseid, limit)
- get_user_progress: tiến độ học tập (thamsố: my=true, courseid)
- list_categories: danh sách danh mục
- get_wallet_balance: số dư ví (thamsố: my=true)
- get_wallet_transactions: lịch sử giao dịch ví (thamsố: my=true, limit)
- get_user_stats: thống kê cá nhân (khóa đã enroll, đã hoàn thành, đang học)
- get_popular_courses: khóa học phổ biến (theo số lượng enroll)
- get_top_rated_courses: khóa được đánh giá cao

Quy tắc:
- Nếu câu hỏi liên quan đến dữ liệu cá nhân (khóa của tôi, ví của tôi, tiến độ của tôi...), thêm ""my"": true
- LUÔN giới hạn limit phù hợp (mặc định 10, tối đa 20)
- Sắp xếp theo createdAt giảm dần nếu không có yêu cầu khác
- Nếu user hỏi về 1 khóa cụ thể, dùng get_course_detail
- Nếu user hỏi về thống kê cá nhân, dùng get_user_stats
- Với get_course_detail, có thể parse courseid từ URL hoặc tìm theo title

Trả về JSON hợp lệ, ví dụ:
{{ ""intent"": ""list_courses"", ""parameters"": {{ ""level"": ""Beginner"" }}, ""limit"": 5 }}
{{ ""intent"": ""list_enrollments"", ""parameters"": {{ ""my"": true }}, ""limit"": 10 }}
{{ ""intent"": ""get_course_detail"", ""parameters"": {{ ""courseid"": ""abc-123"" }} }}
{{ ""intent"": ""get_user_stats"", ""parameters"": {{ ""my"": true }} }}

Nếu câu hỏi KHÔNG liên quan đến database (chào hỏi, hỏi thông tin chung, câu hỏi không rõ ràng...), trả về ""NO_QUERY"".";
            try
            {
                var response = await model.GenerateContent(prompt);
                string content = response.Text?.Trim() ?? "NO_QUERY";

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
- Trả lời bằng tiếng Việt, tự nhiên, ngắn gọn (dưới 300 từ).
- CHỈ dùng dữ liệu trong bảng trên. KHÔNG được bịa thêm thông tin.
- Nếu là danh sách, hãy liệt kê rõ ràng bằng gạch đầu dòng.
- KHÔNG nhắc đến database, SQL hay cách lấy dữ liệu.
- Nếu có nhiều kết quả, hãy tóm tắt hoặc liệt kê 1 cách dễ đọc.
- Nếu hỏi về giá tiền, format rõ ràng: VD ""500.000 VNĐ""
- Thân thiện, hữu ích, đưa ra gợi ý nếu phù hợp.";
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
- Trả lời ngắn gọn (dưới 100 từ).";
            }
            else
            {
                prompt = $@"Bạn là trợ lý AI cho nền tảng học trực tuyến.

Lịch sử:
{histBuilder}

Câu hỏi: ""{userMessage}""

Hướng dẫn:
- Trả lời ngắn gọn, hữu ích bằng tiếng Việt.
- Nếu cần thông tin cụ thể từ database, hãy hướng dẫn người dùng hỏi rõ hơn.
- Bạn có thể trả lời về: khóa học, tiến độ học tập, số dư ví, đánh giá, thống kê cá nhân...
- Không bịa đặt thông tin về khóa học, giá cả, hay người dùng.
- Nếu câu hỏi không liên quan đến nền tảng, trả lời lịch sự rằng bạn chỉ hỗ trợ về nền tảng học trực tuyến.
- Giọng điệu thân thiện, chuyên nghiệp.";
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
