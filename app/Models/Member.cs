using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyonu.Models
{
    /// <summary>
    /// Üye varlık modeli. Kütüphane üyelerinin bilgisini tutar.
    /// </summary>
    public class Member
    {
        /// <summary>
        /// Birincil anahtar. Üyenin benzersiz kimliği.
        /// </summary>
        [Key]
        public int MemberId { get; set; }

        /// <summary>
        /// Üyenin adı soyadı.
        /// </summary>
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [StringLength(150, ErrorMessage = "Ad Soyad en fazla 150 karakter olabilir.")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Üyenin e-posta adresi. Benzersiz olmalıdır.
        /// </summary>
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(200, ErrorMessage = "E-posta en fazla 200 karakter olabilir.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Üyenin telefon numarası.
        /// </summary>
        [StringLength(30, ErrorMessage = "Telefon en fazla 30 karakter olabilir.")]
        public string? Phone { get; set; }

        /// <summary>
        /// Üyelik başlangıç tarihi.
        /// </summary>
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Üye durumu: active, passive.
        /// </summary>
        [Required]
        public MemberStatus Status { get; set; } = MemberStatus.Active;

        /// <summary>
        /// İlişkisel gezinim: Üyenin ödünç geçmişi listesi.
        /// </summary>
        public ICollection<Loan>? Loans { get; set; }
    }

    /// <summary>
    /// Üye durumlarını temsil eden ENUM.
    /// </summary>
    public enum MemberStatus
    {
        Active,
        Passive
    }
}


