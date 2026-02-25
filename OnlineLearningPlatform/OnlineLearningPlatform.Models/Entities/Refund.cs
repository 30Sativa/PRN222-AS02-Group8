using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Yêu cầu hoàn tiền cho một phần hoặc toàn bộ đơn hàng.
    /// </summary>
    public class Refund
    {
        [Key]
        public int RefundId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        /// <summary>
        /// Số tiền được yêu cầu hoàn.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = default!;

        public RefundStatus Status { get; set; } = RefundStatus.Pending;

        public RefundTarget RefundTarget { get; set; } = RefundTarget.Wallet;

        /// <summary>
        /// Ghi chú thêm của admin khi xử lý yêu cầu.
        /// </summary>
        [MaxLength(500)]
        public string? AdminNote { get; set; }

        /// <summary>
        /// Id admin đã xử lý yêu cầu này.
        /// </summary>
        public string? ProcessedById { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ProcessedAt { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; } = default!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = default!;

        [ForeignKey(nameof(ProcessedById))]
        public ApplicationUser? ProcessedBy { get; set; }
    }

    /// <summary>
    /// Trạng thái yêu cầu hoàn tiền.
    /// </summary>
    public enum RefundStatus
    {
        Pending,
        Approved,
        Rejected
    }

    /// <summary>
    /// Hướng hoàn tiền (về ví hay chuyển khoản).
    /// </summary>
    public enum RefundTarget
    {
        Wallet,
        BankTransfer
    }
}

