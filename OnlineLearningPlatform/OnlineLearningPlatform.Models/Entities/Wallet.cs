using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Ví nội bộ của từng user, dùng để nhận/refund và thanh toán.
    /// </summary>
    public class Wallet
    {
        [Key]
        public int WalletId { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;

        /// <summary>
        /// Lần gần nhất số dư được cập nhật.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = default!;

        public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    }
}

