using System;
using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyonu.Models
{
    /// <summary>
    /// Sistem kullanıcıları için model. Yönetici ve normal kullanıcıları temsil eder.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Birincil anahtar. Kullanıcının benzersiz kimliği.
        /// </summary>
        [Key]
        public int UserId { get; set; }

        /// <summary>
        /// Kullanıcı adı. Benzersiz olmalıdır.
        /// </summary>
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir.")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcının e-posta adresi. Benzersiz olmalıdır.
        /// </summary>
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(200, ErrorMessage = "E-posta en fazla 200 karakter olabilir.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Şifre hash'i. Gerçek şifre saklanmaz.
        /// </summary>
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcının adı soyadı.
        /// </summary>
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [StringLength(150, ErrorMessage = "Ad Soyad en fazla 150 karakter olabilir.")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcı rolü: Admin, User
        /// </summary>
        [Required]
        public UserRole Role { get; set; } = UserRole.User;

        /// <summary>
        /// Hesap oluşturulma tarihi.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Son giriş tarihi.
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Hesap durumu: active, inactive
        /// </summary>
        [Required]
        public UserStatus Status { get; set; } = UserStatus.Active;
    }

    /// <summary>
    /// Kullanıcı rollerini temsil eden ENUM.
    /// </summary>
    public enum UserRole
    {
        Admin,
        User
    }

    /// <summary>
    /// Kullanıcı durumlarını temsil eden ENUM.
    /// </summary>
    public enum UserStatus
    {
        Active,
        Inactive
    }
}
