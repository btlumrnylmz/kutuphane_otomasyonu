using Microsoft.AspNetCore.Mvc;
using KutuphaneOtomasyonu.Data;
using KutuphaneOtomasyonu.Models;
using KutuphaneOtomasyonu.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace KutuphaneOtomasyonu.Controllers
{
    /// <summary>
    /// Ana sayfa controller'ı.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly LibraryContext _context;
        private readonly AuthService _authService;

        public HomeController(LibraryContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            // Giriş yapmış kullanıcılar için dashboard göster
            if (_authService.IsLoggedIn())
            {
                var currentUser = _authService.GetCurrentUser();
                var isAdmin = _authService.IsAdmin();
                
                if (isAdmin)
                {
                    // Yönetici için dashboard istatistikleri
                    var stats = new
                    {
                        TotalBooks = await _context.Books.CountAsync(),
                        TotalMembers = await _context.Members.CountAsync(),
                        ActiveLoans = await _context.Loans.CountAsync(l => l.ReturnedAt == null),
                        OverdueLoans = await _context.Loans.CountAsync(l => l.ReturnedAt == null && l.DueAt < DateTime.UtcNow),
                        AvailableCopies = await _context.Copies.CountAsync(c => c.Status == CopyStatus.Available),
                        PendingReturns = await _context.ReturnRequests.CountAsync(r => r.Status == ReturnRequestStatus.Pending)
                    };
                    
                    ViewBag.Stats = stats;
                    ViewBag.IsDashboard = true;
                }
                else
                {
                    // Kullanıcı için dashboard istatistikleri
                    var member = await _context.Members
                        .FirstOrDefaultAsync(m => m.Email.ToLower() == currentUser.Email.ToLower());
                    
                    if (member != null)
                    {
                        var stats = new
                        {
                            ActiveLoans = await _context.Loans.CountAsync(l => l.MemberId == member.MemberId && l.ReturnedAt == null),
                            OverdueLoans = await _context.Loans.CountAsync(l => l.MemberId == member.MemberId && l.ReturnedAt == null && l.DueAt < DateTime.UtcNow),
                            FavoriteBooks = await _context.Favorites.CountAsync(f => f.UserId == currentUser.UserId),
                            TotalBooks = await _context.Books.CountAsync(),
                            AvailableCopies = await _context.Copies.CountAsync(c => c.Status == CopyStatus.Available)
                        };
                        
                        ViewBag.Stats = stats;
                        ViewBag.Member = member;
                        ViewBag.IsDashboard = true;
                    }
                }
            }
            
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

