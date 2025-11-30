using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace KutuphaneOtomasyonu.Attributes
{
    /// <summary>
    /// Güçlü şifre validasyonu için custom attribute.
    /// </summary>
    public class StrongPasswordAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return true; // Required attribute ile kontrol edilir
            }

            var password = value.ToString()!;

            // Minimum 8 karakter
            if (password.Length < 8)
            {
                ErrorMessage = "Şifre en az 8 karakter olmalıdır.";
                return false;
            }

            // En az bir büyük harf
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                ErrorMessage = "Şifre en az bir büyük harf içermelidir.";
                return false;
            }

            // En az bir küçük harf
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                ErrorMessage = "Şifre en az bir küçük harf içermelidir.";
                return false;
            }

            // En az bir rakam
            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                ErrorMessage = "Şifre en az bir rakam içermelidir.";
                return false;
            }

            return true;
        }
    }
}

