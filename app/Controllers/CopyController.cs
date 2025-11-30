using System;
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

        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            IQueryable<Copy> query = _context.Copies.Include(c => c.Book).AsNoTracking();

            var totalItems = await query.CountAsync();
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 100);

            var copies = await query
                .OrderBy(c => c.Book.Title)
                .ThenBy(c => c.ShelfLocation)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagination = new PaginationViewModel
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                ActionName = "Index",
                ControllerName = "Copy",
                QueryParameters = PaginationViewModel.GetQueryParameters(Request)
            };

            ViewBag.Pagination = pagination;

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

            // Raf konumu kontrolü
            if (string.IsNullOrWhiteSpace(copy.ShelfLocation))
            {
                ModelState.AddModelError("ShelfLocation", "Raf konumu zorunludur ve boş bırakılamaz.");
            }

            if (ModelState.IsValid)
            {
                // CopyNumber'ı otomatik hesapla (aynı kitabın kaç kopyası var)
                var existingCopiesCount = await _context.Copies
                    .Where(c => c.BookId == copy.BookId)
                    .CountAsync();
                
                copy.CopyNumber = existingCopiesCount + 1;
                copy.AddedAt = DateTime.UtcNow;
                copy.CreatedAt = DateTime.UtcNow;

                // Raf konumunu trim et
                copy.ShelfLocation = copy.ShelfLocation?.Trim() ?? string.Empty;

                _context.Add(copy);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kopya başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.BookId = new SelectList(_context.Books.AsNoTracking().ToList(), "BookId", "Title", copy.BookId);
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
        public async Task<IActionResult> Edit(int id, [Bind("CopyId,BookId,ShelfLocation,Status,CopyNumber")] Copy copy)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id != copy.CopyId) return NotFound();

            // Raf konumu kontrolü
            if (string.IsNullOrWhiteSpace(copy.ShelfLocation))
            {
                ModelState.AddModelError("ShelfLocation", "Raf konumu zorunludur ve boş bırakılamaz.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.BookId = new SelectList(_context.Books.AsNoTracking().ToList(), "BookId", "Title", copy.BookId);
                return View(copy);
            }

            try
            {
                // Mevcut kopyayı al (CopyNumber ve tarihleri koru)
                var existingCopy = await _context.Copies.FindAsync(id);
                if (existingCopy == null) return NotFound();

                // Sadece değiştirilebilir alanları güncelle
                existingCopy.BookId = copy.BookId;
                existingCopy.Status = copy.Status;
                existingCopy.ShelfLocation = copy.ShelfLocation?.Trim() ?? string.Empty;

                _context.Update(existingCopy);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kopya başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Copies.Any(e => e.CopyId == id)) return NotFound();
                throw;
            }
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


