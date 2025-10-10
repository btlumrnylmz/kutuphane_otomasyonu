-- functions.sql
-- Açıklama: Tarih aralığında en çok ödünç alınan kitapları döndüren tablo-valued function

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID('dbo.fn_top_books_between', 'IF') IS NOT NULL
    DROP FUNCTION dbo.fn_top_books_between;
GO
CREATE FUNCTION dbo.fn_top_books_between
(
    @start_date DATETIME2,
    @end_date   DATETIME2
)
RETURNS TABLE
AS
RETURN
(
    SELECT 
        b.Title AS title,
        COUNT(*) AS borrow_count
    FROM dbo.Loans l
    INNER JOIN dbo.Copies c ON c.CopyId = l.CopyId
    INNER JOIN dbo.Books b ON b.BookId = c.BookId
    WHERE l.LoanedAt >= @start_date AND l.LoanedAt < @end_date
    GROUP BY b.Title
);
GO


