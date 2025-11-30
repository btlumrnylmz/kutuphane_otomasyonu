using System;
using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyonu.Models
{
    /// <summary>
    /// İade talepleri için model. Kullanıcıların iade taleplerini ve yönetici onaylarını tutar.
    /// </summary>
    public class ReturnRequest
    {
        /// <summary>
        /// Birincil anahtar. İade talebinin benzersiz kimliği.
        /// </summary>
        [Key]
        public int ReturnRequestId { get; set; }

        /// <summary>
        /// İlgili ödünç kaydının kimliği (yabancı anahtar).
        /// </summary>
        [Required]
        public int LoanId { get; set; }

        /// <summary>
        /// Talep oluşturulma tarihi.
        /// </summary>
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Talep durumu: Pending, Approved, Rejected
        /// </summary>
        [Required]
        public ReturnRequestStatus Status { get; set; } = ReturnRequestStatus.Pending;

        /// <summary>
        /// Onay/Red tarihi.
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Onaylayan/Reddeden yöneticinin ID'si.
        /// </summary>
        public int? ProcessedByUserId { get; set; }

        /// <summary>
        /// Red nedeni (varsa).
        /// </summary>
        [StringLength(500)]
        public string? RejectionReason { get; set; }

        /// <summary>
        /// İlişkisel gezinim: Ödünç kaydı.
        /// </summary>
        public Loan? Loan { get; set; }

        /// <summary>
        /// İlişkisel gezinim: İşlemi yapan yönetici.
        /// </summary>
        public User? ProcessedByUser { get; set; }
    }

    /// <summary>
    /// İade talebi durumlarını temsil eden ENUM.
    /// </summary>
    public enum ReturnRequestStatus
    {
        Pending,    // Beklemede
        Approved,   // Onaylandı
        Rejected    // Reddedildi
    }
}

