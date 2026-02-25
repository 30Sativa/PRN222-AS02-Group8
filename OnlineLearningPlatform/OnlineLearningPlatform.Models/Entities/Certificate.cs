using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Chứng chỉ hoàn thành khóa học.
    /// </summary>
    public class Certificate
    {
        [Key]
        public Guid CertificateId { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        [Required]
        public Guid CourseId { get; set; }

        /// <summary>
        /// Mã chứng chỉ unique (vd: CERT-OOP1-20240915-00123).
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CertificateCode { get; set; } = default!;

        /// <summary>
        /// Đường dẫn file PDF chứng chỉ trên server/S3.
        /// </summary>
        [MaxLength(500)]
        public string? FileStoragePath { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = default!;

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = default!;
    }
}

