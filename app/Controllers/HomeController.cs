using Microsoft.AspNetCore.Mvc;
using KutuphaneOtomasyonu.Data;
using KutuphaneOtomasyonu.Models;
using KutuphaneOtomasyonu.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Diagnostics;

namespace KutuphaneOtomasyonu.Controllers
{
    /// <summary>
    /// Ana sayfa controller'ı.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly LibraryContext _context;
        private readonly AuthService _authService;
        private readonly ILogger<HomeController> _logger;
        private readonly CacheService _cacheService;

        public HomeController(LibraryContext context, AuthService authService, ILogger<HomeController> logger, CacheService cacheService)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
            _cacheService = cacheService;
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
                    // Yönetici için dashboard istatistikleri (cache ile)
                    var cacheKey = CacheService.CreateKey("dashboard", "admin", "stats");
                    var stats = await _cacheService.GetOrSetAsync(cacheKey, async () => new
                    {
                        TotalBooks = await _context.Books.CountAsync(),
                        TotalMembers = await _context.Members.CountAsync(),
                        ActiveLoans = await _context.Loans.CountAsync(l => l.ReturnedAt == null),
                        OverdueLoans = await _context.Loans.CountAsync(l => l.ReturnedAt == null && l.DueAt < DateTime.UtcNow),
                        AvailableCopies = await _context.Copies.CountAsync(c => c.Status == CopyStatus.Available),
                        PendingReturns = await _context.ReturnRequests.CountAsync(r => r.Status == ReturnRequestStatus.Pending)
                    }, TimeSpan.FromMinutes(5)); // 5 dakika cache
                    
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

        /// <summary>
        /// Hata sayfası.
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            var statusCode = HttpContext.Response.StatusCode;
            var statusCodeReExecuteFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IStatusCodeReExecuteFeature>();
            var exceptionHandlerPathFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
            
            var errorViewModel = new ErrorViewModel
            {
                RequestId = requestId,
                StatusCode = statusCode,
                ExceptionPath = exceptionHandlerPathFeature?.Path,
                ExceptionMessage = exceptionHandlerPathFeature?.Error?.Message
            };

            // Hata loglama
            if (exceptionHandlerPathFeature?.Error != null)
            {
                _logger.LogError(exceptionHandlerPathFeature.Error, 
                    "Hata oluştu: {Path}", exceptionHandlerPathFeature.Path);
            }
            else if (statusCode != 200)
            {
                _logger.LogWarning("HTTP {StatusCode} hatası: {Path}", statusCode, 
                    statusCodeReExecuteFeature?.OriginalPath);
            }

            return View(errorViewModel);
        }

        /// <summary>
        /// Dashboard için grafik verilerini döndürür (AJAX endpoint).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetChartData(string chartType)
        {
            if (!_authService.IsLoggedIn())
            {
                return Unauthorized();
            }

            var currentUser = _authService.GetCurrentUser();
            var isAdmin = _authService.IsAdmin();
            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Email.ToLower() == currentUser.Email.ToLower());

            if (isAdmin)
            {
                var cacheKey = CacheService.CreateKey("dashboard", "charts", chartType);
                var data = await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    return chartType switch
                    {
                        "monthlyLoans" => await GetMonthlyLoansData(),
                        "categoryDistribution" => await GetCategoryDistributionData(),
                        "loanTrend" => await GetLoanTrendData(),
                        _ => null
                    };
                }, TimeSpan.FromMinutes(10));

                return Json(data);
            }
            else if (member != null)
            {
                // Kullanıcı için grafik verileri
                var cacheKey = CacheService.CreateKey("dashboard", "user", currentUser.UserId.ToString(), chartType);
                var data = await _cacheService.GetOrSetAsync(cacheKey, async () =>
                {
                    return chartType switch
                    {
                        "monthlyLoans" => await GetUserMonthlyLoansData(member.MemberId),
                        "categoryDistribution" => await GetUserCategoryDistributionData(member.MemberId),
                        _ => null
                    };
                }, TimeSpan.FromMinutes(5));

                return Json(data);
            }

            return Unauthorized();
        }

        private async Task<object> GetMonthlyLoansData()
        {
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var monthlyData = await _context.Loans
                .Where(l => l.LoanedAt >= sixMonthsAgo)
                .GroupBy(l => new { Year = l.LoanedAt.Year, Month = l.LoanedAt.Month })
                .Select(g => new
                {
                    Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count()
                })
                .OrderBy(x => x.Label)
                .ToListAsync();

            return new
            {
                labels = monthlyData.Select(x => x.Label).ToArray(),
                data = monthlyData.Select(x => x.Count).ToArray()
            };
        }

        private async Task<object> GetCategoryDistributionData()
        {
            var categoryData = await _context.Books
                .GroupBy(b => b.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            return new
            {
                labels = categoryData.Select(x => x.Category).ToArray(),
                data = categoryData.Select(x => x.Count).ToArray()
            };
        }

        private async Task<object> GetLoanTrendData()
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var dailyData = await _context.Loans
                .Where(l => l.LoanedAt >= thirtyDaysAgo)
                .GroupBy(l => l.LoanedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return new
            {
                labels = dailyData.Select(x => x.Date.ToString("MM-dd")).ToArray(),
                data = dailyData.Select(x => x.Count).ToArray()
            };
        }

        /// <summary>
        /// Kullanıcının aylık ödünç istatistiklerini döndürür.
        /// </summary>
        private async Task<object> GetUserMonthlyLoansData(int memberId)
        {
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var monthlyData = await _context.Loans
                .Where(l => l.MemberId == memberId && l.LoanedAt >= sixMonthsAgo)
                .GroupBy(l => new { Year = l.LoanedAt.Year, Month = l.LoanedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            // Son 6 ayın tümünü ekle (veri yoksa 0)
            var allMonths = new List<string>();
            var allCounts = new List<int>();
            var monthNames = new[] { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", 
                                     "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };
            
            for (int i = 5; i >= 0; i--)
            {
                var date = DateTime.UtcNow.AddMonths(-i);
                var label = $"{monthNames[date.Month - 1]} {date.Year}";
                allMonths.Add(label);
                var existing = monthlyData.FirstOrDefault(x => x.Year == date.Year && x.Month == date.Month);
                allCounts.Add(existing?.Count ?? 0);
            }

            return new
            {
                labels = allMonths.ToArray(),
                data = allCounts.ToArray()
            };
        }

        /// <summary>
        /// Kullanıcının ödünç aldığı kitapların kategori dağılımını döndürür.
        /// </summary>
        private async Task<object> GetUserCategoryDistributionData(int memberId)
        {
            var categoryData = await _context.Loans
                .Where(l => l.MemberId == memberId)
                .Include(l => l.Copy)
                .ThenInclude(c => c.Book)
                .Where(l => l.Copy != null && l.Copy.Book != null && !string.IsNullOrEmpty(l.Copy.Book.Category))
                .GroupBy(l => l.Copy.Book.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            // Eğer veri yoksa boş dizi döndür
            if (!categoryData.Any())
            {
                return new
                {
                    labels = new string[0],
                    data = new int[0]
                };
            }

            return new
            {
                labels = categoryData.Select(x => x.Category).ToArray(),
                data = categoryData.Select(x => x.Count).ToArray()
            };
        }
    }

    /// <summary>
    /// Hata sayfası için view model.
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public int StatusCode { get; set; }
        public string? ExceptionPath { get; set; }
        public string? ExceptionMessage { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string ErrorTitle => StatusCode switch
        {
            404 => "Sayfa Bulunamadı",
            403 => "Erişim Reddedildi",
            500 => "Sunucu Hatası",
            _ => "Bir Hata Oluştu"
        };
        public string ErrorDescription => StatusCode switch
        {
            404 => "Aradığınız sayfa bulunamadı. Lütfen URL'yi kontrol edin.",
            403 => "Bu sayfaya erişim yetkiniz bulunmamaktadır.",
            500 => "Sunucuda bir hata oluştu. Lütfen daha sonra tekrar deneyin.",
            _ => "Beklenmeyen bir hata oluştu. Lütfen tekrar deneyin."
        };
    }
}

