using KutuphaneOtomasyonu.Data;
using KutuphaneOtomasyonu.Models;
using KutuphaneOtomasyonu.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneOtomasyonu.Controllers
{
    /// <summary>
    /// Kullanıcı yönetimi işlemlerini yöneten controller. Sadece yöneticiler erişebilir.
    /// </summary>
    public class UserController : Controller
    {
        private readonly LibraryContext _context;
        private readonly AuthService _authService;

        public UserController(LibraryContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Yönetici yetkisi kontrolü yapar.
        /// </summary>
        private IActionResult? CheckAdminAccess()
        {
            if (!_authService.IsLoggedIn())
            {
                TempData["Error"] = "Bu sayfaya erişmek için giriş yapmalısınız.";
                return RedirectToAction("Login", "Auth");
            }

            if (!_authService.IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Auth");
            }

            return null;
        }

        /// <summary>
        /// Kullanıcı listesi sayfası.
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string searchTerm = "")
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            IQueryable<User> query = _context.Users.AsNoTracking();

            // Arama filtresi
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u => 
                    u.Username.ToLower().Contains(searchTerm) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.FullName.ToLower().Contains(searchTerm));
            }

            var totalItems = await query.CountAsync();
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 100);

            var users = await query
                .OrderBy(u => u.Username)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagination = new PaginationViewModel
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                ActionName = "Index",
                ControllerName = "User",
                QueryParameters = PaginationViewModel.GetQueryParameters(Request)
            };

            ViewBag.Pagination = pagination;
            ViewBag.SearchTerm = searchTerm;

            return View(users);
        }

        /// <summary>
        /// Kullanıcı detay sayfası.
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        /// <summary>
        /// Yeni kullanıcı oluşturma sayfası.
        /// </summary>
        public IActionResult Create()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            return View();
        }

        /// <summary>
        /// Yeni kullanıcı oluşturma işlemi.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,Email,FullName,Password,Role,Status")] UserCreateViewModel model)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (ModelState.IsValid)
            {
                try
                {
                    var user = new User
                    {
                        Username = model.Username,
                        Email = model.Email,
                        FullName = model.FullName,
                        PasswordHash = _authService.HashPassword(model.Password),
                        Role = model.Role,
                        Status = model.Status,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Kullanıcı başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Users_Username") == true)
                {
                    ModelState.AddModelError("Username", "Bu kullanıcı adı zaten kullanılmaktadır.");
                    return View(model);
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Users_Email") == true)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılmaktadır.");
                    return View(model);
                }
            }
            return View(model);
        }

        /// <summary>
        /// Kullanıcı düzenleme sayfası.
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            
            var model = new UserEditViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Status = user.Status
            };
            
            return View(model);
        }

        /// <summary>
        /// Kullanıcı düzenleme işlemi.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Username,Email,FullName,Password,Role,Status")] UserEditViewModel model)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id != model.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.Users.FindAsync(id);
                    if (user == null) return NotFound();

                    user.Username = model.Username;
                    user.Email = model.Email;
                    user.FullName = model.FullName;
                    user.Role = model.Role;
                    user.Status = model.Status;

                    // Şifre değiştirilmişse güncelle
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        user.PasswordHash = _authService.HashPassword(model.Password);
                    }

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Kullanıcı başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Users_Username") == true)
                {
                    ModelState.AddModelError("Username", "Bu kullanıcı adı zaten kullanılmaktadır.");
                    return View(model);
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Users_Email") == true)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılmaktadır.");
                    return View(model);
                }
            }
            return View(model);
        }

        /// <summary>
        /// Kullanıcı silme onay sayfası.
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id == null) return NotFound();
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null) return NotFound();
            return View(user);
        }

        /// <summary>
        /// Kullanıcı silme işlemi.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Kendi hesabını silmeye çalışıyorsa engelle
                var currentUser = _authService.GetCurrentUser();
                if (currentUser?.UserId == id)
                {
                    TempData["Error"] = "Kendi hesabınızı silemezsiniz.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Users.Remove(user);
                TempData["Success"] = "Kullanıcı başarıyla silindi.";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
