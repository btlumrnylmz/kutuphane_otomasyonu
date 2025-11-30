using System.Linq;
using System.Threading.Tasks;
using KutuphaneOtomasyonu.Data;
using KutuphaneOtomasyonu.Models;
using KutuphaneOtomasyonu.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneOtomasyonu.Controllers
{
    /// <summary>
    /// Kopya CRUD işlemlerini yöneten controller. Sadece yöneticiler için.
    /// </summary>
    public class CopyController : Controller
    {
        private readonly LibraryContext _context;
        private readonly AuthService _authService;

        public CopyController(LibraryContext context, AuthService authService)
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

            var copies = await _context.Copies.Include(c => c.Book).AsNoTracking().ToListAsync();
            return View(copies);
        }

        public async Task<IActionResult> Details(int? id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id == null) return NotFound();
            var copy = await _context.Copies
                .Include(c => c.Book)
                .FirstOrDefaultAsync(m => m.CopyId == id);
            if (copy == null) return NotFound();
            return View(copy);
        }

        public IActionResult Create()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            ViewBag.BookId = new SelectList(_context.Books.AsNoTracking().ToList(), "BookId", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CopyId,BookId,ShelfLocation,Status")] Copy copy)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (ModelState.IsValid)
            {
                _context.Add(copy);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kopya başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.BookId = new SelectList(_context.Books, "BookId", "Title", copy.BookId);
            return View(copy);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id == null) return NotFound();
            var copy = await _context.Copies.FindAsync(id);
            if (copy == null) return NotFound();
            ViewBag.BookId = new SelectList(_context.Books.AsNoTracking().ToList(), "BookId", "Title", copy.BookId);
            return View(copy);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CopyId,BookId,ShelfLocation,Status")] Copy copy)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id != copy.CopyId) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewBag.BookId = new SelectList(_context.Books, "BookId", "Title", copy.BookId);
                return View(copy);
            }

            _context.Update(copy);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Kopya başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id == null) return NotFound();
            var copy = await _context.Copies.Include(c => c.Book).AsNoTracking().FirstOrDefaultAsync(m => m.CopyId == id);
            if (copy == null) return NotFound();
            return View(copy);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            var copy = await _context.Copies.FindAsync(id);
            if (copy != null)
            {
                _context.Copies.Remove(copy);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kopya başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}


