using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyonu.Models
{
    /// <summary>
    /// Giriş sayfası için view model.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Kullanıcı adı veya e-posta adresi.
        /// </summary>
        [Required(ErrorMessage = "Kullanıcı adı veya e-posta zorunludur.")]
        [Display(Name = "Kullanıcı Adı veya E-posta")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        /// <summary>
        /// Şifre.
        /// </summary>
        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Beni hatırla seçeneği.
        /// </summary>
        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}
