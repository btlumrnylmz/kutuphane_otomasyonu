using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace KutuphaneOtomasyonu.Attributes
{
    /// <summary>
    /// Sadece rakamlardan oluşan değerleri reddeden validasyon attribute'u.
    /// Değer en az bir harf içermelidir.
    /// </summary>
    public class TextOnlyAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return true; // Required attribute ile kontrol edilir
            }

            var text = value.ToString()!.Trim();

            // Sadece rakamlardan oluşuyorsa geçersiz
            if (Regex.IsMatch(text, @"^\d+$"))
            {
                ErrorMessage = "Lütfen geçerli bir isim girin. Sadece rakamlardan oluşan değerler kabul edilmez.";
                return false;
            }

            // En az bir harf içermeli (Türkçe karakterler dahil)
            if (!Regex.IsMatch(text, @"[a-zA-ZçğıöşüÇĞIİÖŞÜ]"))
            {
                ErrorMessage = "Lütfen geçerli bir isim girin. En az bir harf içermelidir.";
                return false;
            }

            return true;
        }
    }
}

