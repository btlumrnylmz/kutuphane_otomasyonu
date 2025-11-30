using System;
using System.Linq;
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
    }
}


