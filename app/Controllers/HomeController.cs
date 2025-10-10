using Microsoft.AspNetCore.Mvc;
using KutuphaneOtomasyonu.Data;
using KutuphaneOtomasyonu.Models;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneOtomasyonu.Controllers
{
    /// <summary>
    /// Ana sayfa controller'ı.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly LibraryContext _context;

        public HomeController(LibraryContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Kitap arama işlemi.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                TempData["Error"] = "Lütfen arama terimi girin.";
                return RedirectToAction("Index");
            }

            var books = await _context.Books
                .Where(b => b.Title.Contains(searchTerm) || 
                           b.Author.Contains(searchTerm) || 
                           b.Category.Contains(searchTerm) ||
                           b.Isbn.Contains(searchTerm))
                .AsNoTracking()
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.SearchResults = books;
            return View("Index");
        }
    }
}

