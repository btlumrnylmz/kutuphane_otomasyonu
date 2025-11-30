using System;
using System.Linq;
using System.Threading.Tasks;
using KutuphaneOtomasyonu.Data;
using KutuphaneOtomasyonu.Models;
using KutuphaneOtomasyonu.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneOtomasyonu.Controllers
{
    /// <summary>
    /// Üye CRUD işlemlerini yöneten controller.
    /// </summary>
    public class MemberController : Controller
    {
        private readonly LibraryContext _context;
        private readonly AuthService _authService;

        public MemberController(LibraryContext context, AuthService authService)
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
        /// Kullanıcılar için kendi profil sayfası (giriş yapmış kullanıcılar için).
        /// </summary>
        public async Task<IActionResult> Profile()
        {
            if (!_authService.IsLoggedIn())
            {
                TempData["Error"] = "Bu sayfaya erişmek için giriş yapmalısınız.";
                return RedirectToAction("Login", "Auth");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
                return RedirectToAction("Index", "Home");
            }

            // Kullanıcının email'ine göre üyeyi bul
            var member = await _context.Members
                .Include(m => m.Loans)
                    .ThenInclude(l => l.Copy)
                        .ThenInclude(c => c.Book)
                .FirstOrDefaultAsync(m => m.Email.ToLower() == currentUser.Email.ToLower());

            if (member == null)
            {
                TempData["Error"] = "Üye kaydınız bulunamadı. Lütfen yönetici ile iletişime geçin.";
                return RedirectToAction("Index", "Home");
            }

            // İstatistikler
            var activeLoans = member.Loans?.Count(l => l.ReturnedAt == null) ?? 0;
            var overdueLoans = member.Loans?.Count(l => l.ReturnedAt == null && l.DueAt < DateTime.UtcNow) ?? 0;
            var totalLoans = member.Loans?.Count ?? 0;
            var favoriteBooks = await _context.Favorites.CountAsync(f => f.UserId == currentUser.UserId);

            ViewBag.ActiveLoans = activeLoans;
            ViewBag.OverdueLoans = overdueLoans;
            ViewBag.TotalLoans = totalLoans;
            ViewBag.FavoriteBooks = favoriteBooks;
            ViewBag.User = currentUser;

            return View(member);
        }

        /// <summary>
        /// Üye kayıt sayfası (yönetici kontrolü olmadan, herkes erişebilir).
        /// </summary>
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// Üye kayıt işlemi (yönetici kontrolü olmadan).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(MemberRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Email'in hem Member hem de User tablolarında kullanılıp kullanılmadığını kontrol et
                    var emailExists = await _context.Members.AnyAsync(m => m.Email.ToLower() == model.Email.ToLower()) ||
                                     await _context.Users.AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());

                    if (emailExists)
                    {
                        ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılmaktadır. Lütfen farklı bir e-posta girin.");
                        return View(model);
                    }

                    // Username olarak email'in @ öncesi kısmını kullan (eğer benzersiz değilse email'in tamamını kullan)
                    var username = model.Email.Split('@')[0];
                    var usernameExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
                    if (usernameExists)
                    {
                        username = model.Email.Replace("@", "_").Replace(".", "_");
                    }

                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // Member oluştur
                        var member = new Member
                        {
                            FullName = model.FullName,
                            Email = model.Email,
                            Phone = model.Phone,
                            JoinedAt = DateTime.UtcNow,
                            Status = MemberStatus.Active
                        };
                        _context.Members.Add(member);
                        await _context.SaveChangesAsync();

                        // User oluştur (giriş için)
                        var user = new User
                        {
                            Username = username,
                            Email = model.Email,
                            FullName = model.FullName,
                            PasswordHash = _authService.HashPassword(model.Password),
                            Role = UserRole.User,
                            Status = UserStatus.Active,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Users.Add(user);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();

                        // Otomatik giriş yap
                        _authService.SetUserSession(user);

                        TempData["Success"] = $"Hoş geldiniz, {model.FullName}! Üyelik kaydınız başarıyla oluşturuldu ve giriş yaptınız.";
                        return RedirectToAction("Index", "Home");
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Members_Email") == true || 
                                                   ex.InnerException?.Message.Contains("IX_Users_Email") == true)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılmaktadır. Lütfen farklı bir e-posta girin.");
                    return View(model);
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Users_Username") == true)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi için kullanıcı adı oluşturulamadı. Lütfen farklı bir e-posta girin.");
                    return View(model);
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlıdır. Lütfen farklı bir e-posta girin.");
                    return View(model);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Kayıt işlemi sırasında bir hata oluştu: {ex.Message}");
                    return View(model);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string searchTerm = "")
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            IQueryable<Member> query = _context.Members.AsNoTracking();

            // Arama filtresi
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(m => 
                    m.FullName.ToLower().Contains(searchTerm) ||
                    m.Email.ToLower().Contains(searchTerm) ||
                    (m.Phone != null && m.Phone.ToLower().Contains(searchTerm)));
            }

            var totalItems = await query.CountAsync();
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 100);

            var members = await query
                .OrderBy(m => m.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagination = new PaginationViewModel
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                ActionName = "Index",
                ControllerName = "Member",
                QueryParameters = PaginationViewModel.GetQueryParameters(Request)
            };

            ViewBag.Pagination = pagination;
            ViewBag.SearchTerm = searchTerm;

            return View(members);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id == null) return NotFound();
            var member = await _context.Members
                .Include(m => m.Loans)
                .ThenInclude(l => l.Copy)
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null) return NotFound();
            return View(member);
        }

        public IActionResult Create()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MemberId,FullName,Email,Phone,JoinedAt,Status")] Member member)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(member);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Üye başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Members_Email") == true)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılmaktadır. Lütfen farklı bir e-posta girin.");
                    return View(member);
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlıdır. Lütfen farklı bir e-posta girin.");
                    return View(member);
                }
            }
            return View(member);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id == null) return NotFound();
            var member = await _context.Members.FindAsync(id);
            if (member == null) return NotFound();
            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MemberId,FullName,Email,Phone,JoinedAt,Status")] Member member)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id != member.MemberId) return NotFound();
            if (!ModelState.IsValid) return View(member);

            try
            {
                _context.Update(member);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Üye başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Members_Email") == true)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılmaktadır. Lütfen farklı bir e-posta girin.");
                return View(member);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlıdır. Lütfen farklı bir e-posta girin.");
                return View(member);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id == null) return NotFound();
            var member = await _context.Members.AsNoTracking().FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null) return NotFound();
            return View(member);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                _context.Members.Remove(member);
                TempData["Success"] = "Üye başarıyla silindi.";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}


