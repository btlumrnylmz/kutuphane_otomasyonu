-- KONTROL SCRIPTI
-- Tüm programmability objelerinin başarıyla oluşturulduğunu doğrulamak için
-- Bu sorguyu SSMS'de çalıştırın

USE KutuphaneOtomasyonu;
GO

PRINT '========================================';
PRINT 'PROGRAMMABILITY OBJELERI KONTROLU';
PRINT '========================================';
PRINT '';

-- 1. VIEW'LARI KONTROL ET
PRINT '1. VIEW''LAR:';
IF OBJECT_ID('dbo.vw_active_loans', 'V') IS NOT NULL
    PRINT '   ✓ vw_active_loans - VAR';
ELSE
    PRINT '   ✗ vw_active_loans - YOK';

IF OBJECT_ID('dbo.vw_top_books_last30', 'V') IS NOT NULL
    PRINT '   ✓ vw_top_books_last30 - VAR';
ELSE
    PRINT '   ✗ vw_top_books_last30 - YOK';
PRINT '';

-- 2. FUNCTION'I KONTROL ET
PRINT '2. FUNCTION:';
IF OBJECT_ID('dbo.fn_top_books_between', 'IF') IS NOT NULL
    PRINT '   ✓ fn_top_books_between - VAR';
ELSE
    PRINT '   ✗ fn_top_books_between - YOK';
PRINT '';

-- 3. STORED PROCEDURE'LARI KONTROL ET
PRINT '3. STORED PROCEDURE''LAR:';
IF OBJECT_ID('dbo.sp_borrow_copy', 'P') IS NOT NULL
    PRINT '   ✓ sp_borrow_copy - VAR';
ELSE
    PRINT '   ✗ sp_borrow_copy - YOK';

IF OBJECT_ID('dbo.sp_return_copy', 'P') IS NOT NULL
    PRINT '   ✓ sp_return_copy - VAR';
ELSE
    PRINT '   ✗ sp_return_copy - YOK';
PRINT '';

-- 4. TRIGGER'I KONTROL ET
PRINT '4. TRIGGER:';
IF OBJECT_ID('dbo.tr_loans_audit', 'TR') IS NOT NULL
    PRINT '   ✓ tr_loans_audit - VAR';
ELSE
    PRINT '   ✗ tr_loans_audit - YOK';
PRINT '';

-- 5. TABLOLARI KONTROL ET (trigger.sql tarafından oluşturulan)
PRINT '5. EK TABLOLAR:';
IF OBJECT_ID('dbo.Audit_Log', 'U') IS NOT NULL
    PRINT '   ✓ Audit_Log tablosu - VAR';
ELSE
    PRINT '   ✗ Audit_Log tablosu - YOK';

IF OBJECT_ID('dbo.Reservations', 'U') IS NOT NULL
    PRINT '   ✓ Reservations tablosu - VAR';
ELSE
    PRINT '   ✗ Reservations tablosu - YOK';
PRINT '';

-- 6. DETAYLI LİSTELEME
PRINT '========================================';
PRINT 'DETAYLI OBJE LİSTESİ:';
PRINT '========================================';
PRINT '';

-- VIEW'ları listele
SELECT 'VIEW' AS ObjectType, TABLE_NAME AS ObjectName, 'dbo' AS SchemaName
FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_SCHEMA = 'dbo'
    AND TABLE_NAME IN ('vw_active_loans', 'vw_top_books_last30')
UNION ALL
-- Function'ları listele
SELECT 'FUNCTION' AS ObjectType, ROUTINE_NAME AS ObjectName, ROUTINE_SCHEMA AS SchemaName
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'FUNCTION'
    AND ROUTINE_NAME = 'fn_top_books_between'
UNION ALL
-- Stored Procedure'ları listele
SELECT 'PROCEDURE' AS ObjectType, ROUTINE_NAME AS ObjectName, ROUTINE_SCHEMA AS SchemaName
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
    AND ROUTINE_NAME IN ('sp_borrow_copy', 'sp_return_copy')
ORDER BY ObjectType, ObjectName;

PRINT '';
PRINT '========================================';
PRINT 'TOPLAM OBJE SAYISI:';
PRINT '========================================';

DECLARE @view_count INT = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME IN ('vw_active_loans', 'vw_top_books_last30'));
DECLARE @function_count INT = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'FUNCTION' AND ROUTINE_NAME = 'fn_top_books_between');
DECLARE @procedure_count INT = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_TYPE = 'PROCEDURE' AND ROUTINE_NAME IN ('sp_borrow_copy', 'sp_return_copy'));
DECLARE @trigger_count INT = (SELECT COUNT(*) FROM sys.triggers WHERE name = 'tr_loans_audit');

PRINT 'VIEW''lar: ' + CAST(@view_count AS VARCHAR(10)) + ' / 2';
PRINT 'FUNCTION''lar: ' + CAST(@function_count AS VARCHAR(10)) + ' / 1';
PRINT 'PROCEDURE''lar: ' + CAST(@procedure_count AS VARCHAR(10)) + ' / 2';
PRINT 'TRIGGER''lar: ' + CAST(@trigger_count AS VARCHAR(10)) + ' / 1';
PRINT '';
PRINT 'TOPLAM: ' + CAST((@view_count + @function_count + @procedure_count + @trigger_count) AS VARCHAR(10)) + ' / 6';

IF (@view_count = 2 AND @function_count = 1 AND @procedure_count = 2 AND @trigger_count = 1)
BEGIN
    PRINT '';
    PRINT '✓ TÜM OBJELER BAŞARIYLA OLUŞTURULDU!';
    PRINT '✓ PROJE PDF GEREKSİNİMLERİNİ KARŞILIYOR!';
END
ELSE
BEGIN
    PRINT '';
    PRINT '⚠ BAZI OBJELER EKSİK OLABİLİR, YUKARIDAKİ LİSTEYİ KONTROL EDİN';
END

GO




