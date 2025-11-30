using System.Security.Cryptography;
using System.Text;
using KutuphaneOtomasyonu.Data;
using KutuphaneOtomasyonu.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using BCrypt.Net;

namespace KutuphaneOtomasyonu.Services
{
    /// <summary>
    /// Kimlik doğrulama ve yetkilendirme işlemlerini yöneten servis.
    /// </summary>
    public class AuthService
    {
        private readonly LibraryContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string SESSION_USER_KEY = "UserSession";

        public AuthService(LibraryContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Kullanıcı girişi yapar.
        /// </summary>
        /// <param name="usernameOrEmail">Kullanıcı adı veya e-posta</param>
        /// <param name="password">Şifre</param>
        /// <returns>Giriş başarılı ise kullanıcı bilgileri, değilse null</returns>
        public async Task<User?> LoginAsync(string usernameOrEmail, string password)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
                return null;

            // Case-insensitive karşılaştırma için normalize et
            var normalizedInput = usernameOrEmail.Trim().ToLowerInvariant();

            // Tüm aktif kullanıcıları çek ve memory'de case-insensitive karşılaştır
            // Bu yaklaşım EF Core'un ToLower() sorunlarını önler
            var activeUsers = await _context.Users
                .Where(u => u.Status == UserStatus.Active)
                .ToListAsync();

            var user = activeUsers.FirstOrDefault(u =>
                u.Username.Trim().ToLowerInvariant() == normalizedInput ||
                u.Email.Trim().ToLowerInvariant() == normalizedInput);

            if (user == null)
                return null;

            // Şifre doğrulaması
            if (!VerifyPassword(password, user.PasswordHash))
                return null;

            // Son giriş tarihini güncelle
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Şifreyi hash'ler. BCrypt kullanır.
        /// </summary>
        /// <param name="password">Ham şifre</param>
        /// <returns>Hash'lenmiş şifre (BCrypt formatında)</returns>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        /// <summary>
        /// Şifreyi doğrular. Hem BCrypt hem eski SHA256 hash'lerini destekler (geriye dönük uyumluluk).
        /// </summary>
        /// <param name="password">Ham şifre</param>
        /// <param name="hashedPassword">Hash'lenmiş şifre</param>
        /// <returns>Şifre doğru ise true</returns>
        private bool VerifyPassword(string password, string hashedPassword)
        {
            // BCrypt hash kontrolü (BCrypt hash'ler $2a$, $2b$, $2y$ ile başlar)
            if (hashedPassword.StartsWith("$2") && BCrypt.Net.BCrypt.Verify(password, hashedPassword))
            {
                return true;
            }

            // Eski SHA256 hash'leri için geriye dönük uyumluluk
            try
            {
                using var sha256 = SHA256.Create();
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "LibrarySalt2024"));
                var oldHash = Convert.ToBase64String(hashedBytes);
                return oldHash == hashedPassword;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Mevcut kullanıcıyı session'dan alır. Session'da user bilgileri varsa oradan, yoksa veritabanından alır.
        /// </summary>
        /// <returns>Giriş yapmış kullanıcı veya null</returns>
        public User? GetCurrentUser()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return null;

            // Session'dan user bilgilerini al (optimizasyon)
            var userJson = httpContext.Session.GetString(SESSION_USER_KEY);
            if (!string.IsNullOrEmpty(userJson))
            {
                try
                {
                    var userSession = JsonSerializer.Deserialize<UserSession>(userJson);
                    if (userSession != null)
                    {
                        // Session'dan user bilgilerini döndür (veritabanı sorgusu yok)
                        return new User
                        {
                            UserId = userSession.UserId,
                            Username = userSession.Username,
                            Email = userSession.Email,
                            FullName = userSession.FullName,
                            Role = userSession.Role,
                            Status = userSession.Status
                        };
                    }
                }
                catch
                {
                    // JSON deserialize hatası - session'ı temizle ve veritabanından al
                    httpContext.Session.Remove(SESSION_USER_KEY);
                }
            }

            // Eski yöntem: session'da userId var mı kontrol et (geriye dönük uyumluluk)
            var userId = httpContext.Session.GetInt32("UserId");
            if (userId == null) return null;

            var user = _context.Users.AsNoTracking().FirstOrDefault(u => u.UserId == userId);
            
            // Bulunan kullanıcıyı session'a kaydet (bir sonraki çağrıda hızlı olsun)
            if (user != null)
            {
                SetUserSession(user);
            }

            return user;
        }

        /// <summary>
        /// Kullanıcının yönetici olup olmadığını kontrol eder.
        /// </summary>
        /// <returns>Yönetici ise true</returns>
        public bool IsAdmin()
        {
            var user = GetCurrentUser();
            return user?.Role == UserRole.Admin;
        }

        /// <summary>
        /// Kullanıcının giriş yapıp yapmadığını kontrol eder.
        /// </summary>
        /// <returns>Giriş yapmış ise true</returns>
        public bool IsLoggedIn()
        {
            return GetCurrentUser() != null;
        }

        /// <summary>
        /// Kullanıcıyı session'a kaydeder. Tüm gerekli bilgileri session'da saklar.
        /// </summary>
        /// <param name="user">Kullanıcı bilgileri</param>
        public void SetUserSession(User user)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            // Eski yöntem (geriye dönük uyumluluk)
            httpContext.Session.SetInt32("UserId", user.UserId);

            // Yeni yöntem: Tüm user bilgilerini session'da sakla
            var userSession = new UserSession
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Status = user.Status
            };

            var userJson = JsonSerializer.Serialize(userSession);
            httpContext.Session.SetString(SESSION_USER_KEY, userJson);
        }

        /// <summary>
        /// Session'da saklanan user bilgileri için model.
        /// </summary>
        private class UserSession
        {
            public int UserId { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public UserRole Role { get; set; }
            public UserStatus Status { get; set; }
        }

        /// <summary>
        /// Kullanıcıyı session'dan çıkarır.
        /// </summary>
        public void Logout()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            httpContext.Session.Remove("UserId");
            httpContext.Session.Remove(SESSION_USER_KEY);
        }

        /// <summary>
        /// Varsayılan yönetici hesabını oluşturur.
        /// </summary>
        public async Task CreateDefaultAdminAsync()
        {
            var adminExists = await _context.Users.AnyAsync(u => u.Role == UserRole.Admin);
            if (adminExists) return;

            var admin = new User
            {
                Username = "admin",
                Email = "admin@kutuphane.com",
                FullName = "Sistem Yöneticisi",
                PasswordHash = HashPassword("admin123"),
                Role = UserRole.Admin,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(admin);
            await _context.SaveChangesAsync();
        }
    }
}
