using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using KutuphaneOtomasyonu.Data;
using KutuphaneOtomasyonu.Models;
using KutuphaneOtomasyonu.Services;
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
        private readonly AuthService _authService;

        public LoanController(LibraryContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Giriş kontrolü yapar.
        /// </summary>
        private IActionResult? CheckLoginAccess()
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
        private IActionResult? CheckAdminAccess()
        {
            var loginCheck = CheckLoginAccess();
            if (loginCheck != null) return loginCheck;

            if (!_authService.IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            return null;
        }

        /// <summary>
        /// Aktif ve iade edilmiş ödünç kayıtlarının listesi. Sadece yöneticiler için.
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            IQueryable<Loan> query = _context.Loans
                .Include(l => l.Member)
                .Include(l => l.Copy).ThenInclude(c => c.Book)
                .AsNoTracking();

            var totalItems = await query.CountAsync();
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 100);

            var loans = await query
                .OrderByDescending(l => l.LoanedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Members = new SelectList(await _context.Members.AsNoTracking().ToListAsync(), "MemberId", "FullName");
            
            // Uygun kopyaları getir - null-safe şekilde
            var availableCopiesList = new List<object>();
            try
            {
                // Raw SQL ile null-safe çekme - ISNULL kullanarak
                var availableCopiesData = await _context.Set<CopyData>()
                    .FromSqlRaw(
                        "SELECT c.CopyId, c.BookId, ISNULL(c.ShelfLocation, '') as ShelfLocation, b.Title as BookTitle " +
                        "FROM Copies c " +
                        "INNER JOIN Books b ON c.BookId = b.BookId " +
                        "WHERE c.Status = {0}",
                        CopyStatus.Available.ToString())
                    .AsNoTracking()
                    .ToListAsync();
                
                foreach (var copyData in availableCopiesData)
                {
                    var shelfLoc = string.IsNullOrWhiteSpace(copyData.ShelfLocation) ? "Belirtilmemiş" : copyData.ShelfLocation;
                    availableCopiesList.Add(new { 
                        CopyId = copyData.CopyId, 
                        DisplayText = $"{copyData.BookTitle} (Raf: {shelfLoc})"
                    });
                }
            }
            catch
            {
                // Hata durumunda boş liste
            }
            
            // Ödünçteki kopyaları getir - null-safe şekilde
            var loanedCopiesList = new List<object>();
            try
            {
                // Raw SQL ile null-safe çekme - ISNULL kullanarak
                var loanedCopiesData = await _context.Set<CopyData>()
                    .FromSqlRaw(
                        "SELECT c.CopyId, c.BookId, ISNULL(c.ShelfLocation, '') as ShelfLocation, b.Title as BookTitle " +
                        "FROM Copies c " +
                        "INNER JOIN Books b ON c.BookId = b.BookId " +
                        "WHERE c.Status = {0}",
                        CopyStatus.Loaned.ToString())
                    .AsNoTracking()
                    .ToListAsync();
                
                foreach (var copyData in loanedCopiesData)
                {
                    var shelfLoc = string.IsNullOrWhiteSpace(copyData.ShelfLocation) ? "Belirtilmemiş" : copyData.ShelfLocation;
                    loanedCopiesList.Add(new { 
                        CopyId = copyData.CopyId, 
                        DisplayText = $"{copyData.BookTitle} (Raf: {shelfLoc})"
                    });
                }
            }
            catch
            {
                // Hata durumunda boş liste
            }
            
            ViewBag.AvailableCopies = new SelectList(availableCopiesList, "CopyId", "DisplayText");
            ViewBag.LoanedCopies = new SelectList(loanedCopiesList, "CopyId", "DisplayText");
            
            var pagination = new PaginationViewModel
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                ActionName = "Index",
                ControllerName = "Loan",
                QueryParameters = PaginationViewModel.GetQueryParameters(Request)
            };

            ViewBag.Pagination = pagination;
            
            return View(loans);
        }

        /// <summary>
        /// Kullanıcılar için kitap ödünç alma sayfası.
        /// </summary>
        public async Task<IActionResult> BorrowBook()
        {
            var loginCheck = CheckLoginAccess();
            if (loginCheck != null) return loginCheck;

            // Admin kullanıcıları için Loan/Index sayfasına yönlendir
            if (_authService.IsAdmin())
            {
                return RedirectToAction("Index");
            }

            // Kullanıcının email'ine göre üyeyi bul
            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
                return RedirectToAction("Index", "Home");
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Email.ToLower() == currentUser.Email.ToLower());

            if (member == null)
            {
                TempData["Error"] = "Üye kaydınız bulunamadı. Lütfen yönetici ile iletişime geçin.";
                return RedirectToAction("Index", "Home");
            }

            // Uygun kopyaları getir (kitap başlığı + raf konumu ile)
            var availableCopies = await _context.Copies
                .Include(c => c.Book)
                .Where(c => c.Status == CopyStatus.Available)
                .Select(c => new { 
                    c.CopyId, 
                    DisplayText = $"{c.Book.Title} (Raf: {c.ShelfLocation ?? "Belirtilmemiş"})"
                })
                .ToListAsync();

            ViewBag.AvailableCopies = new SelectList(availableCopies, "CopyId", "DisplayText");
            ViewBag.Member = member;

            return View();
        }

        /// <summary>
        /// Kullanıcılar için kitap ödünç alma işlemi.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrowBook(int copyId)
        {
            var loginCheck = CheckLoginAccess();
            if (loginCheck != null) return loginCheck;

            // Admin kullanıcıları için Loan/Index sayfasına yönlendir
            if (_authService.IsAdmin())
            {
                TempData["Error"] = "Yöneticiler ödünç işlemlerini Loan/Index sayfasından yapabilir.";
                return RedirectToAction("Index");
            }

            // Kullanıcının email'ine göre üyeyi bul
            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
                return RedirectToAction(nameof(BorrowBook));
            }

            var currentMember = await _context.Members
                .FirstOrDefaultAsync(m => m.Email.ToLower() == currentUser.Email.ToLower());

            if (currentMember == null)
            {
                TempData["Error"] = "Üye kaydınız bulunamadı. Lütfen yönetici ile iletişime geçin.";
                return RedirectToAction(nameof(BorrowBook));
            }

            // Borrow metodunu çağır (memberId'yi otomatik olarak kullanıcıdan al)
            return await Borrow(currentMember.MemberId, copyId);
        }

        /// <summary>
        /// Ödünç alma işlemi. İş kuralları ve transaction içerir. Yöneticiler tüm üyeler için, kullanıcılar sadece kendileri için kullanabilir.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(int memberId, int copyId)
        {
            // Eğer yönetici değilse, sadece kendi MemberId'sini kullanabilir
            if (!_authService.IsAdmin())
            {
                var loginCheck = CheckLoginAccess();
                if (loginCheck != null) return loginCheck;

                var currentUser = _authService.GetCurrentUser();
                if (currentUser == null)
                {
                    TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
                    return RedirectToAction(nameof(BorrowBook));
                }

                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.Email.ToLower() == currentUser.Email.ToLower());

                if (member == null || member.MemberId != memberId)
                {
                    TempData["Error"] = "Başkası adına ödünç alma işlemi yapamazsınız.";
                    return RedirectToAction(nameof(BorrowBook));
                }
            }
            else
            {
                // Yöneticiler için admin kontrolü
                var adminCheck = CheckAdminAccess();
                if (adminCheck != null) return adminCheck;
            }

            if (memberId <= 0 || copyId <= 0)
            {
                TempData["Error"] = "Geçersiz üye veya kopya seçimi.";
                if (_authService.IsAdmin())
                    return RedirectToAction(nameof(Index));
                else
                    return RedirectToAction(nameof(BorrowBook));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var member = await _context.Members.FindAsync(memberId);
                if (member == null)
                {
                    TempData["Error"] = "Seçilen üye bulunamadı.";
                    if (_authService.IsAdmin())
                        return RedirectToAction(nameof(Index));
                    else
                        return RedirectToAction(nameof(BorrowBook));
                }

                var copy = await _context.Copies.Include(c => c.Book).FirstOrDefaultAsync(c => c.CopyId == copyId);
                if (copy == null)
                {
                    TempData["Error"] = "Seçilen kopya bulunamadı.";
                    if (_authService.IsAdmin())
                        return RedirectToAction(nameof(Index));
                    else
                        return RedirectToAction(nameof(BorrowBook));
                }

                if (copy.Status != CopyStatus.Available)
                {
                    TempData["Error"] = $"'{copy.Book?.Title}' kitabı şu anda ödünç verilemez. (Durum: {copy.Status})";
                    if (_authService.IsAdmin())
                        return RedirectToAction(nameof(Index));
                    else
                        return RedirectToAction(nameof(BorrowBook));
                }

                var activeLoansCount = await _context.Loans.CountAsync(l => l.MemberId == memberId && l.ReturnedAt == null);
                if (activeLoansCount >= 3)
                {
                    TempData["Error"] = $"'{member.FullName}' üyesinin 3'ten fazla aktif ödünç kaydı olamaz. Mevcut aktif ödünç sayısı: {activeLoansCount}";
                    if (_authService.IsAdmin())
                        return RedirectToAction(nameof(Index));
                    else
                        return RedirectToAction(nameof(BorrowBook));
                }

                // 60 günden fazla gecikmiş ödünç ve ödeme kontrolü
                var now = DateTime.UtcNow;
                var sixtyDaysAgo = now.AddDays(-60);
                
                var overdueLoans = await _context.Loans
                    .Where(l => l.MemberId == memberId 
                        && l.ReturnedAt == null 
                        && l.DueAt < sixtyDaysAgo)
                    .ToListAsync();

                foreach (var overdueLoan in overdueLoans)
                {
                    // Bu ödünç için hiç ödeme yapılmış mı kontrol et
                    var hasPayment = await _context.Payments
                        .AnyAsync(p => p.LoanId == overdueLoan.LoanId);
                    
                    if (!hasPayment)
                    {
                        var daysOverdue = (now - overdueLoan.DueAt).Days;
                        var overdueCopy = await _context.Copies
                            .Include(c => c.Book)
                            .FirstOrDefaultAsync(c => c.CopyId == overdueLoan.CopyId);
                        
                        TempData["Error"] = $"'{member.FullName}' üyesi yeni kitap alamaz. " +
                            $"'{overdueCopy?.Book?.Title ?? "Bilinmeyen"}' kitabı {daysOverdue} gündür gecikmiş ve hiç ödeme yapılmamış. " +
                            $"Lütfen önce gecikme ödemesini yapın.";
                        if (_authService.IsAdmin())
                            return RedirectToAction(nameof(Index));
                        else
                            return RedirectToAction(nameof(BorrowBook));
                    }
                }

                // Loan ekleme ve Copy güncelleme işlemlerini trigger ile çakışmayacak şekilde SQL ile yap
                var dueDate = now.AddDays(14);
                
                // Loan ekleme (OUTPUT clause olmadan)
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"INSERT INTO Loans (MemberId, CopyId, LoanedAt, DueAt, ReturnedAt) VALUES ({memberId}, {copyId}, {now}, {dueDate}, NULL)");
                
                // Copy durumunu güncelleme
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE Copies SET Status = {(int)CopyStatus.Loaned} WHERE CopyId = {copyId}");

                await transaction.CommitAsync();
                TempData["Success"] = $"'{copy.Book?.Title}' kitabı başarıyla ödünç alındı. İade tarihi: {dueDate:dd.MM.yyyy}";
                
                if (_authService.IsAdmin())
                    return RedirectToAction(nameof(Index));
                else
                    return RedirectToAction(nameof(MyLoans));
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
            
            if (_authService.IsAdmin())
                return RedirectToAction(nameof(Index));
            else
                return RedirectToAction(nameof(BorrowBook));
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
        /// Kullanıcı iade talebi oluşturur.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestReturn(int loanId)
        {
            var loginCheck = CheckLoginAccess();
            if (loginCheck != null) return loginCheck;

            if (loanId <= 0)
            {
                TempData["Error"] = "Geçersiz ödünç kaydı ID'si.";
                return RedirectToAction(nameof(MyLoans));
            }

            var loan = await _context.Loans
                .Include(l => l.Copy).ThenInclude(c => c.Book)
                .Include(l => l.Member)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);

            if (loan == null)
            {
                TempData["Error"] = "Ödünç kaydı bulunamadı.";
                return RedirectToAction(nameof(MyLoans));
            }

            if (loan.ReturnedAt != null)
            {
                TempData["Error"] = "Bu ödünç kaydı zaten iade edilmiş.";
                return RedirectToAction(nameof(MyLoans));
            }

            // Zaten bekleyen bir talep var mı kontrol et
            var existingRequest = await _context.ReturnRequests
                .FirstOrDefaultAsync(r => r.LoanId == loanId && r.Status == ReturnRequestStatus.Pending);

            if (existingRequest != null)
            {
                TempData["Error"] = "Bu ödünç için zaten bekleyen bir iade talebiniz var.";
                return RedirectToAction(nameof(MyLoans));
            }

            var returnRequest = new ReturnRequest
            {
                LoanId = loanId,
                RequestedAt = DateTime.UtcNow,
                Status = ReturnRequestStatus.Pending
            };

            _context.ReturnRequests.Add(returnRequest);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{loan.Copy?.Book?.Title}' kitabı için iade talebiniz oluşturuldu. Yönetici onayı bekleniyor.";
            return RedirectToAction(nameof(MyLoans));
        }

        /// <summary>
        /// Kullanıcının kendi ödünçlerini ve iade taleplerini görüntüler.
        /// </summary>
        public async Task<IActionResult> MyLoans()
        {
            var loginCheck = CheckLoginAccess();
            if (loginCheck != null) return loginCheck;

            // Admin kullanıcıları için Loan/Index sayfasına yönlendir
            if (_authService.IsAdmin())
            {
                return RedirectToAction("Index");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
                return RedirectToAction("Login", "Auth");
            }

            // Kullanıcının email'ine göre üyeyi bul
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Email.ToLower() == currentUser.Email.ToLower());

            if (member == null)
            {
                TempData["Error"] = "Üye kaydınız bulunamadı. Lütfen yönetici ile iletişime geçin.";
                return RedirectToAction("Index", "Home");
            }

            var loans = await _context.Loans
                .Include(l => l.Copy).ThenInclude(c => c.Book)
                .Where(l => l.MemberId == member.MemberId)
                .OrderByDescending(l => l.LoanedAt)
                .AsNoTracking()
                .ToListAsync();

            var returnRequests = await _context.ReturnRequests
                .Include(r => r.Loan).ThenInclude(l => l.Copy).ThenInclude(c => c.Book)
                .Include(r => r.ProcessedByUser)
                .Where(r => r.Loan.MemberId == member.MemberId)
                .OrderByDescending(r => r.RequestedAt)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Loans = loans;
            ViewBag.ReturnRequests = returnRequests;
            ViewBag.Member = member;

            return View();
        }

        /// <summary>
        /// Yönetici için bekleyen iade talepleri listesi.
        /// </summary>
        public async Task<IActionResult> PendingReturns()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            var pendingRequests = await _context.ReturnRequests
                .Include(r => r.Loan).ThenInclude(l => l.Member)
                .Include(r => r.Loan).ThenInclude(l => l.Copy).ThenInclude(c => c.Book)
                .Where(r => r.Status == ReturnRequestStatus.Pending)
                .OrderBy(r => r.RequestedAt)
                .AsNoTracking()
                .ToListAsync();

            return View(pendingRequests);
        }

        /// <summary>
        /// Yönetici iade talebini onaylar ve iade işlemini tamamlar.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveReturn(int returnRequestId)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
                return RedirectToAction("Login", "Auth");
            }

            var returnRequest = await _context.ReturnRequests
                .Include(r => r.Loan).ThenInclude(l => l.Copy).ThenInclude(c => c.Book)
                .Include(r => r.Loan).ThenInclude(l => l.Member)
                .FirstOrDefaultAsync(r => r.ReturnRequestId == returnRequestId);

            if (returnRequest == null)
            {
                TempData["Error"] = "İade talebi bulunamadı.";
                return RedirectToAction(nameof(PendingReturns));
            }

            if (returnRequest.Status != ReturnRequestStatus.Pending)
            {
                TempData["Error"] = "Bu iade talebi zaten işlenmiş.";
                return RedirectToAction(nameof(PendingReturns));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var loan = returnRequest.Loan;
                if (loan == null || loan.ReturnedAt != null)
                {
                    TempData["Error"] = "Ödünç kaydı bulunamadı veya zaten iade edilmiş.";
                    return RedirectToAction(nameof(PendingReturns));
                }

                var returnTime = DateTime.UtcNow;
                var isOverdue = returnTime > loan.DueAt;
                var daysLate = isOverdue ? (returnTime - loan.DueAt).Days : 0;

                // Trigger ile uyumlu olması için doğrudan SQL kullanıyoruz
                FormattableString sqlLoan = $"UPDATE dbo.Loans SET ReturnedAt = {returnTime} WHERE LoanId = {loan.LoanId}";
                await _context.Database.ExecuteSqlInterpolatedAsync(sqlLoan);

                // Copy için de doğrudan SQL kullanıyoruz
                FormattableString sqlCopy = $"UPDATE dbo.Copies SET Status = 'Available' WHERE CopyId = {loan.CopyId}";
                await _context.Database.ExecuteSqlInterpolatedAsync(sqlCopy);

                // İade talebini onaylandı olarak işaretle
                returnRequest.Status = ReturnRequestStatus.Approved;
                returnRequest.ProcessedAt = returnTime;
                returnRequest.ProcessedByUserId = currentUser.UserId;
                _context.ReturnRequests.Update(returnRequest);

                // Rezervasyon varsa ilkini bildir
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
                    notificationMessage = $" Rezervasyon bildirimi gönderildi: {firstReservation.Member.FullName} için {firstReservation.Copy.Book.Title} artık müsait.";
                    Console.WriteLine($"📧 [Simülasyon] {notificationMessage}");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var successMessage = $"'{loan.Copy?.Book?.Title}' kitabı başarıyla iade edildi. Üye: {loan.Member?.FullName}";
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

            return RedirectToAction(nameof(PendingReturns));
        }

        /// <summary>
        /// Yönetici iade talebini reddeder.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectReturn(int returnRequestId, string? rejectionReason)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
                return RedirectToAction("Login", "Auth");
            }

            var returnRequest = await _context.ReturnRequests
                .Include(r => r.Loan).ThenInclude(l => l.Copy).ThenInclude(c => c.Book)
                .Include(r => r.Loan).ThenInclude(l => l.Member)
                .FirstOrDefaultAsync(r => r.ReturnRequestId == returnRequestId);

            if (returnRequest == null)
            {
                TempData["Error"] = "İade talebi bulunamadı.";
                return RedirectToAction(nameof(PendingReturns));
            }

            if (returnRequest.Status != ReturnRequestStatus.Pending)
            {
                TempData["Error"] = "Bu iade talebi zaten işlenmiş.";
                return RedirectToAction(nameof(PendingReturns));
            }

            returnRequest.Status = ReturnRequestStatus.Rejected;
            returnRequest.ProcessedAt = DateTime.UtcNow;
            returnRequest.ProcessedByUserId = currentUser.UserId;
            returnRequest.RejectionReason = rejectionReason;

            _context.ReturnRequests.Update(returnRequest);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"İade talebi reddedildi: '{returnRequest.Loan?.Copy?.Book?.Title}' - Üye: {returnRequest.Loan?.Member?.FullName}";
            return RedirectToAction(nameof(PendingReturns));
        }

        /// <summary>
        /// İade işlemi (Yönetici için - eski metod, geriye dönük uyumluluk için).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int loanId)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

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

                // Trigger ile uyumlu olması için doğrudan SQL kullanıyoruz
                // OUTPUT clause trigger ile çakıştığı için ExecuteSqlInterpolated kullanıyoruz
                // Bu yöntem OUTPUT clause kullanmaz
                FormattableString sqlLoan = $"UPDATE dbo.Loans SET ReturnedAt = {returnTime} WHERE LoanId = {loanId}";
                await _context.Database.ExecuteSqlInterpolatedAsync(sqlLoan);

                // Copy için de doğrudan SQL kullanıyoruz
                FormattableString sqlCopy = $"UPDATE dbo.Copies SET Status = 'Available' WHERE CopyId = {loan.CopyId}";
                await _context.Database.ExecuteSqlInterpolatedAsync(sqlCopy);
                
                // Entity'yi yeniden yükle (tracking için)
                loan = await _context.Loans
                    .AsNoTracking()
                    .Include(l => l.Copy).ThenInclude(c => c.Book)
                    .Include(l => l.Member)
                    .FirstOrDefaultAsync(l => l.LoanId == loanId);

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


