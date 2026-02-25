using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.Models.Entities.Identity
{
    /// <summary>
    /// Bảng người dùng, kế thừa IdentityUser mặc định.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Họ tên đầy đủ hiển thị trong hệ thống.
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = default!;

        /// <summary>
        /// Giới thiệu ngắn về bản thân (thường dùng cho Teacher).
        /// </summary>
        public string? Bio { get; set; }

        /// <summary>
        /// Thời điểm tạo tài khoản.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
