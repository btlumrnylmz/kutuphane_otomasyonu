using System;
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
    /// Ödünç ve iade akışını yöneten controller.
    /// </summary>
    public class LoanController : Controller
    {
        private readonly LibraryContext _context;

        public LoanController(LibraryContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Aktif ve iade edilmiş ödünç kayıtlarının listesi.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var loans = await _context.Loans
                .Include(l => l.Member)
                .Include(l => l.Copy).ThenInclude(c => c.Book)
                .OrderByDescending(l => l.LoanedAt)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Members = new SelectList(await _context.Members.AsNoTracking().ToListAsync(), "MemberId", "FullName");
            ViewBag.AvailableCopies = new SelectList(await _context.Copies.Include(c => c.Book).Where(c => c.Status == CopyStatus.Available).AsNoTracking().ToListAsync(), "CopyId", "ShelfLocation");
            ViewBag.LoanedCopies = new SelectList(await _context.Copies.Include(c => c.Book).Where(c => c.Status == CopyStatus.Loaned).AsNoTracking().ToListAsync(), "CopyId", "ShelfLocation");
            return View(loans);
        }

        /// <summary>
        /// Ödünç alma işlemi. İş kuralları ve transaction içerir.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(int memberId, int copyId)
        {
            if (memberId <= 0 || copyId <= 0)
            {
                TempData["Error"] = "Geçersiz üye veya kopya seçimi.";
                return RedirectToAction(nameof(Index));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var member = await _context.Members.FindAsync(memberId);
                if (member == null)
                {
                    TempData["Error"] = "Seçilen üye bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                var copy = await _context.Copies.Include(c => c.Book).FirstOrDefaultAsync(c => c.CopyId == copyId);
                if (copy == null)
                {
                    TempData["Error"] = "Seçilen kopya bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                if (copy.Status != CopyStatus.Available)
                {
                    TempData["Error"] = $"'{copy.Book?.Title}' kitabı şu anda ödünç verilemez. (Durum: {copy.Status})";
                    return RedirectToAction(nameof(Index));
                }

                var activeLoansCount = await _context.Loans.CountAsync(l => l.MemberId == memberId && l.ReturnedAt == null);
                if (activeLoansCount >= 3)
                {
                    TempData["Error"] = $"'{member.FullName}' üyesinin 3'ten fazla aktif ödünç kaydı olamaz. Mevcut aktif ödünç sayısı: {activeLoansCount}";
                    return RedirectToAction(nameof(Index));
                }

                var now = DateTime.UtcNow;
                var loan = new Loan
                {
                    MemberId = memberId,
                    CopyId = copyId,
                    LoanedAt = now,
                    DueAt = now.AddDays(14),
                    ReturnedAt = null
                };

                copy.Status = CopyStatus.Loaned;
                _context.Loans.Add(loan);
                _context.Copies.Update(copy);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                TempData["Success"] = $"'{copy.Book?.Title}' kitabı '{member.FullName}' üyesine başarıyla ödünç verildi. İade tarihi: {loan.DueAt:dd.MM.yyyy}";
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Veritabanı hatası: {ex.InnerException?.Message ?? ex.Message}";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Ödünç işlemi sırasında bir hata oluştu: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Kopya müsait değilse rezervasyon kuyruğuna ekler.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reserve(int memberId, int copyId)
        {
            var copy = await _context.Copies.Include(c => c.Book).FirstOrDefaultAsync(c => c.CopyId == copyId);
            if (copy == null)
            {
                TempData["Error"] = "Kopya bulunamadı.";
                return RedirectToAction(nameof(Index));
            }
            if (copy.Status != CopyStatus.Loaned)
            {
                TempData["Error"] = "Sadece ödünçteki kopyalar için rezervasyon yapılabilir.";
                return RedirectToAction(nameof(Index));
            }

            var exists = await _context.Set<Reservation>().AnyAsync(r => r.MemberId == memberId && r.CopyId == copyId && !r.Notified);
            if (exists)
            {
                TempData["Error"] = "Bu kopya için zaten bekleme listesine eklenmişsiniz.";
                return RedirectToAction(nameof(Index));
            }

            _context.Set<Reservation>().Add(new Reservation
            {
                MemberId = memberId,
                CopyId = copyId,
                ReservedAt = DateTime.UtcNow,
                Notified = false
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Rezervasyon kuyruğuna eklendiniz.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// İade işlemi. İş kuralları ve transaction içerir.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int loanId)
        {
            if (loanId <= 0)
            {
                TempData["Error"] = "Geçersiz ödünç kaydı ID'si.";
                return RedirectToAction(nameof(Index));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var loan = await _context.Loans
                    .Include(l => l.Copy).ThenInclude(c => c.Book)
                    .Include(l => l.Member)
                    .FirstOrDefaultAsync(l => l.LoanId == loanId);

                if (loan == null)
                {
                    TempData["Error"] = "Ödünç kaydı bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                if (loan.ReturnedAt != null)
                {
                    TempData["Error"] = $"Bu ödünç kaydı zaten iade edilmiş. İade tarihi: {loan.ReturnedAt:dd.MM.yyyy HH:mm}";
                    return RedirectToAction(nameof(Index));
                }

                var returnTime = DateTime.UtcNow;
                var isOverdue = returnTime > loan.DueAt;
                var daysLate = isOverdue ? (returnTime - loan.DueAt).Days : 0;

                loan.ReturnedAt = returnTime;
                loan.Copy.Status = CopyStatus.Available;
                _context.Loans.Update(loan);
                _context.Copies.Update(loan.Copy);
                await _context.SaveChangesAsync();

                // Rezervasyon varsa ilkini bildir ve simüle e-posta logla
                var firstReservation = await _context.Set<Reservation>()
                    .Include(r => r.Member)
                    .Include(r => r.Copy).ThenInclude(c => c.Book)
                    .Where(r => r.CopyId == loan.CopyId && r.Notified == false)
                    .OrderBy(r => r.ReservedAt)
                    .FirstOrDefaultAsync();

                var notificationMessage = "";
                if (firstReservation != null)
                {
                    firstReservation.Notified = true;
                    _context.Update(firstReservation);
                    await _context.SaveChangesAsync();
                    notificationMessage = $" Rezervasyon bildirimi gönderildi: {firstReservation.Member.FullName} için {firstReservation.Copy.Book.Title} artık müsait.";
                    Console.WriteLine($"📧 [Simülasyon] {notificationMessage}");
                }

                await transaction.CommitAsync();

                var successMessage = $"'{loan.Copy.Book?.Title}' kitabı başarıyla iade edildi.";
                if (isOverdue)
                {
                    successMessage += $" (Gecikme: {daysLate} gün)";
                }
                successMessage += notificationMessage;

                TempData["Success"] = successMessage;
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Veritabanı hatası: {ex.InnerException?.Message ?? ex.Message}";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"İade işlemi sırasında bir hata oluştu: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}


