using System.Security.Cryptography;
using System.Text;
using KutuphaneOtomasyonu.Data;
using KutuphaneOtomasyonu.Models;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneOtomasyonu.Services
{
    /// <summary>
    /// Kimlik doğrulama ve yetkilendirme işlemlerini yöneten servis.
    /// </summary>
    public class AuthService
    {
        private readonly LibraryContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

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
            var user = await _context.Users
                .FirstOrDefaultAsync(u => 
                    (u.Username == usernameOrEmail || u.Email == usernameOrEmail) && 
                    u.Status == UserStatus.Active);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return null;

            // Son giriş tarihini güncelle
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Şifreyi hash'ler.
        /// </summary>
        /// <param name="password">Ham şifre</param>
        /// <returns>Hash'lenmiş şifre</returns>
        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "LibrarySalt2024"));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// Şifreyi doğrular.
        /// </summary>
        /// <param name="password">Ham şifre</param>
        /// <param name="hashedPassword">Hash'lenmiş şifre</param>
        /// <returns>Şifre doğru ise true</returns>
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }

        /// <summary>
        /// Mevcut kullanıcıyı session'dan alır.
        /// </summary>
        /// <returns>Giriş yapmış kullanıcı veya null</returns>
        public User? GetCurrentUser()
        {
            var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");
            if (userId == null) return null;

            return _context.Users.Find(userId);
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
        /// Kullanıcıyı session'a kaydeder.
        /// </summary>
        /// <param name="user">Kullanıcı bilgileri</param>
        public void SetUserSession(User user)
        {
            _httpContextAccessor.HttpContext?.Session.SetInt32("UserId", user.UserId);
        }

        /// <summary>
        /// Kullanıcıyı session'dan çıkarır.
        /// </summary>
        public void Logout()
        {
            _httpContextAccessor.HttpContext?.Session.Remove("UserId");
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
