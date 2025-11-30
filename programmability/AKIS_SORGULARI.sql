-- ============================================================
-- AKIŞ SORGULARI - Stored Procedure'ların İş Akışını SQL ile İfade Etme
-- ============================================================
-- Bu dosya, stored procedure'ların işleyişini SQL sorguları ile
-- adım adım gösterir. Akış diyagramı yerine kullanılabilir.
-- ============================================================

-- ============================================================
-- 1. ÖDÜNÇ VERME AKIŞI (sp_borrow_copy Mantığı)
-- ============================================================

-- ADIM 1: Kopya Kontrolü - Kopya mevcut ve müsait mi?
-- Bu sorgu, ödünç vermeden önce kopyanın durumunu kontrol eder
SELECT 
    CopyId,
    BookId,
    Status,
    ShelfLocation,
    CASE 
        WHEN CopyId = @copy_id AND Status = 'Available' THEN 'Kopya ödünç verilebilir'
        WHEN CopyId = @copy_id AND Status != 'Available' THEN 'HATA: Kopya müsait değil'
        WHEN CopyId = @copy_id THEN 'Kopya bulundu'
        ELSE 'HATA: Kopya bulunamadı'
    END AS KontrolSonucu
FROM dbo.Copies
WHERE CopyId = @copy_id; -- Örnek: @copy_id = 1


-- ADIM 2: Üye Kontrolü - Üye mevcut ve aktif mi?
-- Bu sorgu, ödünç almadan önce üyenin durumunu kontrol eder
SELECT 
    MemberId,
    FullName,
    Email,
    Status,
    CASE 
        WHEN MemberId = @member_id AND Status = 'Active' THEN 'Üye ödünç alabilir'
        WHEN MemberId = @member_id AND Status != 'Active' THEN 'HATA: Üye aktif değil'
        ELSE 'HATA: Üye bulunamadı'
    END AS KontrolSonucu
FROM dbo.Members
WHERE MemberId = @member_id; -- Örnek: @member_id = 1


-- ADIM 3: Aktif Ödünç Sayısı Kontrolü - Üyenin aktif ödünç sayısı 3'ten az mı?
-- Bu sorgu, maksimum 3 aktif ödünç kuralını kontrol eder
SELECT 
    @member_id AS MemberId,
    COUNT(*) AS AktifOduncSayisi,
    CASE 
        WHEN COUNT(*) < 3 THEN 'Üye yeni kitap ödünç alabilir (Limit: ' + CAST(COUNT(*) AS NVARCHAR) + '/3)'
        WHEN COUNT(*) >= 3 THEN 'HATA: Üyenin aktif ödünç limiti dolmuş (Maksimum 3)'
    END AS KontrolSonucu
FROM dbo.Loans
WHERE MemberId = @member_id 
  AND ReturnedAt IS NULL; -- Aktif ödünçler


-- ADIM 4: Ödünç Kaydı Oluşturma (Transaction İçinde)
-- Bu sorgu, tüm kontroller başarılıysa ödünç kaydını oluşturur
BEGIN TRANSACTION;

DECLARE @now DATETIME2 = SYSUTCDATETIME();
DECLARE @due_date DATETIME2 = DATEADD(DAY, 14, @now);

INSERT INTO dbo.Loans (MemberId, CopyId, LoanedAt, DueAt, ReturnedAt)
VALUES (@member_id, @copy_id, @now, @due_date, NULL);

-- Kontrol: Eklenen kaydı göster
SELECT 
    LoanId,
    MemberId,
    CopyId,
    LoanedAt,
    DueAt,
    ReturnedAt,
    'Ödünç kaydı başarıyla oluşturuldu' AS IslemSonucu
FROM dbo.Loans
WHERE LoanId = SCOPE_IDENTITY();

COMMIT TRANSACTION;
-- ROLLBACK TRANSACTION; -- Hata durumunda


-- ADIM 5: Kopya Durumunu Güncelleme (Transaction İçinde)
-- Bu sorgu, kopyanın durumunu 'Loaned' yapar
BEGIN TRANSACTION;

UPDATE dbo.Copies
SET Status = 'Loaned'
WHERE CopyId = @copy_id;

-- Kontrol: Güncellenmiş durumu göster
SELECT 
    CopyId,
    Status,
    'Kopya durumu başarıyla güncellendi' AS IslemSonucu
FROM dbo.Copies
WHERE CopyId = @copy_id;

COMMIT TRANSACTION;


-- ============================================================
-- ÖDÜNÇ VERME AKIŞI - TÜM ADIMLARI BİRLEŞTİREN ÖZET SORGU
-- ============================================================
-- Bu sorgu, tüm kontrol adımlarını bir arada gösterir
SELECT 
    -- Kopya Kontrolü
    CASE 
        WHEN EXISTS (SELECT 1 FROM dbo.Copies WHERE CopyId = @copy_id AND Status = 'Available')
        THEN '✓ Kopya müsait'
        ELSE '✗ HATA: Kopya bulunamadı veya müsait değil'
    END AS Adim1_KopyaKontrolu,
    
    -- Üye Kontrolü
    CASE 
        WHEN EXISTS (SELECT 1 FROM dbo.Members WHERE MemberId = @member_id AND Status = 'Active')
        THEN '✓ Üye aktif'
        ELSE '✗ HATA: Üye bulunamadı veya aktif değil'
    END AS Adim2_UyeKontrolu,
    
    -- Aktif Ödünç Sayısı Kontrolü
    CASE 
        WHEN (SELECT COUNT(*) FROM dbo.Loans WHERE MemberId = @member_id AND ReturnedAt IS NULL) < 3
        THEN '✓ Ödünç limiti uygun (' + 
             CAST((SELECT COUNT(*) FROM dbo.Loans WHERE MemberId = @member_id AND ReturnedAt IS NULL) AS NVARCHAR) + '/3)'
        ELSE '✗ HATA: Ödünç limiti dolmuş (Maksimum 3)'
    END AS Adim3_OduncLimitKontrolu,
    
    -- Genel Sonuç
    CASE 
        WHEN EXISTS (SELECT 1 FROM dbo.Copies WHERE CopyId = @copy_id AND Status = 'Available')
         AND EXISTS (SELECT 1 FROM dbo.Members WHERE MemberId = @member_id AND Status = 'Active')
         AND (SELECT COUNT(*) FROM dbo.Loans WHERE MemberId = @member_id AND ReturnedAt IS NULL) < 3
        THEN '✓ TÜM KONTROLLER BAŞARILI - ÖDÜNÇ VERİLEBİLİR'
        ELSE '✗ KONTROL BAŞARISIZ - ÖDÜNÇ VERİLEMEZ'
    END AS GenelSonuc;


-- ============================================================
-- 2. İADE ALMA AKIŞI (sp_return_copy Mantığı)
-- ============================================================

-- ADIM 1: Loan Kaydı Kontrolü - Ödünç kaydı mevcut mu?
-- Bu sorgu, iade edilecek ödünç kaydını bulur
SELECT 
    l.LoanId,
    l.MemberId,
    l.CopyId,
    l.LoanedAt,
    l.DueAt,
    l.ReturnedAt,
    CASE 
        WHEN l.LoanId = @loan_id AND l.ReturnedAt IS NULL THEN 'Ödünç kaydı bulundu, iade edilebilir'
        WHEN l.LoanId = @loan_id AND l.ReturnedAt IS NOT NULL THEN 'HATA: Bu ödünç kaydı zaten iade edilmiş'
        ELSE 'HATA: Ödünç kaydı bulunamadı'
    END AS KontrolSonucu
FROM dbo.Loans l
WHERE l.LoanId = @loan_id; -- Örnek: @loan_id = 1


-- ADIM 2: İade Tarihini Güncelleme (Transaction İçinde)
-- Bu sorgu, ReturnedAt alanını güncel tarih/saat ile günceller
BEGIN TRANSACTION;

-- Önce mevcut durumu göster
SELECT 
    LoanId,
    ReturnedAt,
    'İade işlemi başlatılıyor...' AS IslemDurumu
FROM dbo.Loans
WHERE LoanId = @loan_id;

-- ReturnedAt'i güncelle (trigger tetiklenecek - Audit_Log'a kayıt eklenecek)
UPDATE dbo.Loans
SET ReturnedAt = SYSUTCDATETIME()
WHERE LoanId = @loan_id;

-- Kontrol: Güncellenmiş kaydı göster
SELECT 
    LoanId,
    MemberId,
    CopyId,
    LoanedAt,
    DueAt,
    ReturnedAt,
    CASE 
        WHEN ReturnedAt IS NOT NULL THEN 'İade tarihi başarıyla güncellendi'
        ELSE 'HATA: İade tarihi güncellenemedi'
    END AS IslemSonucu,
    -- Gecikme kontrolü
    CASE 
        WHEN ReturnedAt > DueAt THEN 'GECİKMELİ İADE - ' + CAST(DATEDIFF(DAY, DueAt, ReturnedAt) AS NVARCHAR) + ' gün gecikme'
        ELSE 'ZAMANINDA İADE'
    END AS GecikmeDurumu
FROM dbo.Loans
WHERE LoanId = @loan_id;

COMMIT TRANSACTION;


-- ADIM 3: Kopya Durumunu Güncelleme (Transaction İçinde)
-- Bu sorgu, kopyanın durumunu 'Available' yapar
BEGIN TRANSACTION;

-- Önce CopyId'yi bul
DECLARE @copy_id_for_return INT;
SELECT @copy_id_for_return = CopyId
FROM dbo.Loans
WHERE LoanId = @loan_id;

-- Kopya durumunu güncelle
UPDATE dbo.Copies
SET Status = 'Available'
WHERE CopyId = @copy_id_for_return;

-- Kontrol: Güncellenmiş durumu göster
SELECT 
    c.CopyId,
    c.Status,
    b.Title AS KitapAdi,
    'Kopya durumu başarıyla güncellendi - Yeni ödünç verilebilir' AS IslemSonucu
FROM dbo.Copies c
INNER JOIN dbo.Books b ON b.BookId = c.BookId
WHERE c.CopyId = @copy_id_for_return;

COMMIT TRANSACTION;


-- ============================================================
-- İADE ALMA AKIŞI - TÜM ADIMLARI BİRLEŞTİREN ÖZET SORGU
-- ============================================================
-- Bu sorgu, tüm kontrol adımlarını bir arada gösterir
SELECT 
    -- Loan Kaydı Kontrolü
    CASE 
        WHEN EXISTS (SELECT 1 FROM dbo.Loans WHERE LoanId = @loan_id)
        THEN CASE 
            WHEN (SELECT ReturnedAt FROM dbo.Loans WHERE LoanId = @loan_id) IS NULL
            THEN '✓ Ödünç kaydı bulundu ve iade edilmemiş'
            ELSE '✗ HATA: Bu ödünç kaydı zaten iade edilmiş'
        END
        ELSE '✗ HATA: Ödünç kaydı bulunamadı'
    END AS Adim1_LoanKontrolu,
    
    -- İade Durumu
    CASE 
        WHEN EXISTS (SELECT 1 FROM dbo.Loans WHERE LoanId = @loan_id AND ReturnedAt IS NULL)
        THEN '✓ İADE EDİLEBİLİR'
        ELSE '✗ İADE EDİLEMEZ'
    END AS GenelSonuc,
    
    -- Gecikme Bilgisi (varsa)
    CASE 
        WHEN EXISTS (SELECT 1 FROM dbo.Loans WHERE LoanId = @loan_id AND ReturnedAt IS NULL)
        THEN CASE 
            WHEN (SELECT DueAt FROM dbo.Loans WHERE LoanId = @loan_id) < GETUTCDATE()
            THEN '⚠ GECİKMELİ - ' + CAST(DATEDIFF(DAY, (SELECT DueAt FROM dbo.Loans WHERE LoanId = @loan_id), GETUTCDATE()) AS NVARCHAR) + ' gün gecikme var'
            ELSE '✓ Zamanında iade'
        END
        ELSE ''
    END AS GecikmeDurumu;


-- ============================================================
-- 3. AKIŞ DİYAGRAMI YERİNE: İŞLEM ADIMLARI ÖZET TABLOSU
-- ============================================================

-- ÖDÜNÇ VERME ADIMLARI
SELECT 
    1 AS AdimNo,
    'Kopya Kontrolü' AS AdimAdi,
    'Kopya mevcut mu ve Status = ''Available'' mı?' AS Kontrol,
    'IF NOT EXISTS → ROLLBACK + HATA' AS HataDurumu,
    'Devam' AS BasariliDurum
UNION ALL
SELECT 2, 'Üye Kontrolü', 'Üye mevcut mu ve Status = ''Active'' mı?', 'IF NOT EXISTS → ROLLBACK + HATA', 'Devam'
UNION ALL
SELECT 3, 'Aktif Ödünç Sayısı', 'COUNT(*) < 3 mü?', 'IF >= 3 → ROLLBACK + HATA', 'Devam'
UNION ALL
SELECT 4, 'Loan Kaydı Oluştur', 'INSERT INTO Loans (DueAt = LoanedAt + 14 gün)', 'TRY-CATCH → ROLLBACK', 'Devam'
UNION ALL
SELECT 5, 'Kopya Durumu Güncelle', 'UPDATE Copies SET Status = ''Loaned''', 'TRY-CATCH → ROLLBACK', 'Devam'
UNION ALL
SELECT 6, 'Transaction Commit', 'COMMIT TRANSACTION', '-', 'BAŞARILI - ÖDÜNÇ VERİLDİ';

-- İADE ALMA ADIMLARI
SELECT 
    1 AS AdimNo,
    'Loan Kaydı Kontrolü' AS AdimAdi,
    'LoanId ile kayıt bulundu mu ve ReturnedAt IS NULL mı?' AS Kontrol,
    'IF NULL → ROLLBACK + HATA' AS HataDurumu,
    'Devam' AS BasariliDurum
UNION ALL
SELECT 2, 'İade Durumu Kontrolü', 'ReturnedAt IS NOT NULL mı? (Zaten iade edilmiş mi?)', 'IF NOT NULL → ROLLBACK + HATA', 'Devam'
UNION ALL
SELECT 3, 'ReturnedAt Güncelle', 'UPDATE Loans SET ReturnedAt = SYSUTCDATETIME() (Trigger tetiklenecek)', 'TRY-CATCH → ROLLBACK', 'Devam'
UNION ALL
SELECT 4, 'Kopya Durumu Güncelle', 'UPDATE Copies SET Status = ''Available''', 'TRY-CATCH → ROLLBACK', 'Devam'
UNION ALL
SELECT 5, 'Transaction Commit', 'COMMIT TRANSACTION', '-', 'BAŞARILI - İADE ALINDI';


-- ============================================================
-- 4. EŞZAMANLILIK DENEYİ İÇİN SORGU
-- ============================================================

-- Aynı kopyayı eşzamanlı ödünç denemesi simülasyonu
-- Bu sorgu, iki farklı session'ın aynı anda aynı kopyayı ödünç almaya çalışmasını gösterir

-- SESSION 1: İlk kullanıcı ödünç almaya çalışıyor
BEGIN TRANSACTION;
    -- Kopya kontrolü (HENÜZ Available)
    SELECT 
        CopyId,
        Status,
        'SESSION 1: Kopya kontrolü - Status: ' + Status AS Durum
    FROM dbo.Copies
    WHERE CopyId = 1; -- Aynı kopya
    
    -- ... (diğer kontroller)
    
    -- Loan kaydı oluşturma (BİRİNCİ İŞLEM BAŞARILI OLACAK)
    -- INSERT INTO dbo.Loans ...
    -- UPDATE dbo.Copies SET Status = 'Loaned' ...
    
    -- COMMIT yapılacak
COMMIT TRANSACTION;

-- SESSION 2: İkinci kullanıcı aynı anda ödünç almaya çalışıyor
BEGIN TRANSACTION;
    -- Kopya kontrolü (ARTIK Loaned - SESSION 1 commit ettikten sonra)
    SELECT 
        CopyId,
        Status,
        'SESSION 2: Kopya kontrolü - Status: ' + Status + ' → HATA: Müsait değil!' AS Durum
    FROM dbo.Copies
    WHERE CopyId = 1; -- Aynı kopya (ama artık Loaned)
    
    -- HATA: Kopya bulunamadı veya müsait değil
    -- ROLLBACK yapılacak
ROLLBACK TRANSACTION;

-- Sonuç: İlk işlem başarılı, ikinci işlem hata alır (READ COMMITTED isolation level sayesinde)



