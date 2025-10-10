using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyonu.Models
{
    /// <summary>
    /// Yeni kullanıcı oluşturma için view model.
    /// </summary>
    public class UserCreateViewModel
    {
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
        /// Şifre.
        /// </summary>
        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6, en fazla 100 karakter olabilir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

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
