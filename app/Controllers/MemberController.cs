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
        private IActionResult CheckAdminAccess()
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

        public async Task<IActionResult> Index()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            var members = await _context.Members.AsNoTracking().ToListAsync();
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


