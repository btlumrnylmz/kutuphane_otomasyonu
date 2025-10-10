using System;
using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyonu.Models
{
    /// <summary>
    /// Rezervasyon modeli. Bir kopya için bekleme listesi kaydını temsil eder.
    /// </summary>
    public class Reservation
    {
        /// <summary>
        /// Birincil anahtar. Rezervasyonun benzersiz kimliği.
        /// </summary>
        [Key]
        public int ReservationId { get; set; }

        /// <summary>
        /// Üye kimliği (FK).
        /// </summary>
        [Required]
        public int MemberId { get; set; }

        /// <summary>
        /// Kopya kimliği (FK).
        /// </summary>
        [Required]
        public int CopyId { get; set; }

        /// <summary>
        /// Rezervasyon tarihi (UTC).
        /// </summary>
        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Bildirim gönderildi mi? (simülasyon)
        /// </summary>
        public bool Notified { get; set; } = false;

        public Member? Member { get; set; }
        public Copy? Copy { get; set; }
    }
}



