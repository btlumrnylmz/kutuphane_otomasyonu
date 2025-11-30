using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyonu.Models
{
    /// <summary>
    /// Kitap varlık modeli. Kütüphane koleksiyonundaki kitapları temsil eder.
    /// </summary>
    public class Book
    {
        /// <summary>
        /// Birincil anahtar. Kitabın benzersiz kimliği.
        /// </summary>
        [Key]
        public int BookId { get; set; }

        /// <summary>
        /// Kitabın ISBN numarası. Benzersiz olmalıdır.
        /// </summary>
        [Required(ErrorMessage = "ISBN zorunludur.")]
        [StringLength(20, ErrorMessage = "ISBN en fazla 20 karakter olabilir.")]
        public string Isbn { get; set; } = string.Empty;

        /// <summary>
        /// Kitabın başlığı.
        /// </summary>
        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Kitabın yazarı.
        /// </summary>
        [Required(ErrorMessage = "Yazar zorunludur.")]
        [StringLength(150, ErrorMessage = "Yazar en fazla 150 karakter olabilir.")]
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// Kitabın kategorisi.
        /// </summary>
        [Required(ErrorMessage = "Kategori zorunludur.")]
        [StringLength(100, ErrorMessage = "Kategori en fazla 100 karakter olabilir.")]
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Kitabın yayın tarihi.
        /// </summary>
        public DateTime? PublishedDate { get; set; }

        /// <summary>
        /// Kitabın yayın yılı.
        /// </summary>
        public int? PublishYear { get; set; }

        /// <summary>
        /// Kitabın sayfa sayısı.
        /// </summary>
        public int? PageCount { get; set; }

        /// <summary>
        /// Kitabın açıklaması.
        /// </summary>
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string? Description { get; set; }

        /// <summary>
        /// Kitap kapağı görsel URL'i.
        /// </summary>
        [StringLength(500, ErrorMessage = "Kapak görsel URL'i en fazla 500 karakter olabilir.")]
        public string? CoverImageUrl { get; set; }

        /// <summary>
        /// Kitabın oluşturulma tarihi.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// İlişkisel gezinim: Kitabın kopyaları listesi.
        /// </summary>
        public ICollection<Copy>? Copies { get; set; }
    }
}
