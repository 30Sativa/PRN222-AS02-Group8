using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Đơn hàng thanh toán (thay thế bảng Payment cũ).
    /// Một Order có thể chứa nhiều Course (OrderDetail).
    /// </summary>
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        /// <summary>
        /// Tổng số tiền của đơn hàng (tổng các OrderDetail.Price).
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        /// <summary>
        /// Phương thức thanh toán: VNPAY, MOMO, PAYPAL, WALLET...
        /// </summary>
        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Mã giao dịch từ cổng thanh toán.
        /// </summary>
        [MaxLength(100)]
        public string? TransactionId { get; set; }

        /// <summary>
        /// JSON raw trả về từ cổng thanh toán (lưu để debug/audit).
        /// </summary>
        public string? PaymentGatewayResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = default!;

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
    }

    /// <summary>
    /// Trạng thái đơn hàng.
    /// </summary>
    public enum OrderStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded,
        PartialRefunded
    }
}

