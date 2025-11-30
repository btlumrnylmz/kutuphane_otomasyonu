using System;
using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyonu.Models
{
    /// <summary>
    /// Kullanıcı favori kitapları için model.
    /// </summary>
    public class Favorite
    {
        /// <summary>
        /// Birincil anahtar. Favori kaydının benzersiz kimliği.
        /// </summary>
        [Key]
        public int FavoriteId { get; set; }

        /// <summary>
        /// Kullanıcı kimliği (yabancı anahtar).
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Kitap kimliği (yabancı anahtar).
        /// </summary>
        [Required]
        public int BookId { get; set; }

        /// <summary>
        /// Favoriye eklenme tarihi.
        /// </summary>
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// İlişkisel gezinim: Kullanıcı.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// İlişkisel gezinim: Kitap.
        /// </summary>
        public Book? Book { get; set; }
    }
}








