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
    /// Kitap CRUD işlemlerini yöneten controller.
    /// </summary>
    public class BookController : Controller
    {
        private readonly LibraryContext _context;
        private readonly AuthService _authService;

        public BookController(LibraryContext context, AuthService authService)
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

        /// <summary>
        /// Kitap listesi.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.AsNoTracking().ToListAsync();
            return View(books);
        }

        /// <summary>
        /// Kitap detayları.
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var book = await _context.Books
                .Include(b => b.Copies)
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null) return NotFound();
            return View(book);
        }

        /// <summary>
        /// Yeni kitap formu.
        /// </summary>
        public IActionResult Create()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            return View();
        }

        /// <summary>
        /// Yeni kitap oluşturma post işlemi.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,Isbn,Title,Author,PublishYear,Category")] Book book)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(book);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Kitap başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Books_Isbn") == true)
                {
                    ModelState.AddModelError("Isbn", "Bu ISBN numarası zaten kullanılmaktadır. Lütfen farklı bir ISBN girin.");
                    return View(book);
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    ModelState.AddModelError("Isbn", "Bu ISBN numarası zaten kayıtlıdır. Lütfen farklı bir ISBN girin.");
                    return View(book);
                }
            }
            return View(book);
        }

        /// <summary>
        /// Kitap düzenleme formu.
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id == null) return NotFound();
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        /// <summary>
        /// Kitap düzenleme post işlemi.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookId,Isbn,Title,Author,PublishYear,Category")] Book book)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id != book.BookId) return NotFound();
            if (!ModelState.IsValid) return View(book);

            try
            {
                _context.Update(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kitap başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Books_Isbn") == true)
            {
                ModelState.AddModelError("Isbn", "Bu ISBN numarası zaten kullanılmaktadır. Lütfen farklı bir ISBN girin.");
                return View(book);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
            {
                ModelState.AddModelError("Isbn", "Bu ISBN numarası zaten kayıtlıdır. Lütfen farklı bir ISBN girin.");
                return View(book);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Books.Any(e => e.BookId == id)) return NotFound();
                throw;
            }
        }

        /// <summary>
        /// Silme onay sayfası.
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id == null) return NotFound();
            var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null) return NotFound();
            return View(book);
        }

        /// <summary>
        /// Silme post işlemi.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kitap başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}


