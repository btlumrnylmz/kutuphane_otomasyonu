# Programmability Kurulum Talimatları

Bu klasördeki SQL dosyalarını veritabanınıza uygulamak için aşağıdaki yöntemlerden birini kullanabilirsiniz.

## ⚠️ ÖNEMLİ NOTLAR

1. **Sıralama Önemli:** Dosyaları şu sırayla çalıştırın:
   - `triggers.sql` (Audit_Log ve Reservations tablolarını oluşturur)
   - `views.sql`
   - `functions.sql`
   - `sp.sql`

2. **Veritabanı:** `KutuphaneOtomasyonu` veritabanını seçtiğinizden emin olun.

## 📋 YÖNTEM 1: SQL Server Management Studio (SSMS) - ÖNERİLEN

### Adım 1: SSMS'i Açın
1. SQL Server Management Studio'yu başlatın
2. Veritabanınıza bağlanın (Server: `DESKTOP-D00J96T\SQLEXPRESS`)

### Adım 2: Veritabanını Seçin
```sql
USE KutuphaneOtomasyonu;
GO
```

### Adım 3: Dosyaları Çalıştırın

#### Tek Tek Çalıştırma:
1. **File → Open → File** (veya `Ctrl+O`) ile dosyayı açın
2. Dosya yolunu seçin (örn: `C:\Users\betul\kutuphane_otomasyonu_vtys2\programmability\triggers.sql`)
3. **F5** tuşuna basın veya **Execute** butonuna tıklayın
4. "Commands completed successfully" mesajını kontrol edin

#### Hızlı Yöntem - Tümünü Birden:
1. `RunAllProgrammability.sql` dosyasını açın
2. **Ancak** `:r` komutu çalışmıyorsa, dosyaları sırayla manuel açıp çalıştırın:
   - `triggers.sql`
   - `views.sql`
   - `functions.sql`
   - `sp.sql`

## 📋 YÖNTEM 2: Visual Studio SQL Server Object Explorer

### Adım 1: SQL Server Object Explorer'ı Açın
1. Visual Studio'yu açın
2. **View → SQL Server Object Explorer** (veya `Ctrl+\, Ctrl+S`)
3. Projenizdeki veritabanına bağlanın

### Adım 2: New Query
1. Veritabanına sağ tıklayın
2. **New Query** seçin
3. Dosya içeriğini kopyala-yapıştır yapın
4. **Execute** butonuna tıklayın (veya `Ctrl+Shift+E`)

## 📋 YÖNTEM 3: Komut Satırı (sqlcmd)

### Adım 1: Command Prompt'u Açın
Windows + R → `cmd` → Enter

### Adım 2: Dosyaları Çalıştırın
```cmd
cd C:\Users\betul\kutuphane_otomasyonu_vtys2\programmability

sqlcmd -S DESKTOP-D00J96T\SQLEXPRESS -d KutuphaneOtomasyonu -i triggers.sql
sqlcmd -S DESKTOP-D00J96T\SQLEXPRESS -d KutuphaneOtomasyonu -i views.sql
sqlcmd -S DESKTOP-D00J96T\SQLEXPRESS -d KutuphaneOtomasyonu -i functions.sql
sqlcmd -S DESKTOP-D00J96T\SQLEXPRESS -d KutuphaneOtomasyonu -i sp.sql
```

**Not:** Eğer Windows Authentication kullanmıyorsanız:
```cmd
sqlcmd -S DESKTOP-D00J96T\SQLEXPRESS -d KutuphaneOtomasyonu -U kullanici_adi -P sifre -i sp.sql
```

## 📋 YÖNTEM 4: Azure Data Studio

1. Azure Data Studio'yu açın
2. Veritabanınıza bağlanın
3. **File → Open File** ile SQL dosyasını açın
4. Veritabanını seçin (sağ üstteki dropdown'dan)
5. **Run** butonuna tıklayın (veya `F5`)

## ✅ KONTROL - Oluşturulan Objeleri Doğrulama

Stored procedure'ların başarıyla oluşturulduğunu kontrol etmek için:

```sql
-- Tüm stored procedure'ları listele
SELECT 
    ROUTINE_SCHEMA,
    ROUTINE_NAME,
    CREATED,
    LAST_ALTERED
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
    AND ROUTINE_NAME IN ('sp_borrow_copy', 'sp_return_copy')
ORDER BY ROUTINE_NAME;

-- Procedure'ların varlığını kontrol et
IF OBJECT_ID('dbo.sp_borrow_copy', 'P') IS NOT NULL
    PRINT '✓ sp_borrow_copy başarıyla oluşturuldu'
ELSE
    PRINT '✗ sp_borrow_copy bulunamadı'

IF OBJECT_ID('dbo.sp_return_copy', 'P') IS NOT NULL
    PRINT '✓ sp_return_copy başarıyla oluşturuldu'
ELSE
    PRINT '✗ sp_return_copy bulunamadı'
GO
```

## 🔍 HATA GİDERME

### Hata: "Invalid object name 'dbo.Copies'"
- **Çözüm:** Önce EF Core migration'larını çalıştırın (`dotnet ef database update`)

### Hata: "Must declare the scalar variable '@member_id'"
- **Çözüm:** Dosyanın tamamını seçip çalıştırın, yalnızca bir kısmını değil

### Hata: "Batch" hatası
- **Çözüm:** Her `GO` komutundan sonra dosyayı ayırın veya dosyanın tamamını çalıştırın

## 📝 TEST - Stored Procedure'ları Test Etme

Stored procedure'ları test etmek için:

```sql
-- Test 1: sp_borrow_copy
-- Önce uygun bir member_id ve copy_id bulun
DECLARE @member_id INT = 1;  -- Gerçek bir member ID girin
DECLARE @copy_id INT = 1;    -- Status='Available' olan bir copy ID girin

EXEC dbo.sp_borrow_copy @member_id = @member_id, @copy_id = @copy_id;

-- Test 2: sp_return_copy
-- Önce aktif bir loan_id bulun
DECLARE @loan_id INT = 1;    -- Gerçek bir loan ID girin

EXEC dbo.sp_return_copy @loan_id = @loan_id;
```

## 📞 YARDIM

Sorun yaşarsanız:
1. Hata mesajını kopyalayın
2. Hangi yöntemi kullandığınızı not edin
3. SQL Server versiyonunuzu kontrol edin (`SELECT @@VERSION`)




