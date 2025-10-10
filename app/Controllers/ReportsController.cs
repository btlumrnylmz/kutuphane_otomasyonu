using System;
using System.Linq;
using System.Threading.Tasks;
using KutuphaneOtomasyonu.Data;
using KutuphaneOtomasyonu.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneOtomasyonu.Controllers
{
    /// <summary>
    /// Rapor sayfaları: En çok ödünç alınanlar, aktif ödünçler, üye bazında ödünç sayısı, audit log, rezervasyonlar
    /// </summary>
    public class ReportsController : Controller
    {
        private readonly LibraryContext _context;
        private readonly AuthService _authService;

        public ReportsController(LibraryContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Giriş kontrolü yapar.
        /// </summary>
        private IActionResult CheckLoginAccess()
        {
            if (!_authService.IsLoggedIn())
            {
                TempData["Error"] = "Bu sayfaya erişmek için giriş yapmalısınız.";
                return RedirectToAction("Login", "Auth");
            }
            return null;
        }

        /// <summary>
        /// Yönetici yetkisi kontrolü yapar.
        /// </summary>
        private IActionResult CheckAdminAccess()
        {
            var loginCheck = CheckLoginAccess();
            if (loginCheck != null) return loginCheck;

            if (!_authService.IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            return null;
        }

        public IActionResult Index()
        {
            var loginCheck = CheckLoginAccess();
            if (loginCheck != null) return loginCheck;
            return View();
        }

        /// <summary>
        /// Son 30 günde en çok ödünç alınan 10 kitap (vw_top_books_last30).
        /// </summary>
        public async Task<IActionResult> TopBooks()
        {
            var loginCheck = CheckLoginAccess();
            if (loginCheck != null) return loginCheck;

            var rows = await _context.Set<TopBookRow>()
                .FromSqlRaw("SELECT title, borrow_count FROM dbo.vw_top_books_last30")
                .AsNoTracking()
                .ToListAsync();
            return View(rows);
        }

        /// <summary>
        /// Aktif ödünç listesi ve gecikme gün sayısı (vw_active_loans).
        /// </summary>
        public async Task<IActionResult> ActiveLoans()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            var rows = await _context.Set<ActiveLoanRow>()
                .FromSqlRaw("SELECT member_name, book_title, due_at, delay_days FROM dbo.vw_active_loans")
                .AsNoTracking()
                .ToListAsync();
            return View(rows);
        }

        /// <summary>
        /// Üye bazında toplam ödünç sayısı (desc).
        /// </summary>
        public async Task<IActionResult> MemberLoanCounts()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            var rows = await _context.Set<MemberLoanCountRow>()
                .FromSqlRaw(@"SELECT m.FullName AS member_name, COUNT(*) AS total_loans
FROM dbo.Loans l INNER JOIN dbo.Members m ON m.MemberId = l.MemberId
GROUP BY m.FullName
ORDER BY COUNT(*) DESC")
                .AsNoTracking()
                .ToListAsync();
            return View(rows);
        }

        /// <summary>
        /// Audit log listesi.
        /// </summary>
        public async Task<IActionResult> AuditLog()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            var rows = await _context.Set<AuditLogRow>()
                .FromSqlRaw(@"SELECT a.Action AS action, a.ActionTime AS action_time, a.LoanId AS loan_id
FROM dbo.Audit_Log a ORDER BY a.ActionTime DESC")
                .AsNoTracking()
                .ToListAsync();
            return View(rows);
        }

        /// <summary>
        /// Rezervasyon kuyruğu ve bildirim durumu.
        /// </summary>
        public async Task<IActionResult> Reservations()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            var rows = await _context.Set<ReservationRow>()
                .FromSqlRaw(@"SELECT r.ReservationId AS reservation_id, m.FullName AS member_name, b.Title AS book_title, r.ReservedAt AS reserved_at, r.Notified AS notified
FROM dbo.Reservations r
INNER JOIN dbo.Members m ON m.MemberId = r.MemberId
INNER JOIN dbo.Copies c ON c.CopyId = r.CopyId
INNER JOIN dbo.Books b ON b.BookId = c.BookId
ORDER BY r.ReservedAt ASC")
                .AsNoTracking()
                .ToListAsync();
            return View(rows);
        }
    }

    // Basit DTO'lar (Keyless). Görünümler bu türleri model olarak kullanır.
    public class TopBookRow
    {
        public string title { get; set; } = string.Empty;
        public int borrow_count { get; set; }
    }
    public class ActiveLoanRow
    {
        public string member_name { get; set; } = string.Empty;
        public string book_title { get; set; } = string.Empty;
        public DateTime due_at { get; set; }
        public int delay_days { get; set; }
    }
    public class MemberLoanCountRow
    {
        public string member_name { get; set; } = string.Empty;
        public int total_loans { get; set; }
    }
    public class AuditLogRow
    {
        public string action { get; set; } = string.Empty;
        public DateTime action_time { get; set; }
        public int loan_id { get; set; }
    }
    public class ReservationRow
    {
        public int reservation_id { get; set; }
        public string member_name { get; set; } = string.Empty;
        public string book_title { get; set; } = string.Empty;
        public DateTime reserved_at { get; set; }
        public bool notified { get; set; }
    }
}


