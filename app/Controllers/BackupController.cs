using System;
using System.IO;
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
    /// Veritabanı yedekleme ve geri yükleme işlemlerini yöneten controller.
    /// PDF gereksinimi: N5 - Yedekleme/geri yükleme kısa senaryosu
    /// </summary>
    public class BackupController : Controller
    {
        private readonly LibraryContext _context;
        private readonly AuthService _authService;
        private readonly ILogger<BackupController> _logger;

        public BackupController(LibraryContext context, AuthService authService, ILogger<BackupController> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
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

        /// <summary>
        /// Yedekleme sayfası.
        /// </summary>
        public IActionResult Index()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            return View();
        }

        /// <summary>
        /// Veritabanı yedekleme işlemi (SQL script olarak export).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Backup()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            try
            {
                var backupData = new
                {
                    Timestamp = DateTime.UtcNow,
                    Books = await _context.Books.ToListAsync(),
                    Copies = await _context.Copies.ToListAsync(),
                    Members = await _context.Members.ToListAsync(),
                    Loans = await _context.Loans.ToListAsync(),
                    Users = await _context.Users.ToListAsync(),
                    Payments = await _context.Payments.ToListAsync(),
                    ReturnRequests = await _context.ReturnRequests.ToListAsync(),
                    Favorites = await _context.Favorites.ToListAsync(),
                    Reservations = await _context.Set<Reservation>().ToListAsync()
                };

                var backupJson = System.Text.Json.JsonSerializer.Serialize(backupData, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                var fileName = $"KutuphaneOtomasyonu_Backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
                var bytes = System.Text.Encoding.UTF8.GetBytes(backupJson);

                _logger.LogInformation("Veritabanı yedekleme işlemi başlatıldı: {FileName}", fileName);

                return File(bytes, "application/json", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yedekleme işlemi sırasında hata oluştu");
                TempData["Error"] = $"Yedekleme işlemi başarısız: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// SQL Server yedekleme (BAK dosyası) - SQL Server Management Studio komutu.
        /// </summary>
        public IActionResult SqlBackup()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            // SQL Server backup komutu
            var connectionString = _context.Database.GetConnectionString();
            var databaseName = "KutuphaneOtomasyonu";
            var backupPath = $@"C:\Backups\{databaseName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak";

            var sqlCommand = $@"
-- SQL Server Yedekleme Komutu
-- SSMS'de çalıştırın veya sqlcmd ile:
-- sqlcmd -S DESKTOP-D00J96T\SQLEXPRESS -E -Q ""BACKUP DATABASE [{databaseName}] TO DISK = '{backupPath}' WITH FORMAT, INIT, NAME = 'Full Backup of {databaseName}', SKIP, NOREWIND, NOUNLOAD, STATS = 10""

BACKUP DATABASE [{databaseName}]
TO DISK = '{backupPath}'
WITH FORMAT, INIT, 
     NAME = 'Full Backup of {databaseName}', 
     SKIP, NOREWIND, NOUNLOAD, 
     STATS = 10;
";

            ViewBag.SqlCommand = sqlCommand;
            ViewBag.BackupPath = backupPath;
            return View();
        }

        /// <summary>
        /// SQL Server geri yükleme komutu gösterir.
        /// </summary>
        public IActionResult Restore()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            return View();
        }

        /// <summary>
        /// Geri yükleme komutu oluşturur.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerateRestoreCommand(string backupFilePath)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (string.IsNullOrWhiteSpace(backupFilePath))
            {
                TempData["Error"] = "Yedek dosya yolu belirtilmelidir.";
                return RedirectToAction(nameof(Restore));
            }

            var databaseName = "KutuphaneOtomasyonu";
            var sqlCommand = $@"
-- SQL Server Geri Yükleme Komutu
-- ÖNEMLİ: Geri yükleme öncesi mevcut veritabanını yedekleyin!
-- SSMS'de çalıştırın:

USE master;
GO

-- Mevcut bağlantıları kapat
ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- Geri yükleme
RESTORE DATABASE [{databaseName}]
FROM DISK = '{backupFilePath}'
WITH REPLACE, RECOVERY,
     MOVE '{databaseName}' TO 'C:\Program Files\Microsoft SQL Server\MSSQL15.SQLEXPRESS\MSSQL\DATA\{databaseName}.mdf',
     MOVE '{databaseName}_Log' TO 'C:\Program Files\Microsoft SQL Server\MSSQL15.SQLEXPRESS\MSSQL\DATA\{databaseName}_Log.ldf';
GO

-- Çoklu kullanıcı moduna dön
ALTER DATABASE [{databaseName}] SET MULTI_USER;
GO
";

            ViewBag.SqlCommand = sqlCommand;
            ViewBag.BackupFilePath = backupFilePath;
            return View("Restore");
        }
    }
}

