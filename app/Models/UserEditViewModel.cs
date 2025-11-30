using System.ComponentModel.DataAnnotations;
using KutuphaneOtomasyonu.Attributes;

namespace KutuphaneOtomasyonu.Models
{
    /// <summary>
    /// Kullanıcı düzenleme için view model.
    /// </summary>
    public class UserEditViewModel
    {
        /// <summary>
        /// Kullanıcı ID'si.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Kullanıcı adı.
        /// </summary>
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı en fazla 50 karakter olabilir.")]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// E-posta adresi.
        /// </summary>
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(200, ErrorMessage = "E-posta en fazla 200 karakter olabilir.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Ad soyad.
        /// </summary>
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [StringLength(150, ErrorMessage = "Ad Soyad en fazla 150 karakter olabilir.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Yeni şifre (boş bırakılırsa değiştirilmez).
        /// </summary>
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8, en fazla 100 karakter olabilir.")]
        [StrongPassword(ErrorMessage = "Şifre en az 8 karakter olmalı ve büyük harf, küçük harf ve rakam içermelidir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Değiştirmek için doldurun)")]
        public string? Password { get; set; }

        /// <summary>
        /// Kullanıcı rolü.
        /// </summary>
        [Required(ErrorMessage = "Rol seçimi zorunludur.")]
        [Display(Name = "Rol")]
        public UserRole Role { get; set; } = UserRole.User;

        /// <summary>
        /// Kullanıcı durumu.
        /// </summary>
        [Required(ErrorMessage = "Durum seçimi zorunludur.")]
        [Display(Name = "Durum")]
        public UserStatus Status { get; set; } = UserStatus.Active;
    }
}
