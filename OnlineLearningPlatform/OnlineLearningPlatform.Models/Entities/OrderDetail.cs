using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Chi tiết từng khóa học trong đơn hàng.
    /// Lưu giá tại thời điểm mua để không bị ảnh hưởng khi đổi giá sau này.
    /// </summary>
    public class OrderDetail
    {
        [Key]
        public int OrderDetailId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public Guid CourseId { get; set; }

        /// <summary>
        /// Giá khóa học tại thời điểm mua (snapshot).
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Số tiền được giảm (nếu có coupon).
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountApplied { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = default!;

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = default!;

        /// <summary>
        /// Bản ghi ghi danh tương ứng, tạo sau khi thanh toán thành công.
        /// </summary>
        public Enrollment? Enrollment { get; set; }
    }
}

