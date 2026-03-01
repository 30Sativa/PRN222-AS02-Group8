using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Giao dịch vào/ra ví nội bộ (refund, purchase, top up...).
    /// </summary>
    public class WalletTransaction
    {
        [Key]
        public int WalletTransactionId { get; set; }

        [Required]
        public int WalletId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Loại giao dịch (Hoàn tiền, Mua khóa, Nạp, Rút...).
        /// </summary>
        [Required]
        public WalletTransactionType Type { get; set; }

        [Required]
        [MaxLength(300)]
        public string Description { get; set; } = default!;

        public int? RefundId { get; set; }

        public int? OrderId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(WalletId))]
        public Wallet Wallet { get; set; } = default!;

        [ForeignKey(nameof(RefundId))]
        public Refund? Refund { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order? Order { get; set; }
    }

    /// <summary>
    /// Loại giao dịch ví nội bộ.
    /// </summary>
    public enum WalletTransactionType
    {
        Refund,
        Purchase,
        TopUp,
        Withdrawal
    }
}

