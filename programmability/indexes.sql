-- indexes.sql
-- Açıklama: Performans optimizasyonu için indeksler
-- Hedef: SQL Server
-- PDF gereksinimi: N4 - Performans: hedef sorgular için uygun indeksler

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1) Books tablosu için indeksler
-- ISBN üzerinde zaten UNIQUE indeks var (migration'da oluşturuldu)
-- Title üzerinde arama için indeks
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Books_Title' AND object_id = OBJECT_ID('dbo.Books'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Books_Title ON dbo.Books(Title);
    PRINT 'IX_Books_Title indeksi oluşturuldu.';
END
GO

-- Author üzerinde arama için indeks
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Books_Author' AND object_id = OBJECT_ID('dbo.Books'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Books_Author ON dbo.Books(Author);
    PRINT 'IX_Books_Author indeksi oluşturuldu.';
END
GO

-- Category üzerinde filtreleme için indeks
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Books_Category' AND object_id = OBJECT_ID('dbo.Books'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Books_Category ON dbo.Books(Category);
    PRINT 'IX_Books_Category indeksi oluşturuldu.';
END
GO

-- 2) Copies tablosu için indeksler
-- BookId üzerinde JOIN performansı için (zaten FK, ama explicit indeks ekliyoruz)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Copies_BookId' AND object_id = OBJECT_ID('dbo.Copies'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Copies_BookId ON dbo.Copies(BookId);
    PRINT 'IX_Copies_BookId indeksi oluşturuldu.';
END
GO

-- Status üzerinde filtreleme için indeks (Available kopyaları bulmak için)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Copies_Status' AND object_id = OBJECT_ID('dbo.Copies'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Copies_Status ON dbo.Copies(Status);
    PRINT 'IX_Copies_Status indeksi oluşturuldu.';
END
GO

-- 3) Loans tablosu için indeksler
-- CopyId üzerinde JOIN performansı için
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Loans_CopyId' AND object_id = OBJECT_ID('dbo.Loans'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Loans_CopyId ON dbo.Loans(CopyId);
    PRINT 'IX_Loans_CopyId indeksi oluşturuldu.';
END
GO

-- MemberId üzerinde JOIN ve filtreleme için
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Loans_MemberId' AND object_id = OBJECT_ID('dbo.Loans'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Loans_MemberId ON dbo.Loans(MemberId);
    PRINT 'IX_Loans_MemberId indeksi oluşturuldu.';
END
GO

-- ReturnedAt üzerinde filtreleme için (aktif ödünçleri bulmak için)
-- NULL değerler için özel indeks
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Loans_ReturnedAt' AND object_id = OBJECT_ID('dbo.Loans'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Loans_ReturnedAt ON dbo.Loans(ReturnedAt);
    PRINT 'IX_Loans_ReturnedAt indeksi oluşturuldu.';
END
GO

-- DueAt üzerinde gecikme kontrolü için
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Loans_DueAt' AND object_id = OBJECT_ID('dbo.Loans'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Loans_DueAt ON dbo.Loans(DueAt);
    PRINT 'IX_Loans_DueAt indeksi oluşturuldu.';
END
GO

-- Composite indeks: MemberId + ReturnedAt (aktif ödünç sayısı sorguları için)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Loans_MemberId_ReturnedAt' AND object_id = OBJECT_ID('dbo.Loans'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Loans_MemberId_ReturnedAt ON dbo.Loans(MemberId, ReturnedAt);
    PRINT 'IX_Loans_MemberId_ReturnedAt composite indeksi oluşturuldu.';
END
GO

-- LoanedAt üzerinde tarih aralığı sorguları için (raporlar)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Loans_LoanedAt' AND object_id = OBJECT_ID('dbo.Loans'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Loans_LoanedAt ON dbo.Loans(LoanedAt);
    PRINT 'IX_Loans_LoanedAt indeksi oluşturuldu.';
END
GO

-- 4) Members tablosu için indeksler
-- Email üzerinde zaten UNIQUE indeks var (migration'da oluşturuldu)
-- FullName üzerinde arama için indeks
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Members_FullName' AND object_id = OBJECT_ID('dbo.Members'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Members_FullName ON dbo.Members(FullName);
    PRINT 'IX_Members_FullName indeksi oluşturuldu.';
END
GO

-- Status üzerinde filtreleme için
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Members_Status' AND object_id = OBJECT_ID('dbo.Members'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Members_Status ON dbo.Members(Status);
    PRINT 'IX_Members_Status indeksi oluşturuldu.';
END
GO

-- 5) Payments tablosu için indeksler
-- LoanId üzerinde JOIN performansı için
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Payments_LoanId' AND object_id = OBJECT_ID('dbo.Payments'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Payments_LoanId ON dbo.Payments(LoanId);
    PRINT 'IX_Payments_LoanId indeksi oluşturuldu.';
END
GO

-- PaymentDate üzerinde tarih sorguları için
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Payments_PaymentDate' AND object_id = OBJECT_ID('dbo.Payments'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Payments_PaymentDate ON dbo.Payments(PaymentDate);
    PRINT 'IX_Payments_PaymentDate indeksi oluşturuldu.';
END
GO

-- 6) Users tablosu için indeksler
-- Email üzerinde login sorguları için (zaten UNIQUE olabilir)
-- Username üzerinde login sorguları için
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Username' AND object_id = OBJECT_ID('dbo.Users'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_Username ON dbo.Users(Username);
    PRINT 'IX_Users_Username indeksi oluşturuldu.';
END
GO

-- 7) Favorites tablosu için indeksler
-- UserId + BookId composite indeks (zaten UNIQUE olarak migration'da oluşturuldu)
-- UserId üzerinde ayrı indeks (favori listesi sorguları için)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Favorites_UserId' AND object_id = OBJECT_ID('dbo.Favorites'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Favorites_UserId ON dbo.Favorites(UserId);
    PRINT 'IX_Favorites_UserId indeksi oluşturuldu.';
END
GO

-- 8) ReturnRequests tablosu için indeksler
-- LoanId üzerinde JOIN performansı için
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReturnRequests_LoanId' AND object_id = OBJECT_ID('dbo.ReturnRequests'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ReturnRequests_LoanId ON dbo.ReturnRequests(LoanId);
    PRINT 'IX_ReturnRequests_LoanId indeksi oluşturuldu.';
END
GO

-- Status üzerinde filtreleme için (pending returns sorguları)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ReturnRequests_Status' AND object_id = OBJECT_ID('dbo.ReturnRequests'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ReturnRequests_Status ON dbo.ReturnRequests(Status);
    PRINT 'IX_ReturnRequests_Status indeksi oluşturuldu.';
END
GO

-- 9) Reservations tablosu için indeksler
-- CopyId üzerinde JOIN ve filtreleme için
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reservations_CopyId' AND object_id = OBJECT_ID('dbo.Reservations'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Reservations_CopyId ON dbo.Reservations(CopyId);
    PRINT 'IX_Reservations_CopyId indeksi oluşturuldu.';
END
GO

-- MemberId üzerinde filtreleme için
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reservations_MemberId' AND object_id = OBJECT_ID('dbo.Reservations'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Reservations_MemberId ON dbo.Reservations(MemberId);
    PRINT 'IX_Reservations_MemberId indeksi oluşturuldu.';
END
GO

-- 10) Audit_Log tablosu için indeksler
-- LoanId üzerinde JOIN performansı için
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Audit_Log_LoanId' AND object_id = OBJECT_ID('dbo.Audit_Log'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Audit_Log_LoanId ON dbo.Audit_Log(LoanId);
    PRINT 'IX_Audit_Log_LoanId indeksi oluşturuldu.';
END
GO

-- ActionTime üzerinde tarih sorguları için (raporlar)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Audit_Log_ActionTime' AND object_id = OBJECT_ID('dbo.Audit_Log'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Audit_Log_ActionTime ON dbo.Audit_Log(ActionTime);
    PRINT 'IX_Audit_Log_ActionTime indeksi oluşturuldu.';
END
GO

PRINT 'Tüm indeksler başarıyla oluşturuldu veya zaten mevcut.';
GO






