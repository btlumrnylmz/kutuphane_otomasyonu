using System.Linq;
using System.Threading.Tasks;
using System.IO;
using KutuphaneOtomasyonu.Data;
using KutuphaneOtomasyonu.Models;
using KutuphaneOtomasyonu.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace KutuphaneOtomasyonu.Controllers
{
    /// <summary>
    /// Kitap CRUD işlemlerini yöneten controller.
    /// </summary>
    public class BookController : Controller
    {
        private readonly LibraryContext _context;
        private readonly AuthService _authService;
        private readonly IWebHostEnvironment _env;

        public BookController(LibraryContext context, AuthService authService, IWebHostEnvironment env)
        {
            _context = context;
            _authService = authService;
            _env = env;
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
        /// Kitap listesi. Arama, filtreleme ve pagination destekler.
        /// </summary>
        public async Task<IActionResult> Index(string searchTerm, string category, string author, int page = 1, int pageSize = 20)
        {
            IQueryable<Book> query = _context.Books.AsNoTracking();
            
            // Arama terimi filtresi
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(b => 
                    b.Title.ToLower().Contains(searchTerm) || 
                    b.Author.ToLower().Contains(searchTerm) ||
                    b.Isbn.ToLower().Contains(searchTerm) ||
                    (b.Description != null && b.Description.ToLower().Contains(searchTerm)));
            }
            
            // Kategori filtresi
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(b => b.Category == category);
            }
            
            // Yazar filtresi
            if (!string.IsNullOrWhiteSpace(author))
            {
                query = query.Where(b => b.Author == author);
            }
            
            // Toplam kayıt sayısı
            var totalItems = await query.CountAsync();
            
            // Pagination parametreleri
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 5, 100); // Min 5, Max 100
            
            // Sayfalama
            var books = await query
                .OrderBy(b => b.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            // Kategorileri ve yazarları filtre için al
            var categories = await _context.Books
                .Select(b => b.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
            
            var authors = await _context.Books
                .Select(b => b.Author)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
            
            ViewBag.Categories = categories;
            ViewBag.Authors = authors;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedAuthor = author;
            
            // Pagination model
            var pagination = new PaginationViewModel
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                ActionName = "Index",
                ControllerName = "Book",
                QueryParameters = PaginationViewModel.GetQueryParameters(Request)
            };
            
            ViewBag.Pagination = pagination;
            
            // Kullanıcının favori kitaplarını al
            if (_authService.IsLoggedIn())
            {
                var currentUser = _authService.GetCurrentUser();
                if (currentUser != null)
                {
                    var favoriteBookIds = await _context.Favorites
                        .Where(f => f.UserId == currentUser.UserId)
                        .Select(f => f.BookId)
                        .ToListAsync();
                    
                    ViewBag.FavoriteBookIds = favoriteBookIds;
                }
            }
            
            return View(books);
        }

        /// <summary>
        /// Kitap detayları.
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var book = await _context.Books
                .Include(b => b.Copies)
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null) return NotFound();
            
            // Kullanıcının bu kitabı favoriye ekleyip eklemediğini kontrol et
            if (_authService.IsLoggedIn())
            {
                var currentUser = _authService.GetCurrentUser();
                if (currentUser != null)
                {
                    var isFavorite = await _context.Favorites
                        .AnyAsync(f => f.UserId == currentUser.UserId && f.BookId == id);
                    ViewBag.IsFavorite = isFavorite;
                }
            }
            
            return View(book);
        }

        /// <summary>
        /// Yeni kitap formu.
        /// </summary>
        public IActionResult Create()
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            return View();
        }

        /// <summary>
        /// Yeni kitap oluşturma post işlemi. Dosya yükleme destekler.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,Isbn,Title,Author,PublishYear,Category,Description,PageCount")] Book book, IFormFile? coverImage)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (ModelState.IsValid)
            {
                try
                {
                    // Dosya yükleme işlemi
                    if (coverImage != null && coverImage.Length > 0)
                    {
                        // Dosya boyutu kontrolü (5MB)
                        if (coverImage.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("", "Dosya boyutu 5MB'dan büyük olamaz.");
                            return View(book);
                        }

                        // Dosya uzantısı kontrolü
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                        var fileExtension = Path.GetExtension(coverImage.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("", "Sadece JPG, JPEG ve PNG dosyaları yüklenebilir.");
                            return View(book);
                        }

                        // Dosya adını oluştur (ISBN veya benzersiz ID kullan)
                        var fileName = !string.IsNullOrWhiteSpace(book.Isbn) 
                            ? $"{book.Isbn.Replace(" ", "_").Replace("-", "_")}{fileExtension}"
                            : $"{Guid.NewGuid()}{fileExtension}";

                        // wwwroot/img/books/ klasörünü oluştur
                        var uploadPath = Path.Combine(_env.WebRootPath, "img", "books");
                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        // Dosya yolunu oluştur
                        var filePath = Path.Combine(uploadPath, fileName);

                        // Dosyayı kaydet
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await coverImage.CopyToAsync(stream);
                        }

                        // CoverImageUrl'yi ayarla
                        book.CoverImageUrl = $"/img/books/{fileName}";
                    }

                    _context.Add(book);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Kitap başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Books_Isbn") == true)
                {
                    ModelState.AddModelError("Isbn", "Bu ISBN numarası zaten kullanılmaktadır. Lütfen farklı bir ISBN girin.");
                    return View(book);
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    ModelState.AddModelError("Isbn", "Bu ISBN numarası zaten kayıtlıdır. Lütfen farklı bir ISBN girin.");
                    return View(book);
                }
            }
            return View(book);
        }

        /// <summary>
        /// Kitap düzenleme formu.
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id == null) return NotFound();
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();
            return View(book);
        }

        /// <summary>
        /// Kitap düzenleme post işlemi. Dosya yükleme destekler.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookId,Isbn,Title,Author,PublishYear,Category,Description,PageCount,CoverImageUrl")] Book book, IFormFile? coverImage)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id != book.BookId) return NotFound();
            if (!ModelState.IsValid) return View(book);

            try
            {
                // Dosya yükleme işlemi (yeni dosya yüklenirse)
                if (coverImage != null && coverImage.Length > 0)
                {
                    // Dosya boyutu kontrolü (5MB)
                    if (coverImage.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "Dosya boyutu 5MB'dan büyük olamaz.");
                        return View(book);
                    }

                    // Dosya uzantısı kontrolü
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(coverImage.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("", "Sadece JPG, JPEG ve PNG dosyaları yüklenebilir.");
                        return View(book);
                    }

                    // Eski resmi sil (varsa ve yeni bir resim yükleniyorsa)
                    if (!string.IsNullOrWhiteSpace(book.CoverImageUrl) && book.CoverImageUrl.StartsWith("/img/books/"))
                    {
                        var oldFilePath = Path.Combine(_env.WebRootPath, book.CoverImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            try
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                            catch
                            {
                                // Eski dosya silinemezse devam et
                            }
                        }
                    }

                    // Dosya adını oluştur (ISBN veya benzersiz ID kullan)
                    var fileName = !string.IsNullOrWhiteSpace(book.Isbn) 
                        ? $"{book.Isbn.Replace(" ", "_").Replace("-", "_")}{fileExtension}"
                        : $"{Guid.NewGuid()}{fileExtension}";

                    // wwwroot/img/books/ klasörünü oluştur
                    var uploadPath = Path.Combine(_env.WebRootPath, "img", "books");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    // Dosya yolunu oluştur
                    var filePath = Path.Combine(uploadPath, fileName);

                    // Dosyayı kaydet
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await coverImage.CopyToAsync(stream);
                    }

                    // CoverImageUrl'yi ayarla
                    book.CoverImageUrl = $"/img/books/{fileName}";
                }

                _context.Update(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kitap başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Books_Isbn") == true)
            {
                ModelState.AddModelError("Isbn", "Bu ISBN numarası zaten kullanılmaktadır. Lütfen farklı bir ISBN girin.");
                return View(book);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
            {
                ModelState.AddModelError("Isbn", "Bu ISBN numarası zaten kayıtlıdır. Lütfen farklı bir ISBN girin.");
                return View(book);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Books.Any(e => e.BookId == id)) return NotFound();
                throw;
            }
        }

        /// <summary>
        /// Silme onay sayfası.
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            if (id == null) return NotFound();
            var book = await _context.Books.AsNoTracking().FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null) return NotFound();
            return View(book);
        }

        /// <summary>
        /// Silme post işlemi.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var adminCheck = CheckAdminAccess();
            if (adminCheck != null) return adminCheck;

            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kitap başarıyla silindi.";
            }
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Kitabı favorilere ekler.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToFavorites(int bookId)
        {
            if (!_authService.IsLoggedIn())
            {
                TempData["Error"] = "Favorilere eklemek için giriş yapmalısınız.";
                return RedirectToAction("Login", "Auth");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
                return RedirectToAction(nameof(Details), new { id = bookId });
            }

            // Kitap var mı kontrol et
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                TempData["Error"] = "Kitap bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Zaten favoriye eklenmiş mi kontrol et
            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == currentUser.UserId && f.BookId == bookId);

            if (existingFavorite != null)
            {
                TempData["Info"] = "Bu kitap zaten favorilerinizde.";
                return RedirectToAction(nameof(Details), new { id = bookId });
            }

            var favorite = new Favorite
            {
                UserId = currentUser.UserId,
                BookId = bookId,
                AddedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{book.Title}' favorilerinize eklendi.";
            return RedirectToAction(nameof(Details), new { id = bookId });
        }

        /// <summary>
        /// Kitabı favorilerden çıkarır.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromFavorites(int bookId)
        {
            if (!_authService.IsLoggedIn())
            {
                TempData["Error"] = "Favorilerden çıkarmak için giriş yapmalısınız.";
                return RedirectToAction("Login", "Auth");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
                return RedirectToAction(nameof(Details), new { id = bookId });
            }

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == currentUser.UserId && f.BookId == bookId);

            if (favorite == null)
            {
                TempData["Info"] = "Bu kitap favorilerinizde değil.";
                return RedirectToAction(nameof(Details), new { id = bookId });
            }

            var bookTitle = favorite.Book?.Title ?? "Kitap";
            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"'{bookTitle}' favorilerinizden çıkarıldı.";
            return RedirectToAction(nameof(Details), new { id = bookId });
        }


        /// <summary>
        /// Kullanıcının favori kitapları listesi.
        /// </summary>
        public async Task<IActionResult> Favorites()
        {
            if (!_authService.IsLoggedIn())
            {
                TempData["Error"] = "Favorilerinizi görmek için giriş yapmalısınız.";
                return RedirectToAction("Login", "Auth");
            }

            var currentUser = _authService.GetCurrentUser();
            if (currentUser == null)
            {
                TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
                return RedirectToAction("Index", "Home");
            }

            var favorites = await _context.Favorites
                .Include(f => f.Book)
                .Where(f => f.UserId == currentUser.UserId)
                .OrderByDescending(f => f.AddedAt)
                .ToListAsync();

            return View(favorites);
        }
    }
}


