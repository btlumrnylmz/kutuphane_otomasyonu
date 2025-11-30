using System;
using KutuphaneOtomasyonu.Models;
using KutuphaneOtomasyonu.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KutuphaneOtomasyonu.Data
{
    /// <summary>
    /// Uygulamanın Entity Framework Core DbContext'i. İlişki ve kısıtların tanımını içerir.
    /// </summary>
    public class LibraryContext : DbContext
    {
        public DbSet<Book> Books => Set<Book>();
        public DbSet<Copy> Copies => Set<Copy>();
        public DbSet<Member> Members => Set<Member>();
        public DbSet<Loan> Loans => Set<Loan>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<ReturnRequest> ReturnRequests => Set<ReturnRequest>();
        public DbSet<Favorite> Favorites => Set<Favorite>();

        public LibraryContext()
        {
        }

        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options)
        {
        }

        /// <summary>
        /// Geliştirme/örnek amaçlı: appsettings.json üzerinden sağlayıcıyı ve bağlantı dizesini otomatik okur.
        /// Gerçek uygulamada DI ile konfigüre edilmesi tavsiye edilir.
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                var provider = configuration["DatabaseProvider"] ?? "SqlServer";

                if (string.Equals(provider, "PostgreSql", StringComparison.OrdinalIgnoreCase))
                {
                    var npgsql = configuration.GetConnectionString("PostgreSql") ??
                                 "Host=localhost;Database=KutuphaneOtomasyonu;Username=postgres;Password=postgres";
                    optionsBuilder.UseNpgsql(npgsql);
                }
                else
                {
                    var sql = configuration.GetConnectionString("SqlServer") ??
                              "Server=(localdb)\\MSSQLLocalDB;Database=KutuphaneOtomasyonu;Trusted_Connection=True;MultipleActiveResultSets=true";
                    optionsBuilder.UseSqlServer(sql);
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tabloların isimleri
            modelBuilder.Entity<Book>().ToTable("Books");
            modelBuilder.Entity<Copy>().ToTable("Copies");
            modelBuilder.Entity<Member>().ToTable("Members");
            modelBuilder.Entity<Loan>().ToTable("Loans");
            modelBuilder.Entity<Reservation>().ToTable("Reservations");
            modelBuilder.Entity<AuditLog>().ToTable("Audit_Log");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Payment>().ToTable("Payments");
            modelBuilder.Entity<ReturnRequest>().ToTable("ReturnRequests");
            modelBuilder.Entity<Favorite>().ToTable("Favorites");
            
            // CopyData için keyless entity (raw SQL sorguları için)
            modelBuilder.Entity<CopyData>().HasNoKey();

            // Book: benzersiz ISBN
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.Isbn)
                .IsUnique();

            modelBuilder.Entity<Book>()
                .Property(b => b.Isbn)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<Book>()
                .Property(b => b.Title)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Book>()
                .Property(b => b.Author)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Book>()
                .Property(b => b.Category)
                .HasMaxLength(100)
                .IsRequired();

            // Member: benzersiz Email
            modelBuilder.Entity<Member>()
                .HasIndex(m => m.Email)
                .IsUnique();

            modelBuilder.Entity<Member>()
                .Property(m => m.Email)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Member>()
                .Property(m => m.FullName)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Member>()
                .Property(m => m.Phone)
                .HasMaxLength(30);

            // User: benzersiz Username ve Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Username)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.FullName)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            // Enum -> string dönüşümü (istenirse veritabanında okunabilirlik için)
            modelBuilder.Entity<Copy>()
                .Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            
            // ShelfLocation için null-safe yapılandırma
            modelBuilder.Entity<Copy>()
                .Property(c => c.ShelfLocation)
                .HasMaxLength(50)
                .IsRequired(false); // Nullable olabilir

            modelBuilder.Entity<Member>()
                .Property(m => m.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // ReturnRequest ilişkileri
            modelBuilder.Entity<ReturnRequest>()
                .HasOne(r => r.Loan)
                .WithMany()
                .HasForeignKey(r => r.LoanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReturnRequest>()
                .HasOne(r => r.ProcessedByUser)
                .WithMany()
                .HasForeignKey(r => r.ProcessedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ReturnRequest>()
                .Property(r => r.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Reservation ilişkileri
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Member)
                .WithMany() // Üye üzerinde ayrı bir Reservations koleksiyonu tutulmuyor
                .HasForeignKey(r => r.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Copy)
                .WithMany(c => c.Reservations) // Copy entity'sindeki Reservations navigation property'sini kullan
                .HasForeignKey(r => r.CopyId)
                .OnDelete(DeleteBehavior.Restrict);

            // İlişkiler
            modelBuilder.Entity<Copy>()
                .HasOne(c => c.Book)
                .WithMany(b => b.Copies)
                .HasForeignKey(c => c.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Copy)
                .WithMany(c => c.Loans)
                .HasForeignKey(l => l.CopyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Member)
                .WithMany(m => m.Loans)
                .HasForeignKey(l => l.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment ilişkileri
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Loan)
                .WithMany()
                .HasForeignKey(p => p.LoanId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            // Favorite ilişkileri
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Book)
                .WithMany()
                .HasForeignKey(f => f.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Favorite: Bir kullanıcı aynı kitabı iki kez favoriye ekleyemez
            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.BookId })
                .IsUnique();

            // Tarih alanları için gerekli varsayılanlar
            modelBuilder.Entity<Loan>()
                .Property(l => l.LoanedAt)
                .HasDefaultValueSql("GETUTCDATE()"); // SQL Server için. PostgreSQL kullanılıyorsa NOW() tercih edilebilir.

            // DueAt uygulama mantığıyla atanacaktır (ödünç alırken +14 gün)

            // Rapor view'ları için keyless entity'ler
            modelBuilder.Entity<TopBookRow>().HasNoKey();
            modelBuilder.Entity<ActiveLoanRow>().HasNoKey();
            modelBuilder.Entity<MemberLoanCountRow>().HasNoKey();
            modelBuilder.Entity<AuditLogRow>().HasNoKey();
            modelBuilder.Entity<ReservationRow>().HasNoKey();
        }
    }
}


