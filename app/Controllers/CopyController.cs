using System.Linq;
using System.Threading.Tasks;
using KutuphaneOtomasyonu.Data;
using KutuphaneOtomasyonu.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneOtomasyonu.Controllers
{
    /// <summary>
    /// Kopya CRUD işlemlerini yöneten controller.
    /// </summary>
    public class CopyController : Controller
    {
        private readonly LibraryContext _context;

        public CopyController(LibraryContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var copies = await _context.Copies.Include(c => c.Book).AsNoTracking().ToListAsync();
            return View(copies);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var copy = await _context.Copies
                .Include(c => c.Book)
                .FirstOrDefaultAsync(m => m.CopyId == id);
            if (copy == null) return NotFound();
            return View(copy);
        }

        public IActionResult Create()
        {
            ViewBag.BookId = new SelectList(_context.Books.AsNoTracking().ToList(), "BookId", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CopyId,BookId,ShelfLocation,Status")] Copy copy)
        {
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
            if (id == null) return NotFound();
            var copy = await _context.Copies.Include(c => c.Book).AsNoTracking().FirstOrDefaultAsync(m => m.CopyId == id);
            if (copy == null) return NotFound();
            return View(copy);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
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


