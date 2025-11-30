-- RunAllProgrammability.sql
-- Tüm programmability dosyalarını sırayla çalıştırmak için master script
-- Kullanım: Bu dosyayı SQL Server Management Studio'da açıp F5 ile çalıştırın

-- 1. VIEW'ları oluştur
:r views.sql
GO

-- 2. FUNCTION'ı oluştur
:r functions.sql
GO

-- 3. TRIGGER'ı oluştur
:r triggers.sql
GO

-- 4. STORED PROCEDURE'ları oluştur
:r sp.sql
GO

PRINT '========================================';
PRINT 'Tüm programmability objeleri başarıyla oluşturuldu!';
PRINT 'Oluşturulan objeler:';
PRINT '  - 2 VIEW (vw_active_loans, vw_top_books_last30)';
PRINT '  - 1 FUNCTION (fn_top_books_between)';
PRINT '  - 1 TRIGGER (tr_loans_audit)';
PRINT '  - 2 STORED PROCEDURE (sp_borrow_copy, sp_return_copy)';
PRINT '========================================';
GO






