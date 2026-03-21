using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineLearningPlatform.Models.Entities.Identity;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Ghi nhận mỗi lần user sử dụng một coupon.
    /// </summary>
    public class CouponUsage
    {
        [Key]
        public int CouponUsageId { get; set; }

        [Required]
        public int CouponId { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        [Required]
        public int OrderId { get; set; }

        /// <summary>
        /// Số tiền được giảm trong lần sử dụng này.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        public DateTime UsedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(CouponId))]
        public Coupon Coupon { get; set; } = default!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = default!;

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = default!;
    }
}
