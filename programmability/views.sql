-- views.sql
-- Açıklama: Raporlar için gerekli VIEW tanımları
-- Hedef: SQL Server (GETUTCDATE, DATEDIFF kullanımı)

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1) Aktif ödünçler görünümü: üye adı, kitap adı, vade tarihi ve gecikme gün sayısı
IF OBJECT_ID('dbo.vw_active_loans', 'V') IS NOT NULL
    DROP VIEW dbo.vw_active_loans;
GO
CREATE VIEW dbo.vw_active_loans
AS
SELECT 
    m.FullName AS member_name,
    b.Title AS book_title,
    l.DueAt AS due_at,
    CASE WHEN l.DueAt < GETUTCDATE() THEN DATEDIFF(DAY, l.DueAt, GETUTCDATE()) ELSE 0 END AS delay_days,
    l.LoanId,
    l.LoanedAt,
    c.ShelfLocation
FROM dbo.Loans l
INNER JOIN dbo.Members m ON m.MemberId = l.MemberId
INNER JOIN dbo.Copies c ON c.CopyId = l.CopyId
INNER JOIN dbo.Books b ON b.BookId = c.BookId
WHERE l.ReturnedAt IS NULL;
GO

-- 2) Son 30 günde en çok ödünç alınan 10 kitap
IF OBJECT_ID('dbo.vw_top_books_last30', 'V') IS NOT NULL
    DROP VIEW dbo.vw_top_books_last30;
GO
CREATE VIEW dbo.vw_top_books_last30
AS
SELECT TOP (10)
    b.Title AS title,
    COUNT(*) AS borrow_count
FROM dbo.Loans l
INNER JOIN dbo.Copies c ON c.CopyId = l.CopyId
INNER JOIN dbo.Books b ON b.BookId = c.BookId
WHERE l.LoanedAt >= DATEADD(DAY, -30, GETUTCDATE())
GROUP BY b.Title
ORDER BY COUNT(*) DESC, b.Title ASC;
GO


