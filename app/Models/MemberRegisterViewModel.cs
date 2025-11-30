using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyonu.Models
{
    /// <summary>
    /// Üye kayıt sayfası için view model.
    /// </summary>
    public class MemberRegisterViewModel
    {
        /// <summary>
        /// Üyenin adı soyadı.
        /// </summary>
        [Required(ErrorMessage = "Ad Soyad zorunludur.")]
        [StringLength(150, ErrorMessage = "Ad Soyad en fazla 150 karakter olabilir.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Üyenin e-posta adresi.
        /// </summary>
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(200, ErrorMessage = "E-posta en fazla 200 karakter olabilir.")]
        [Display(Name = "E-posta Adresi")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Üyenin telefon numarası.
        /// </summary>
        [StringLength(30, ErrorMessage = "Telefon en fazla 30 karakter olabilir.")]
        [Display(Name = "Telefon Numarası")]
        public string? Phone { get; set; }

        /// <summary>
        /// Şifre.
        /// </summary>
        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6, en fazla 100 karakter olabilir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Şifre tekrarı.
        /// </summary>
        [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrarı")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

