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

            // ISBN ve Email benzersizliği zaten indekslerle güvence altında.
            if (!context.Books.Any())
            {
                var b1 = new Book { Isbn = "9786050401234", Title = "Kürk Mantolu Madonna", Author = "Sabahattin Ali", PublishYear = 1943, Category = "Roman" };
                var b2 = new Book { Isbn = "9789754700114", Title = "Simyacı", Author = "Paulo Coelho", PublishYear = 1988, Category = "Kişisel Gelişim" };
                var b3 = new Book { Isbn = "9789750808626", Title = "Beyaz Zambaklar Ülkesinde", Author = "Grigory Petrov", PublishYear = 1913, Category = "Deneme" };

                context.Books.AddRange(b1, b2, b3);
                context.SaveChanges();

                // 5 kopya: bazıları loaned, bazıları available
                var c1 = new Copy { BookId = b1.BookId, ShelfLocation = "A1-01", Status = CopyStatus.Available };
                var c2 = new Copy { BookId = b1.BookId, ShelfLocation = "A1-02", Status = CopyStatus.Loaned };
                var c3 = new Copy { BookId = b2.BookId, ShelfLocation = "B2-05", Status = CopyStatus.Available };
                var c4 = new Copy { BookId = b2.BookId, ShelfLocation = "B2-06", Status = CopyStatus.Damaged };
                var c5 = new Copy { BookId = b3.BookId, ShelfLocation = "C3-10", Status = CopyStatus.Loaned };

                context.Copies.AddRange(c1, c2, c3, c4, c5);
                context.SaveChanges();

                // 2 üye: 1 aktif, 1 pasif
                var u1 = new Member { FullName = "Ayşe Yılmaz", Email = "ayse.yilmaz@example.com", Phone = "+90 555 111 2233", JoinedAt = DateTime.UtcNow.AddDays(-20), Status = MemberStatus.Active };
                var u2 = new Member { FullName = "Mehmet Demir", Email = "mehmet.demir@example.com", Phone = "+90 532 444 5566", JoinedAt = DateTime.UtcNow.AddDays(-60), Status = MemberStatus.Passive };

                context.Members.AddRange(u1, u2);
                context.SaveChanges();

                // Ödünç kayıtları: loaned durumundaki kopyalara bağlayalım
                var l1 = new Loan
                {
                    CopyId = c2.CopyId,
                    MemberId = u1.MemberId,
                    LoanedAt = DateTime.UtcNow.AddDays(-3),
                    DueAt = DateTime.UtcNow.AddDays(11),
                    ReturnedAt = null
                };

                var l2 = new Loan
                {
                    CopyId = c5.CopyId,
                    MemberId = u1.MemberId,
                    LoanedAt = DateTime.UtcNow.AddDays(-16),
                    DueAt = DateTime.UtcNow.AddDays(-2), // gecikmiş örnek
                    ReturnedAt = null
                };

                context.Loans.AddRange(l1, l2);
                context.SaveChanges();
            }
        }
    }
}


