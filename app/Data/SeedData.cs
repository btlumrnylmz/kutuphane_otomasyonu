using System;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using KutuphaneOtomasyonu.Models;

namespace KutuphaneOtomasyonu.Data
{
    /// <summary>
    /// Başlangıç veri yükleyici. Uygulama başlangıcında çağrılarak örnek veriler eklenir.
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Veritabanını oluşturur ve örnek verilerle doldurur.
        /// Türkçe örnek veri seti içerir.
        /// </summary>
        public static void Initialize(LibraryContext context)
        {
            context.Database.EnsureCreated();

            // Yeni kitapları ekle (ISBN kontrolü ile)
            var booksToSeed = new List<Book>
                {
                    new Book 
                    { 
                        Isbn = "9786050401234", 
                        Title = "Kürk Mantolu Madonna", 
                        Author = "Sabahattin Ali", 
                        PublishYear = 1943, 
                        Category = "Roman",
                        Description = "Sabahattin Ali'nin en ünlü eserlerinden biri. Bir aşk hikayesi ve toplumsal eleştiri içeren bu roman, Türk edebiyatının klasiklerindendir.",
                        PageCount = 160,
                        CoverImageUrl = "https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=400&h=600&fit=crop"
                    },
                    new Book 
                    { 
                        Isbn = "9789754700114", 
                        Title = "Simyacı", 
                        Author = "Paulo Coelho", 
                        PublishYear = 1988, 
                        Category = "Kişisel Gelişim",
                        Description = "Bir çobanın kişisel efsanesini keşfetme yolculuğunu anlatan bu kitap, hayallerin peşinden gitmenin önemini vurgular.",
                        PageCount = 163,
                        CoverImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&h=600&fit=crop"
                    },
                    new Book 
                    { 
                        Isbn = "9789750808626", 
                        Title = "Beyaz Zambaklar Ülkesinde", 
                        Author = "Grigory Petrov", 
                        PublishYear = 1923, 
                        Category = "Deneme",
                        Description = "Finlandiya'nın kalkınma hikayesini anlatan bu eser, eğitim ve toplumsal gelişimin önemini vurgular.",
                        PageCount = 200,
                        CoverImageUrl = "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=400&h=600&fit=crop"
                    },
                    new Book 
                    { 
                        Isbn = "9789750719380", 
                        Title = "Suç ve Ceza", 
                        Author = "Fyodor Dostoyevski", 
                        PublishYear = 1866, 
                        Category = "Roman",
                        Description = "Dostoyevski'nin başyapıtı. Raskolnikov'un işlediği cinayet ve sonrasında yaşadığı vicdan azabını konu alır.",
                        PageCount = 671,
                        CoverImageUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400&h=600&fit=crop"
                    },
                    new Book 
                    { 
                        Isbn = "9789750807568", 
                        Title = "Savaş ve Barış", 
                        Author = "Lev Tolstoy", 
                        PublishYear = 1869, 
                        Category = "Roman",
                        Description = "Napolyon savaşları döneminde Rusya'da geçen bu epik roman, tarih, felsefe ve aşkı harmanlar.",
                        PageCount = 1225,
                        CoverImageUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400&h=600&fit=crop"
                    },
                    new Book 
                    { 
                        Isbn = "9789750808627", 
                        Title = "İnce Memed", 
                        Author = "Yaşar Kemal", 
                        PublishYear = 1955, 
                        Category = "Roman",
                        Description = "Toroslar'da geçen bu destansı roman, eşkıya İnce Memed'in hikayesini anlatır.",
                        PageCount = 436,
                        CoverImageUrl = "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=400&h=600&fit=crop"
                    },
                    new Book 
                    { 
                        Isbn = "9789750808628", 
                        Title = "1984", 
                        Author = "George Orwell", 
                        PublishYear = 1949, 
                        Category = "Distopya",
                        Description = "Totaliter bir gelecek tasviri. Büyük Birader'in gözetiminde yaşayan bir toplumun hikayesi.",
                        PageCount = 328,
                        CoverImageUrl = "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=400&h=600&fit=crop"
                    },
                    new Book 
                    { 
                        Isbn = "9789750808629", 
                        Title = "Hayvan Çiftliği", 
                        Author = "George Orwell", 
                        PublishYear = 1945, 
                        Category = "Distopya",
                        Description = "Bir çiftlikteki hayvanların devrim hikayesi. Politik bir alegori ve toplumsal eleştiri.",
                        PageCount = 112,
                        CoverImageUrl = "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=400&h=600&fit=crop"
                    },
                    new Book 
                    { 
                        Isbn = "9789750808630", 
                        Title = "Küçük Prens", 
                        Author = "Antoine de Saint-Exupéry", 
                        PublishYear = 1943, 
                        Category = "Çocuk Edebiyatı",
                        Description = "Bir çocuğun gözünden büyüklerin dünyasını anlatan bu felsefi hikaye, her yaştan okura hitap eder.",
                        PageCount = 96,
                        CoverImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&h=600&fit=crop"
                    },
                    new Book 
                    { 
                        Isbn = "9789750808631", 
                        Title = "Sefiller", 
                        Author = "Victor Hugo", 
                        PublishYear = 1862, 
                        Category = "Roman",
                        Description = "Jean Valjean'ın hayatını anlatan bu epik roman, adalet, merhamet ve toplumsal eşitsizlik temalarını işler.",
                        PageCount = 1463,
                        CoverImageUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400&h=600&fit=crop"
                    },
                    new Book 
                    { 
                        Isbn = "9789750808632", 
                        Title = "Anna Karenina", 
                        Author = "Lev Tolstoy", 
                        PublishYear = 1877, 
                        Category = "Roman",
                        Description = "Evli bir kadının aşkı uğruna toplumsal kuralları çiğnemesini anlatan bu klasik, Rus toplumunun detaylı bir portresini çizer.",
                        PageCount = 864,
                        CoverImageUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400&h=600&fit=crop"
                    },
                    new Book 
                    { 
                        Isbn = "9789750808633", 
                        Title = "Yabancı", 
                        Author = "Albert Camus", 
                        PublishYear = 1942, 
                        Category = "Felsefe",
                        Description = "Varoluşçu bir roman. Meursault'un toplumsal normlara yabancılaşmasını ve absürtlük felsefesini işler.",
                        PageCount = 123,
                        CoverImageUrl = "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=400&h=600&fit=crop"
                    },
                    new Book 
                    { 
                        Isbn = "9789750808634", 
                        Title = "Dönüşüm", 
                        Author = "Franz Kafka", 
                        PublishYear = 1915, 
                        Category = "Roman",
                        Description = "Gregor Samsa'nın bir sabah böceğe dönüşmesini anlatan bu absürt hikaye, modern edebiyatın başyapıtlarındandır.",
                        PageCount = 55,
                        CoverImageUrl = "https://images.unsplash.com/photo-1543002588-bfa74002ed7e?w=400&h=600&fit=crop"
                    },
                    new Book 
                    { 
                        Isbn = "9789750808635", 
                        Title = "Şeker Portakalı", 
                        Author = "José Mauro de Vasconcelos", 
                        PublishYear = 1968, 
                        Category = "Roman",
                        Description = "Brezilya'da geçen bu duygusal hikaye, bir çocuğun hayal gücü ve sevgi dolu dünyasını anlatır.",
                        PageCount = 192,
                        CoverImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&h=600&fit=crop"
                    },
                    new Book 
                    { 
                        Isbn = "9789750808636", 
                        Title = "Martı", 
                        Author = "Richard Bach", 
                        PublishYear = 1970, 
                        Category = "Kişisel Gelişim",
                        Description = "Bir martının uçma tutkusunu anlatan bu felsefi hikaye, kişisel gelişim ve özgürlük temalarını işler.",
                        PageCount = 96,
                        CoverImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400&h=600&fit=crop"
                    }
                };

                // Sadece veritabanında olmayan kitapları ekle (ISBN kontrolü ile)
                var existingIsbns = context.Books.Select(b => b.Isbn).ToList();
                var booksToAdd = booksToSeed.Where(b => !existingIsbns.Contains(b.Isbn)).ToList();
                
                if (booksToAdd.Any())
                {
                    context.Books.AddRange(booksToAdd);
                    context.SaveChanges();
                }

            // Kopyaları ekle (sadece veritabanında kopya yoksa)
            if (!context.Copies.Any())
            {
                // Veritabanından kitapları al (ISBN ile eşleştirerek)
                var allBooks = context.Books.ToList();
                var b1 = allBooks.FirstOrDefault(b => b.Isbn == "9786050401234");
                var b2 = allBooks.FirstOrDefault(b => b.Isbn == "9789754700114");
                var b3 = allBooks.FirstOrDefault(b => b.Isbn == "9789750808626");
                var b4 = allBooks.FirstOrDefault(b => b.Isbn == "9789750719380");
                var b5 = allBooks.FirstOrDefault(b => b.Isbn == "9789750807568");
                var b6 = allBooks.FirstOrDefault(b => b.Isbn == "9789750808627");
                var b7 = allBooks.FirstOrDefault(b => b.Isbn == "9789750808628");
                var b8 = allBooks.FirstOrDefault(b => b.Isbn == "9789750808629");
                var b9 = allBooks.FirstOrDefault(b => b.Isbn == "9789750808630");
                var b10 = allBooks.FirstOrDefault(b => b.Isbn == "9789750808631");

                    // Her kitap için en az 1-2 kopya ekle
                    var copies = new List<Copy>();
                    
                    if (b1 != null)
                    {
                        copies.Add(new Copy { BookId = b1.BookId, ShelfLocation = "A1-01", Status = CopyStatus.Available });
                        copies.Add(new Copy { BookId = b1.BookId, ShelfLocation = "A1-02", Status = CopyStatus.Loaned });
                    }
                    if (b2 != null)
                    {
                        copies.Add(new Copy { BookId = b2.BookId, ShelfLocation = "B2-05", Status = CopyStatus.Available });
                        copies.Add(new Copy { BookId = b2.BookId, ShelfLocation = "B2-06", Status = CopyStatus.Available });
                    }
                    if (b3 != null)
                    {
                        copies.Add(new Copy { BookId = b3.BookId, ShelfLocation = "C3-10", Status = CopyStatus.Loaned });
                        copies.Add(new Copy { BookId = b3.BookId, ShelfLocation = "C3-11", Status = CopyStatus.Available });
                    }
                    if (b4 != null)
                    {
                        copies.Add(new Copy { BookId = b4.BookId, ShelfLocation = "D4-01", Status = CopyStatus.Available });
                        copies.Add(new Copy { BookId = b4.BookId, ShelfLocation = "D4-02", Status = CopyStatus.Available });
                    }
                    if (b5 != null) copies.Add(new Copy { BookId = b5.BookId, ShelfLocation = "E5-01", Status = CopyStatus.Available });
                    if (b6 != null)
                    {
                        copies.Add(new Copy { BookId = b6.BookId, ShelfLocation = "F6-01", Status = CopyStatus.Available });
                        copies.Add(new Copy { BookId = b6.BookId, ShelfLocation = "F6-02", Status = CopyStatus.Available });
                    }
                    if (b7 != null) copies.Add(new Copy { BookId = b7.BookId, ShelfLocation = "G7-01", Status = CopyStatus.Available });
                    if (b8 != null) copies.Add(new Copy { BookId = b8.BookId, ShelfLocation = "H8-01", Status = CopyStatus.Available });
                    if (b9 != null)
                    {
                        copies.Add(new Copy { BookId = b9.BookId, ShelfLocation = "I9-01", Status = CopyStatus.Available });
                        copies.Add(new Copy { BookId = b9.BookId, ShelfLocation = "I9-02", Status = CopyStatus.Available });
                    }
                    if (b10 != null) copies.Add(new Copy { BookId = b10.BookId, ShelfLocation = "J10-01", Status = CopyStatus.Available });

                    if (copies.Any())
                    {
                        context.Copies.AddRange(copies);
                        context.SaveChanges();
                    }

                // Üyeleri ekle (sadece yoksa)
                if (!context.Members.Any())
                {
                    var u1 = new Member { FullName = "Ayşe Yılmaz", Email = "ayse.yilmaz@example.com", Phone = "+90 555 111 2233", JoinedAt = DateTime.UtcNow.AddDays(-20), Status = MemberStatus.Active };
                    var u2 = new Member { FullName = "Mehmet Demir", Email = "mehmet.demir@example.com", Phone = "+90 532 444 5566", JoinedAt = DateTime.UtcNow.AddDays(-60), Status = MemberStatus.Passive };

                    context.Members.AddRange(u1, u2);
                    context.SaveChanges();

                    // Ödünç kayıtları: loaned durumundaki kopyalara bağlayalım
                    var c2 = copies.FirstOrDefault(c => c.ShelfLocation == "A1-02");
                    var c5 = copies.FirstOrDefault(c => c.ShelfLocation == "C3-10");
                    
                    if (c2 != null)
                    {
                        var l1 = new Loan
                        {
                            CopyId = c2.CopyId,
                            MemberId = u1.MemberId,
                            LoanedAt = DateTime.UtcNow.AddDays(-3),
                            DueAt = DateTime.UtcNow.AddDays(11),
                            ReturnedAt = null
                        };
                        context.Loans.Add(l1);
                    }

                    if (c5 != null)
                    {
                        var l2 = new Loan
                        {
                            CopyId = c5.CopyId,
                            MemberId = u1.MemberId,
                            LoanedAt = DateTime.UtcNow.AddDays(-16),
                            DueAt = DateTime.UtcNow.AddDays(-2), // gecikmiş örnek
                            ReturnedAt = null
                        };
                        context.Loans.Add(l2);
                    }

                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// wwwroot/img/books/ klasöründeki resimlere göre kitaplar ekler.
        /// Resim dosya adlarından kitap bilgilerini çıkarır veya manuel eşleştirme yapar.
        /// </summary>
        public static void AddBooksFromImages(LibraryContext context, IWebHostEnvironment env)
        {
            var imagesPath = Path.Combine(env.WebRootPath, "img", "books");
            
            if (!Directory.Exists(imagesPath))
            {
                Directory.CreateDirectory(imagesPath);
                return;
            }

            var imageFiles = Directory.GetFiles(imagesPath)
                .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || 
                           f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || 
                           f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!imageFiles.Any())
            {
                return; // Resim yoksa çık
            }

            // Resim dosya adlarından kitap bilgilerini çıkar
            // Format: "kitap-adi-yazar-kategori.jpg" veya sadece "kitap-adi.jpg"
            var booksToAdd = new List<Book>();
            var existingIsbns = context.Books.Select(b => b.Isbn).ToList();
            var baseIsbn = 9789750809000; // Başlangıç ISBN

            foreach (var imagePath in imageFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(imagePath);
                var imageUrl = $"/img/books/{Path.GetFileName(imagePath)}";
                
                // Eğer bu resim zaten bir kitaba aitse, atla
                if (context.Books.Any(b => b.CoverImageUrl == imageUrl))
                {
                    continue;
                }

                // Dosya adından bilgi çıkar (örn: "kurk-mantolu-madonna-sabahattin-ali-roman")
                var parts = fileName.Split('-');
                
                // Basit bir eşleştirme: İlk kısım genelde kitap adı
                var title = string.Join(" ", parts.Take(Math.Min(parts.Length, 3)))
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower())
                    .ToArray();
                
                var bookTitle = string.Join(" ", title);
                if (string.IsNullOrWhiteSpace(bookTitle))
                {
                    bookTitle = "Bilinmeyen Kitap";
                }

                // Varsayılan değerler
                var author = parts.Length > 3 ? string.Join(" ", parts.Skip(3).Take(2)) : "Bilinmeyen Yazar";
                var category = parts.Length > 5 ? parts[5] : "Roman";
                
                // ISBN oluştur (benzersiz olması için)
                var isbn = (baseIsbn++).ToString();

                var book = new Book
                {
                    Isbn = isbn,
                    Title = bookTitle,
                    Author = author,
                    PublishYear = DateTime.Now.Year - new Random().Next(1, 50), // Son 50 yıl içinde rastgele
                    Category = category,
                    Description = $"{bookTitle} - {author} tarafından yazılmış {category} türünde bir eser.",
                    PageCount = new Random().Next(100, 500),
                    CoverImageUrl = imageUrl
                };

                if (!existingIsbns.Contains(book.Isbn))
                {
                    booksToAdd.Add(book);
                }
            }

            if (booksToAdd.Any())
            {
                context.Books.AddRange(booksToAdd);
                context.SaveChanges();

                // Her kitap için en az 1 kopya ekle
                foreach (var book in booksToAdd)
                {
                    var savedBook = context.Books.FirstOrDefault(b => b.Isbn == book.Isbn);
                    if (savedBook != null && !context.Copies.Any(c => c.BookId == savedBook.BookId))
                    {
                        var copy = new Copy
                        {
                            BookId = savedBook.BookId,
                            ShelfLocation = $"A{new Random().Next(1, 10)}-{new Random().Next(1, 20):D2}",
                            Status = CopyStatus.Available,
                            CopyNumber = 1
                        };
                        context.Copies.Add(copy);
                    }
                }
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Yüklenen resimlere göre kitapları ekler (kullanıcı resimleri yükledikten sonra çağrılır).
        /// </summary>
        public static void AddUploadedBooksFromImages(LibraryContext context, IWebHostEnvironment env)
        {
            var mapping = new Dictionary<string, BookInfo>
            {
                { "allah-icin-sevmek-702793-12151633-70-K.jpg", new BookInfo {
                    Title = "Allah İçin Sevmek",
                    Author = "Osman Nuri Topbaş",
                    Category = "Dini",
                    PublishYear = 2012,
                    PageCount = 224,
                    Description = "Allah'ı ve O'nun rızasını kazanmak için sevmenin önemini anlatan dini bir eser.",
                    Isbn = "9789752631234"
                }},
                { "babalar-ve-ogullar.jpg", new BookInfo {
                    Title = "Babalar ve Oğullar",
                    Author = "Ivan Turgenev",
                    Category = "Roman",
                    PublishYear = 1862,
                    PageCount = 256,
                    Description = "Rus edebiyatının klasik eserlerinden biri. Nesiller arası çatışmayı ve toplumsal değişimi konu alır.",
                    Isbn = "9789750712345"
                }},
                { "bir-idam-mahkumunun-son-gunu-57569-12243410-57-K.jpg", new BookInfo {
                    Title = "Bir İdam Mahkumunun Son Günü",
                    Author = "Victor Hugo",
                    Category = "Roman",
                    PublishYear = 1829,
                    PageCount = 128,
                    Description = "İdam cezasına çarptırılmış bir mahkumun son gününde yaşadığı duyguları ve düşünceleri anlatan duygusal bir eser.",
                    Isbn = "9789750812345"
                }},
                { "bogurtlen-kisi-860400-13489405-86-K.jpg", new BookInfo {
                    Title = "Böğürtlen Kışı",
                    Author = "Sarah Jio",
                    Category = "Roman",
                    PublishYear = 2011,
                    PageCount = 384,
                    Description = "Bir ada kasabasında geçen gizemli ve romantik bir hikaye.",
                    Isbn = "9789750812346"
                }},
                { "dil-belasi-317944-12541550-31-K.jpg", new BookInfo {
                    Title = "Dil Belası",
                    Author = "Ayşe Kulin",
                    Category = "Roman",
                    PublishYear = 2018,
                    PageCount = 304,
                    Description = "Ayşe Kulin'in kaleminden güçlü bir kadın karakterin hayatı ve mücadelesi.",
                    Isbn = "9789750812347"
                }},
                { "engeregin-gozu-512308-9499558-51-K.jpg", new BookInfo {
                    Title = "Engereğin Gözü",
                    Author = "Alev Alatlı",
                    Category = "Roman",
                    PublishYear = 1984,
                    PageCount = 384,
                    Description = "Türk edebiyatının önemli yazarlarından Alev Alatlı'nın ilk romanı.",
                    Isbn = "9789750812348"
                }},
                { "fahrenheit-289854-13887542-28-K.jpg", new BookInfo {
                    Title = "Fahrenheit 451",
                    Author = "Ray Bradbury",
                    Category = "Distopya",
                    PublishYear = 1953,
                    PageCount = 256,
                    Description = "Kitapların yakıldığı ve düşüncenin yasaklandığı distopik bir gelecek tasviri.",
                    Isbn = "9789750812349"
                }},
                { "gazap-uzumleri-13865964-56-K.jpg", new BookInfo {
                    Title = "Gazap Üzümleri",
                    Author = "John Steinbeck",
                    Category = "Roman",
                    PublishYear = 1939,
                    PageCount = 544,
                    Description = "Büyük Buhran döneminde yaşanan göç ve yoksulluk hikayesi.",
                    Isbn = "9789750812350"
                }},
                { "harry-potter-ve-ates-kadehi-4-13467938-55-K.jpg", new BookInfo {
                    Title = "Harry Potter ve Ateş Kadehi",
                    Author = "J.K. Rowling",
                    Category = "Fantastik",
                    PublishYear = 2000,
                    PageCount = 672,
                    Description = "Harry Potter serisinin dördüncü kitabı. Üçbüyücü Turnuvası'nın hikayesi.",
                    Isbn = "9789750812351"
                }},
                { "harry-potter-ve-azkaban-tutsagi-3-13467960-59-K.jpg", new BookInfo {
                    Title = "Harry Potter ve Azkaban Tutsağı",
                    Author = "J.K. Rowling",
                    Category = "Fantastik",
                    PublishYear = 1999,
                    PageCount = 464,
                    Description = "Harry Potter serisinin üçüncü kitabı. Sirius Black'in kaçışı ve gerçek hikayesi.",
                    Isbn = "9789750812352"
                }},
                { "harry-potter-ve-felsefe-tasi-1-11605137-55-K.jpg", new BookInfo {
                    Title = "Harry Potter ve Felsefe Taşı",
                    Author = "J.K. Rowling",
                    Category = "Fantastik",
                    PublishYear = 1997,
                    PageCount = 320,
                    Description = "Harry Potter serisinin ilk kitabı. Harry'nin büyülü dünyaya adım atışı.",
                    Isbn = "9789750812353"
                }},
                { "harry-potter-ve-melez-prens-6-11607143-55-K.jpg", new BookInfo {
                    Title = "Harry Potter ve Melez Prens",
                    Author = "J.K. Rowling",
                    Category = "Fantastik",
                    PublishYear = 2005,
                    PageCount = 608,
                    Description = "Harry Potter serisinin altıncı kitabı. Voldemort'un geçmişinin keşfedilmesi.",
                    Isbn = "9789750812354"
                }},
                { "harry-potter-ve-olum-yadigarlari-7-11606871-55-K.jpg", new BookInfo {
                    Title = "Harry Potter ve Ölüm Yadigarları",
                    Author = "J.K. Rowling",
                    Category = "Fantastik",
                    PublishYear = 2007,
                    PageCount = 784,
                    Description = "Harry Potter serisinin yedinci ve son kitabı. Voldemort ile nihai savaş.",
                    Isbn = "9789750812355"
                }},
                { "harry-potter-ve-sirlar-odasi-2-13467935-59-K.jpg", new BookInfo {
                    Title = "Harry Potter ve Sırlar Odası",
                    Author = "J.K. Rowling",
                    Category = "Fantastik",
                    PublishYear = 1998,
                    PageCount = 368,
                    Description = "Harry Potter serisinin ikinci kitabı. Hogwarts'ta gizemli saldırılar.",
                    Isbn = "9789750812356"
                }},
                { "harry-potter-ve-zumruduanka-yoldasligi-5-11546143-55-K.jpg", new BookInfo {
                    Title = "Harry Potter ve Zümrüdüanka Yoldaşlığı",
                    Author = "J.K. Rowling",
                    Category = "Fantastik",
                    PublishYear = 2003,
                    PageCount = 896,
                    Description = "Harry Potter serisinin beşinci kitabı. Zümrüdüanka Yoldaşlığı'nın kuruluşu.",
                    Isbn = "9789750812357"
                }},
                { "kalplerin-kesfi-245420-11567894-24-K.jpg", new BookInfo {
                    Title = "Kalplerin Keşfi",
                    Author = "İmam Gazali",
                    Category = "Dini",
                    PublishYear = 1106,
                    PageCount = 456,
                    Description = "İslam tasavvufunun önemli eserlerinden biri. Kalp ve ruh temizliğini konu alır.",
                    Isbn = "9789750812358"
                }},
                { "mart-menekseleri-868275-13859485-86-K.jpg", new BookInfo {
                    Title = "Mart Menekseleri",
                    Author = "Ayşe Kulin",
                    Category = "Roman",
                    PublishYear = 2000,
                    PageCount = 320,
                    Description = "Ayşe Kulin'in kaleminden modern Türkiye'nin bir ailesinin hikayesi.",
                    Isbn = "9789750812359"
                }},
                { "mutlu-olum-704651-13999934-70-K.jpg", new BookInfo {
                    Title = "Mutlu Ölüm",
                    Author = "Albert Camus",
                    Category = "Felsefe",
                    PublishYear = 1971,
                    PageCount = 192,
                    Description = "Camus'nun ölümünden sonra yayımlanan romanı. Absürtlük felsefesinin izlerini taşır.",
                    Isbn = "9789750812360"
                }},
                { "Oz-Terapinin-Getirdikleri_1.jpg", new BookInfo {
                    Title = "Öz Terapinin Getirdikleri",
                    Author = "Doğan Cüceloğlu",
                    Category = "Kişisel Gelişim",
                    PublishYear = 2012,
                    PageCount = 288,
                    Description = "Kişisel gelişim ve öz farkındalık üzerine yazılmış önemli bir eser.",
                    Isbn = "9789750812361"
                }},
                { "yasli-amca-ve-deniz-13882646-88-K.jpg", new BookInfo {
                    Title = "Yaşlı Adam ve Deniz",
                    Author = "Ernest Hemingway",
                    Category = "Roman",
                    PublishYear = 1952,
                    PageCount = 128,
                    Description = "Nobel ödüllü eser. Yaşlı bir balıkçının büyük bir balıkla mücadelesi.",
                    Isbn = "9789750812362"
                }}
            };

            AddBooksFromImageMapping(context, env, mapping);
        }

        /// <summary>
        /// Manuel olarak resim dosya adlarına göre kitaplar ekler.
        /// Resim dosya adlarını ve kitap bilgilerini eşleştirir.
        /// </summary>
        public static void AddBooksFromImageMapping(LibraryContext context, IWebHostEnvironment env, Dictionary<string, BookInfo> imageBookMapping)
        {
            var imagesPath = Path.Combine(env.WebRootPath, "img", "books");
            
            if (!Directory.Exists(imagesPath))
            {
                return;
            }

            var existingIsbns = context.Books.Select(b => b.Isbn).ToList();
            var booksToAdd = new List<Book>();
            var baseIsbn = 9789750809000;

            foreach (var mapping in imageBookMapping)
            {
                var imageFileName = mapping.Key;
                var bookInfo = mapping.Value;
                var imagePath = Path.Combine(imagesPath, imageFileName);

                if (!File.Exists(imagePath))
                {
                    continue; // Dosya yoksa atla
                }

                var imageUrl = $"/img/books/{imageFileName}";
                
                // Eğer bu resim zaten bir kitaba aitse, atla
                if (context.Books.Any(b => b.CoverImageUrl == imageUrl))
                {
                    continue;
                }

                var isbn = bookInfo.Isbn ?? (baseIsbn++).ToString();

                if (existingIsbns.Contains(isbn))
                {
                    continue; // ISBN zaten varsa atla
                }

                var book = new Book
                {
                    Isbn = isbn,
                    Title = bookInfo.Title,
                    Author = bookInfo.Author,
                    PublishYear = bookInfo.PublishYear ?? DateTime.Now.Year - new Random().Next(1, 50),
                    Category = bookInfo.Category ?? "Roman",
                    Description = bookInfo.Description ?? $"{bookInfo.Title} - {bookInfo.Author} tarafından yazılmış bir eser.",
                    PageCount = bookInfo.PageCount,
                    CoverImageUrl = imageUrl
                };

                booksToAdd.Add(book);
            }

            if (booksToAdd.Any())
            {
                context.Books.AddRange(booksToAdd);
                context.SaveChanges();

                // Her kitap için en az 1 kopya ekle
                foreach (var book in booksToAdd)
                {
                    var savedBook = context.Books.FirstOrDefault(b => b.Isbn == book.Isbn);
                    if (savedBook != null && !context.Copies.Any(c => c.BookId == savedBook.BookId))
                    {
                        var copy = new Copy
                        {
                            BookId = savedBook.BookId,
                            ShelfLocation = $"A{new Random().Next(1, 10)}-{new Random().Next(1, 20):D2}",
                            Status = CopyStatus.Available,
                            CopyNumber = 1
                        };
                        context.Copies.Add(copy);
                    }
                }
                context.SaveChanges();
            }
        }
    }

    /// <summary>
    /// Kitap bilgilerini tutan yardımcı sınıf.
    /// </summary>
    public class BookInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int? PublishYear { get; set; }
        public int? PageCount { get; set; }
        public string? Description { get; set; }
        public string? Isbn { get; set; }
    }
}


