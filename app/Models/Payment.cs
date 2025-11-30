using System;
using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyonu.Models
{
    /// <summary>
    /// Gecikme ödemeleri için model. Üyelerin geciken ödünçler için yaptığı ödemeleri tutar.
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// Birincil anahtar. Ödemenin benzersiz kimliği.
        /// </summary>
        [Key]
        public int PaymentId { get; set; }

        /// <summary>
        /// İlgili ödünç kaydının kimliği (yabancı anahtar).
        /// </summary>
        [Required]
        public int LoanId { get; set; }

        /// <summary>
        /// Ödeme tutarı (TL).
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ödeme tutarı 0'dan büyük olmalıdır.")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Ödeme tarihi.
        /// </summary>
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ödeme açıklaması/notu.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// İlişkisel gezinim: Ödünç kaydı.
        /// </summary>
        public Loan? Loan { get; set; }
    }
}

