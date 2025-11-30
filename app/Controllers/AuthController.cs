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
        private readonly RateLimitingService _rateLimitingService;

        public AuthController(AuthService authService, RateLimitingService rateLimitingService)
        {
            _authService = authService;
            _rateLimitingService = rateLimitingService;
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
        /// Kullanıcı girişi yapar. Rate limiting ile korunur (5 dakikada 5 deneme).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Rate limiting kontrolü - IP adresi ve kullanıcı adı kombinasyonu
            var rateLimitKey = $"{HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}_{model.UsernameOrEmail}";
            
            if (!_rateLimitingService.IsAllowed(rateLimitKey, maxAttempts: 5, windowMinutes: 5))
            {
                var remainingAttempts = _rateLimitingService.GetRemainingAttempts(rateLimitKey);
                ModelState.AddModelError("", 
                    $"Çok fazla başarısız giriş denemesi. Lütfen 5 dakika bekleyin ve tekrar deneyin.");
                ViewBag.RateLimitExceeded = true;
                return View(model);
            }

            var user = await _authService.LoginAsync(model.UsernameOrEmail, model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı adı/şifre hatalı veya hesap aktif değil.");
                var remaining = _rateLimitingService.GetRemainingAttempts(rateLimitKey);
                if (remaining > 0)
                {
                    ModelState.AddModelError("", $"Kalan deneme hakkı: {remaining}");
                }
                return View(model);
            }

            // Başarılı login - rate limiting kayıtlarını temizle
            _rateLimitingService.ClearAttempts(rateLimitKey);

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
