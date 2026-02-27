namespace OnlineLearningPlatform.Services.DTOs.User.Request
{
    // DTO search đa tiêu chí (từ BE, query trên DB)
    public class SearchUserRequest
    {
        // Tìm theo tên hoặc email (keyword chung)
        public string? Keyword { get; set; }

        // Lọc theo role cụ thể (Admin, Teacher, Student)
        public string? Role { get; set; }

        // Số trang hiện tại (mặc định 1)
        public int PageNumber { get; set; } = 1;

        // Số record mỗi trang (mặc định 10)
        public int PageSize { get; set; } = 10;
    }
}
