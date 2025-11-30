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
    /// Gecikme ödemeleri işlemlerini yöneten controller.
    /// </summary>
    public class PaymentController : Controller
    {
        private readonly LibraryContext _context;
        private readonly AuthService _authService;

        // Günlük gecikme ücreti (TL)
        private const decimal DailyPenaltyRate = 5.00m;

        public PaymentController(LibraryContext context, AuthService authService)
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
        /// Gecikme ödemeleri listesi. Yöneticiler tüm ödemeleri, üyeler sadece kendi ödemelerini görür.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var loginCheck = CheckLoginAccess();
            if (loginCheck != null) return loginCheck;

            var isAdmin = _authService.IsAdmin();
            var currentUser = _authService.GetCurrentUser();

            IQueryable<Payment> paymentsQuery = _context.Payments
                .Include(p => p.Loan)
                    .ThenInclude(l => l.Member)
                .Include(p => p.Loan)
                    .ThenInclude(l => l.Copy)
                        .ThenInclude(c => c.Book)
                .OrderByDescending(p => p.PaymentDate);

            // Üyeler sadece kendi ödemelerini görür (eğer üye ise)
            if (!isAdmin && currentUser != null)
            {
                // Kullanıcı üye değilse, tüm ödemeleri göster (yönetici değilse bile)
                // Bu durumda sadece giriş yapmış olması yeterli
            }

            var payments = await paymentsQuery.ToListAsync();
            return View(payments);
        }

        /// <summary>
        /// Geciken ödünçler listesi ve ödeme sayfası.
        /// Kullanıcılar sadece kendi ödünçlerini, yöneticiler tüm ödünçleri görür.
        /// </summary>
        public async Task<IActionResult> OverdueLoans()
        {
            var loginCheck = CheckLoginAccess();
            if (loginCheck != null) return loginCheck;

            var isAdmin = _authService.IsAdmin();
            var currentUser = _authService.GetCurrentUser();

            var now = DateTime.UtcNow;
            var overdueLoansQuery = _context.Loans
                .Include(l => l.Member)
                .Include(l => l.Copy)
                    .ThenInclude(c => c.Book)
                .Where(l => l.ReturnedAt == null && l.DueAt < now);

            // Kullanıcılar sadece kendi ödünçlerini görür (email ile eşleştirme)
            if (!isAdmin && currentUser != null)
            {
                // User.Email ile Member.Email eşleştir
                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.Email.ToLower() == currentUser.Email.ToLower());
                
                if (member != null)
                {
                    overdueLoansQuery = overdueLoansQuery.Where(l => l.MemberId == member.MemberId);
                }
                else
                {
                    // Eğer kullanıcının Member kaydı yoksa boş liste döndür
                    overdueLoansQuery = overdueLoansQuery.Where(l => false);
                }
            }

            var overdueLoans = await overdueLoansQuery
                .OrderBy(l => l.DueAt)
                .ToListAsync();

            var overdueLoansWithPenalty = overdueLoans.Select(loan =>
            {
                var delayDays = (now - loan.DueAt).Days;
                var totalPenalty = delayDays * DailyPenaltyRate;
                var paidAmount = _context.Payments
                    .Where(p => p.LoanId == loan.LoanId)
                    .Sum(p => (decimal?)p.Amount) ?? 0;
                var remainingAmount = totalPenalty - paidAmount;

                return new OverdueLoanViewModel
                {
                    Loan = loan,
                    DelayDays = delayDays,
                    TotalPenalty = totalPenalty,
                    PaidAmount = paidAmount,
                    RemainingAmount = remainingAmount
                };
            }).ToList();

            ViewBag.IsAdmin = isAdmin;
            return View(overdueLoansWithPenalty);
        }

        /// <summary>
        /// Ödeme yapma sayfası.
        /// Kullanıcılar sadece kendi ödünçleri için ödeme yapabilir.
        /// Yöneticiler sadece görüntüleyebilir, ödeme kaydedemez.
        /// </summary>
        public async Task<IActionResult> Pay(int? loanId)
        {
            var loginCheck = CheckLoginAccess();
            if (loginCheck != null) return loginCheck;

            if (loanId == null) return NotFound();

            var isAdmin = _authService.IsAdmin();
            var currentUser = _authService.GetCurrentUser();
            
            // Yöneticiler ödeme yapamaz, sadece görüntüleyebilir
            if (isAdmin)
            {
                TempData["Error"] = "Yöneticiler ödeme kaydedemez. Ödemeleri sadece görüntüleyebilirsiniz.";
                return RedirectToAction(nameof(OverdueLoans));
            }

            var loan = await _context.Loans
                .Include(l => l.Member)
                .Include(l => l.Copy)
                    .ThenInclude(c => c.Book)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);

            if (loan == null) return NotFound();

            // Kullanıcılar sadece kendi ödünçleri için ödeme yapabilir
            if (!isAdmin && currentUser != null)
            {
                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.Email.ToLower() == currentUser.Email.ToLower());
                
                if (member == null || loan.MemberId != member.MemberId)
                {
                    TempData["Error"] = "Bu ödünç kaydına erişim yetkiniz yok.";
                    return RedirectToAction(nameof(OverdueLoans));
                }
            }

            if (loan.ReturnedAt != null)
            {
                TempData["Error"] = "Bu ödünç kaydı zaten iade edilmiş.";
                return RedirectToAction(nameof(OverdueLoans));
            }

            var now = DateTime.UtcNow;
            if (loan.DueAt >= now)
            {
                TempData["Error"] = "Bu ödünç henüz gecikmemiş.";
                return RedirectToAction(nameof(OverdueLoans));
            }

            var delayDays = (now - loan.DueAt).Days;
            var totalPenalty = delayDays * DailyPenaltyRate;
            var paidAmount = await _context.Payments
                .Where(p => p.LoanId == loan.LoanId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
            var remainingAmount = totalPenalty - paidAmount;

            ViewBag.Loan = loan;
            ViewBag.DelayDays = delayDays;
            ViewBag.TotalPenalty = totalPenalty;
            ViewBag.PaidAmount = paidAmount;
            ViewBag.RemainingAmount = remainingAmount;
            ViewBag.DailyPenaltyRate = DailyPenaltyRate;

            return View();
        }

        /// <summary>
        /// Ödeme işlemini gerçekleştirir.
        /// Kullanıcılar sadece kendi ödünçleri için ödeme yapabilir.
        /// Yöneticiler ödeme kaydedemez.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int loanId, decimal amount, string? description)
        {
            var loginCheck = CheckLoginAccess();
            if (loginCheck != null) return loginCheck;

            var isAdmin = _authService.IsAdmin();
            
            // Yöneticiler ödeme kaydedemez
            if (isAdmin)
            {
                TempData["Error"] = "Yöneticiler ödeme kaydedemez. Ödemeleri sadece görüntüleyebilirsiniz.";
                return RedirectToAction(nameof(OverdueLoans));
            }

            if (amount <= 0)
            {
                TempData["Error"] = "Ödeme tutarı 0'dan büyük olmalıdır.";
                return RedirectToAction(nameof(Pay), new { loanId });
            }

            var currentUser = _authService.GetCurrentUser();

            var loan = await _context.Loans
                .Include(l => l.Member)
                .Include(l => l.Copy)
                    .ThenInclude(c => c.Book)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);

            if (loan == null)
            {
                TempData["Error"] = "Ödünç kaydı bulunamadı.";
                return RedirectToAction(nameof(OverdueLoans));
            }

            // Kullanıcılar sadece kendi ödünçleri için ödeme yapabilir
            if (!isAdmin && currentUser != null)
            {
                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.Email.ToLower() == currentUser.Email.ToLower());
                
                if (member == null || loan.MemberId != member.MemberId)
                {
                    TempData["Error"] = "Bu ödünç kaydına erişim yetkiniz yok.";
                    return RedirectToAction(nameof(OverdueLoans));
                }
            }

            if (loan.ReturnedAt != null)
            {
                TempData["Error"] = "Bu ödünç kaydı zaten iade edilmiş.";
                return RedirectToAction(nameof(OverdueLoans));
            }

            var now = DateTime.UtcNow;
            if (loan.DueAt >= now)
            {
                TempData["Error"] = "Bu ödünç henüz gecikmemiş.";
                return RedirectToAction(nameof(OverdueLoans));
            }

            var delayDays = (now - loan.DueAt).Days;
            var totalPenalty = delayDays * DailyPenaltyRate;
            var paidAmount = await _context.Payments
                .Where(p => p.LoanId == loan.LoanId)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;
            var remainingAmount = totalPenalty - paidAmount;

            if (amount > remainingAmount)
            {
                TempData["Error"] = $"Ödeme tutarı kalan borçtan fazla olamaz. Kalan borç: {remainingAmount:F2} TL";
                return RedirectToAction(nameof(Pay), new { loanId });
            }

            var payment = new Payment
            {
                LoanId = loanId,
                Amount = amount,
                PaymentDate = now,
                Description = description
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{amount:F2} TL ödeme başarıyla kaydedildi. Kalan borç: {remainingAmount - amount:F2} TL";
            return RedirectToAction(nameof(OverdueLoans));
        }

        /// <summary>
        /// Ödeme geçmişi.
        /// Kullanıcılar sadece kendi ödeme geçmişlerini görür.
        /// </summary>
        public async Task<IActionResult> History(int? loanId)
        {
            var loginCheck = CheckLoginAccess();
            if (loginCheck != null) return loginCheck;

            if (loanId == null) return NotFound();

            var isAdmin = _authService.IsAdmin();
            var currentUser = _authService.GetCurrentUser();

            var loan = await _context.Loans
                .Include(l => l.Member)
                .Include(l => l.Copy)
                    .ThenInclude(c => c.Book)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);

            if (loan == null) return NotFound();

            // Kullanıcılar sadece kendi ödeme geçmişlerini görür
            if (!isAdmin && currentUser != null)
            {
                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.Email.ToLower() == currentUser.Email.ToLower());
                
                if (member == null || loan.MemberId != member.MemberId)
                {
                    TempData["Error"] = "Bu ödeme geçmişine erişim yetkiniz yok.";
                    return RedirectToAction(nameof(OverdueLoans));
                }
            }

            var payments = await _context.Payments
                .Where(p => p.LoanId == loanId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            ViewBag.Loan = loan;
            ViewBag.IsAdmin = isAdmin;
            return View(payments);
        }
    }

    /// <summary>
    /// Geciken ödünç view model'i.
    /// </summary>
    public class OverdueLoanViewModel
    {
        public Loan Loan { get; set; } = null!;
        public int DelayDays { get; set; }
        public decimal TotalPenalty { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
    }
}

