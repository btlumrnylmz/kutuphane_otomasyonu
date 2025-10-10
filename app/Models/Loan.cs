using System;
using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyonu.Models
{
    /// <summary>
    /// Ödünç varlık modeli. Üyenin bir kopyayı ödünç almasını temsil eder.
    /// </summary>
    public class Loan
    {
        /// <summary>
        /// Birincil anahtar. Ödünç kaydının benzersiz kimliği.
        /// </summary>
        [Key]
        public int LoanId { get; set; }

        /// <summary>
        /// İlgili kopyanın kimliği (yabancı anahtar).
        /// </summary>
        [Required]
        public int CopyId { get; set; }

        /// <summary>
        /// İlgili üyenin kimliği (yabancı anahtar).
        /// </summary>
        [Required]
        public int MemberId { get; set; }

        /// <summary>
        /// Ödünç alınan tarih.
        /// </summary>
        public DateTime LoanedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Son iade tarihi (vade).
        /// </summary>
        public DateTime DueAt { get; set; }

        /// <summary>
        /// Gerçek iade tarihi (iade edilmemişse boş/NULL).
        /// </summary>
        public DateTime? ReturnedAt { get; set; }

        /// <summary>
        /// İlişkisel gezinim: Kopya.
        /// </summary>
        public Copy? Copy { get; set; }

        /// <summary>
        /// İlişkisel gezinim: Üye.
        /// </summary>
        public Member? Member { get; set; }
    }
}


