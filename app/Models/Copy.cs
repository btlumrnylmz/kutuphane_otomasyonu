using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyonu.Models
{
    /// <summary>
    /// Kitap kopyası varlık modeli. Bir kitabın fiziksel kopyalarını temsil eder.
    /// </summary>
    public class Copy
    {
        /// <summary>
        /// Birincil anahtar. Kopyanın benzersiz kimliği.
        /// </summary>
        [Key]
        public int CopyId { get; set; }

        /// <summary>
        /// Kopyanın ait olduğu kitabın ID'si.
        /// </summary>
        [Required]
        public int BookId { get; set; }

        /// <summary>
        /// Kopya numarası (aynı kitabın farklı kopyalarını ayırt etmek için).
        /// </summary>
        [Required]
        public int CopyNumber { get; set; }

        /// <summary>
        /// Kopyanın durumu: Available, Loaned, Reserved, Maintenance.
        /// </summary>
        [Required]
        public CopyStatus Status { get; set; } = CopyStatus.Available;

        /// <summary>
        /// Kopyanın raf konumu.
        /// </summary>
        [StringLength(50, ErrorMessage = "Raf konumu en fazla 50 karakter olabilir.")]
        public string? ShelfLocation { get; set; }

        /// <summary>
        /// Kopyanın oluşturulma tarihi.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Kopyanın eklendiği tarih.
        /// </summary>
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// İlişkisel gezinim: Kopyanın ait olduğu kitap.
        /// </summary>
        public Book? Book { get; set; }

        /// <summary>
        /// İlişkisel gezinim: Kopyanın ödünç geçmişi listesi.
        /// </summary>
        public ICollection<Loan>? Loans { get; set; }

        /// <summary>
        /// İlişkisel gezinim: Kopyanın rezervasyonları listesi.
        /// </summary>
        public ICollection<Reservation>? Reservations { get; set; }
    }

    /// <summary>
    /// Kopya durumlarını temsil eden ENUM.
    /// </summary>
    public enum CopyStatus
    {
        Available,      // Müsait
        Loaned,         // Ödünç verilmiş
        Reserved,       // Rezerve edilmiş
        Maintenance,    // Bakımda
        Damaged         // Hasarlı
    }
}
