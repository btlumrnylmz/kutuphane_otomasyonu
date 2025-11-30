using KutuphaneOtomasyonu.Data;
using KutuphaneOtomasyonu.Models;
using KutuphaneOtomasyonu.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text;

namespace KutuphaneOtomasyonu.Controllers
{
    /// <summary>
    /// Veri export/import işlemlerini yöneten controller.
    /// </summary>
    public class ExportController : Controller
    {
        private readonly LibraryContext _context;
        private readonly AuthService _authService;

        public ExportController(LibraryContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
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
        /// Kitapları Excel formatında export eder.
        /// </summary>
        public async Task<IActionResult> ExportBooksExcel()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Kitaplar");

            // Başlıklar
            worksheet.Cells[1, 1].Value = "ISBN";
            worksheet.Cells[1, 2].Value = "Başlık";
            worksheet.Cells[1, 3].Value = "Yazar";
            worksheet.Cells[1, 4].Value = "Kategori";
            worksheet.Cells[1, 5].Value = "Yayın Yılı";
            worksheet.Cells[1, 6].Value = "Sayfa Sayısı";
            worksheet.Cells[1, 7].Value = "Açıklama";

            // Başlık stili
            using (var range = worksheet.Cells[1, 1, 1, 7])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            // Veriler
            var books = await _context.Books.AsNoTracking().OrderBy(b => b.Title).ToListAsync();
            for (int i = 0; i < books.Count; i++)
            {
                var row = i + 2;
                worksheet.Cells[row, 1].Value = books[i].Isbn;
                worksheet.Cells[row, 2].Value = books[i].Title;
                worksheet.Cells[row, 3].Value = books[i].Author;
                worksheet.Cells[row, 4].Value = books[i].Category;
                worksheet.Cells[row, 5].Value = books[i].PublishYear;
                worksheet.Cells[row, 6].Value = books[i].PageCount;
                worksheet.Cells[row, 7].Value = books[i].Description;
            }

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Kitaplar_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Kitapları CSV formatında export eder.
        /// </summary>
        public async Task<IActionResult> ExportBooksCsv()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            var books = await _context.Books.AsNoTracking().OrderBy(b => b.Title).ToListAsync();
            
            var sb = new StringBuilder();
            sb.AppendLine("ISBN,Başlık,Yazar,Kategori,Yayın Yılı,Sayfa Sayısı,Açıklama");

            foreach (var book in books)
            {
                sb.AppendLine($"{EscapeCsv(book.Isbn)},{EscapeCsv(book.Title)},{EscapeCsv(book.Author)}," +
                    $"{EscapeCsv(book.Category)},{book.PublishYear},{book.PageCount ?? 0},{EscapeCsv(book.Description ?? "")}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"Kitaplar_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }

        /// <summary>
        /// Üyeleri Excel formatında export eder.
        /// </summary>
        public async Task<IActionResult> ExportMembersExcel()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Üyeler");

            worksheet.Cells[1, 1].Value = "Ad Soyad";
            worksheet.Cells[1, 2].Value = "E-posta";
            worksheet.Cells[1, 3].Value = "Telefon";
            worksheet.Cells[1, 4].Value = "Üyelik Tarihi";
            worksheet.Cells[1, 5].Value = "Durum";

            using (var range = worksheet.Cells[1, 1, 1, 5])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            var members = await _context.Members.AsNoTracking().OrderBy(m => m.FullName).ToListAsync();
            for (int i = 0; i < members.Count; i++)
            {
                var row = i + 2;
                worksheet.Cells[row, 1].Value = members[i].FullName;
                worksheet.Cells[row, 2].Value = members[i].Email;
                worksheet.Cells[row, 3].Value = members[i].Phone;
                worksheet.Cells[row, 4].Value = members[i].JoinedAt.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 5].Value = members[i].Status.ToString();
            }

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"Uyeler_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Ödünç kayıtlarını Excel formatında export eder.
        /// </summary>
        public async Task<IActionResult> ExportLoansExcel()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Ödünçler");

            worksheet.Cells[1, 1].Value = "Üye";
            worksheet.Cells[1, 2].Value = "Kitap";
            worksheet.Cells[1, 3].Value = "Kopya";
            worksheet.Cells[1, 4].Value = "Ödünç Tarihi";
            worksheet.Cells[1, 5].Value = "Vade Tarihi";
            worksheet.Cells[1, 6].Value = "İade Tarihi";
            worksheet.Cells[1, 7].Value = "Durum";

            using (var range = worksheet.Cells[1, 1, 1, 7])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            var loans = await _context.Loans
                .Include(l => l.Member)
                .Include(l => l.Copy).ThenInclude(c => c.Book)
                .AsNoTracking()
                .OrderByDescending(l => l.LoanedAt)
                .ToListAsync();

            for (int i = 0; i < loans.Count; i++)
            {
                var row = i + 2;
                worksheet.Cells[row, 1].Value = loans[i].Member.FullName;
                worksheet.Cells[row, 2].Value = loans[i].Copy.Book.Title;
                worksheet.Cells[row, 3].Value = loans[i].Copy.ShelfLocation;
                worksheet.Cells[row, 4].Value = loans[i].LoanedAt.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cells[row, 5].Value = loans[i].DueAt.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 6].Value = loans[i].ReturnedAt?.ToString("yyyy-MM-dd HH:mm") ?? "";
                worksheet.Cells[row, 7].Value = loans[i].ReturnedAt == null ? "Aktif" : "İade Edildi";
            }

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"OduncKayitlari_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// CSV için özel karakterleri escape eder.
        /// </summary>
        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }
    }
}

