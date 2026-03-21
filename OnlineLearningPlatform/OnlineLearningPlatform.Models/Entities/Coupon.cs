using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineLearningPlatform.Models.Entities.Identity;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Mã khuyến mãi (coupon) do Admin hoặc Teacher tạo.
    /// </summary>
    public class Coupon
    {
        [Key]
        public int CouponId { get; set; }

        /// <summary>
        /// Mã coupon duy nhất, do người tạo nhập (VD: SUMMER2026).
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = default!;

        /// <summary>
        /// Mô tả ngắn gọn (VD: Giảm 20% cho khóa học Python).
        /// </summary>
        [MaxLength(255)]
        public string? Description { get; set; }

        /// <summary>
        /// Loại giảm giá: Percentage (%) hoặc FixedAmount (VNĐ).
        /// </summary>
        public CouponDiscountType DiscountType { get; set; } = CouponDiscountType.Percentage;

        /// <summary>
        /// Giá trị giảm: nếu Percentage thì là số % (VD: 20 = giảm 20%),
        /// nếu FixedAmount thì là số tiền VNĐ.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// Số tiền giảm tối đa (áp dụng khi DiscountType = Percentage).
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxDiscountAmount { get; set; }

        /// <summary>
        /// Số lượng mã có thể sử dụng. Null = không giới hạn.
        /// </summary>
        public int? UsageLimit { get; set; }

        /// <summary>
        /// Số lần đã được sử dụng.
        /// </summary>
        public int UsedCount { get; set; } = 0;

        /// <summary>
        /// Số lần mỗi user có thể dùng. Null = 1 lần/user.
        /// </summary>
        public int? MaxUsagePerUser { get; set; } = 1;

        /// <summary>
        /// Ngày bắt đầu có hiệu lực.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Ngày hết hạn.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Giá trị đơn hàng tối thiểu để áp dụng coupon.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinOrderAmount { get; set; }

        /// <summary>
        /// Nếu true, coupon chỉ áp dụng cho khóa học trong CourseId cụ thể.
        /// </summary>
        public bool IsCourseSpecific { get; set; } = false;

        /// <summary>
        /// Áp dụng cho khóa học cụ thể (nếu IsCourseSpecific = true).
        /// </summary>
        public Guid? CourseId { get; set; }

        /// <summary>
        /// Ai được phép dùng: AllUsers, FirstTimeUsers, SpecificUsers.
        /// </summary>
        public CouponTarget Audience { get; set; } = CouponTarget.AllUsers;

        /// <summary>
        /// Teacher tạo coupon (null nếu do Admin tạo).
        /// </summary>
        public string? CreatedByTeacherId { get; set; }

        /// <summary>
        /// Admin tạo coupon (null nếu do Teacher tạo).
        /// </summary>
        public string? CreatedByAdminId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(CourseId))]
        public Course? Course { get; set; }

        [ForeignKey(nameof(CreatedByTeacherId))]
        public ApplicationUser? CreatedByTeacher { get; set; }

        [ForeignKey(nameof(CreatedByAdminId))]
        public ApplicationUser? CreatedByAdmin { get; set; }

        public ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
    }

    public enum CouponDiscountType
    {
        Percentage,
        FixedAmount
    }

    public enum CouponTarget
    {
        AllUsers,
        FirstTimeUsers,
        SpecificUsers
    }
}
