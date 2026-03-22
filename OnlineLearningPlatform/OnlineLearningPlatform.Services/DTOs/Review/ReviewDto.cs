using System;

namespace OnlineLearningPlatform.Services.DTOs.Review
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public Guid CourseId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Phân bố số lượng đánh giá theo từng mức sao (1–5).
    /// </summary>
    public class RatingBreakdownDto
    {
        /// <summary>Số đánh giá 5 sao</summary>
        public int Star5 { get; set; }
        /// <summary>Số đánh giá 4 sao</summary>
        public int Star4 { get; set; }
        /// <summary>Số đánh giá 3 sao</summary>
        public int Star3 { get; set; }
        /// <summary>Số đánh giá 2 sao</summary>
        public int Star2 { get; set; }
        /// <summary>Số đánh giá 1 sao</summary>
        public int Star1 { get; set; }
        /// <summary>Tổng số đánh giá</summary>
        public int Total { get; set; }
        /// <summary>Điểm trung bình</summary>
        public double Average { get; set; }

        /// <summary>Tỉ lệ % cho từng mức sao (0–100)</summary>
        public double Pct5 => Total == 0 ? 0 : (double)Star5 / Total * 100;
        public double Pct4 => Total == 0 ? 0 : (double)Star4 / Total * 100;
        public double Pct3 => Total == 0 ? 0 : (double)Star3 / Total * 100;
        public double Pct2 => Total == 0 ? 0 : (double)Star2 / Total * 100;
        public double Pct1 => Total == 0 ? 0 : (double)Star1 / Total * 100;
    }
}
