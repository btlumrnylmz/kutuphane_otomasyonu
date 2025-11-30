using KutuphaneOtomasyonu.Models;
using KutuphaneOtomasyonu.Services;
using Microsoft.AspNetCore.Mvc;

namespace KutuphaneOtomasyonu.Controllers
{
    /// <summary>
    /// Kimlik doğrulama işlemlerini yöneten controller.
    /// </summary>
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Giriş sayfasını gösterir.
        /// </summary>
        public IActionResult Login()
        {
            // Zaten giriş yapmışsa ana sayfaya yönlendir
            if (_authService.IsLoggedIn())
            {
                return RedirectToAction("Index", "Home");
            }

            // Diğer sayfalardan gelen TempData mesajlarını temizle
            // Sadece logout mesajını koru
            var logoutMessage = TempData["LogoutMessage"];
            TempData.Clear();
            if (logoutMessage != null)
            {
                TempData["Success"] = logoutMessage;
            }

            return View();
        }

        /// <summary>
        /// Kullanıcı girişi yapar.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _authService.LoginAsync(model.UsernameOrEmail, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı adı/şifre hatalı veya hesap aktif değil.");
                return View(model);
            }

            // Session'a kullanıcıyı kaydet
            _authService.SetUserSession(user);

            // Beni hatırla seçeneği
            if (model.RememberMe)
            {
                // Session süresini uzat (zaten 30 dakika olarak ayarlanmış)
            }

            TempData["Success"] = $"Hoş geldiniz, {user.FullName}!";
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Kullanıcı çıkışı yapar.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _authService.Logout();
            TempData["LogoutMessage"] = "Başarıyla çıkış yaptınız.";
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Erişim reddedildi sayfası.
        /// </summary>
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
