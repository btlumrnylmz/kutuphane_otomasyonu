# SQL Server Management Studio (SSMS) ile sp.sql Çalıştırma

## 📸 ADIM ADIM GÖRSELLİ REHBER

### ADIM 1: SSMS'i Açın ve Bağlanın
```
1. SQL Server Management Studio'yu başlatın
2. Connect to Server penceresinde:
   - Server name: DESKTOP-D00J96T\SQLEXPRESS
   - Authentication: Windows Authentication
   - [Connect] butonuna tıklayın
```

### ADIM 2: Veritabanını Seçin
```
1. Sol panelde "Object Explorer" açık olmalı
2. "Databases" klasörünü genişletin (+)
3. "KutuphaneOtomasyonu" veritabanına sağ tıklayın
4. "New Query" seçin (veya Ctrl+N)
```

### ADIM 3: sp.sql Dosyasını Açın
```
1. Üst menüden: File → Open → File (veya Ctrl+O)
2. Dosya seçici açılacak
3. Şu klasöre gidin:
   C:\Users\betul\kutuphane_otomasyonu_vtys2\programmability
4. "sp.sql" dosyasını seçin ve [Open] butonuna tıklayın
```

### ADIM 4: Veritabanını Doğrulayın
```
1. Query penceresinin üst kısmında dropdown menü var
2. "KutuphaneOtomasyonu" seçili olduğundan emin olun
3. Eğer değilse, dropdown'dan seçin
```

### ADIM 5: Dosyayı Çalıştırın
```
1. Query penceresinde tüm kod görünüyor olmalı
2. F5 tuşuna basın VEYA
3. Üst menüden: Query → Execute (veya Execute butonu)
4. Birkaç saniye bekleyin
```

### ADIM 6: Başarı Kontrolü
```
Messages sekmesinde şu mesajı görmelisiniz:
"Commands completed successfully."

Eğer hata varsa, Errors listesinde görünecektir.
```

---

## ✅ DOĞRULAMA - Stored Procedure'lar Oluşturuldu mu?

Yeni bir query açın ve şunu çalıştırın:

```sql
USE KutuphaneOtomasyonu;
GO

-- Stored procedure'ları listele
SELECT 
    ROUTINE_NAME AS ProcedureName,
    CREATED AS CreatedDate,
    LAST_ALTERED AS LastModified
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
    AND ROUTINE_NAME IN ('sp_borrow_copy', 'sp_return_copy')
ORDER BY ROUTINE_NAME;
```

**Beklenen Sonuç:**
```
ProcedureName      | CreatedDate        | LastModified
-------------------|--------------------|--------------------
sp_borrow_copy     | 2024-xx-xx ...     | 2024-xx-xx ...
sp_return_copy     | 2024-xx-xx ...     | 2024-xx-xx ...
```

Eğer 2 satır görüyorsanız, başarılı! ✅

---

## 🔍 YAYGIN HATALAR VE ÇÖZÜMLERİ

### Hata 1: "Invalid object name 'dbo.Copies'"
**Sebep:** Tablolar henüz oluşturulmamış  
**Çözüm:** Önce migration'ları çalıştırın:
```bash
cd C:\Users\betul\kutuphane_otomasyonu_vtys2\app
dotnet ef database update
```

### Hata 2: "Must declare the scalar variable"
**Sebep:** Dosyanın bir kısmı seçili ve çalıştırılmış  
**Çözüm:** Tüm dosyayı seçin (Ctrl+A) sonra F5'e basın

### Hata 3: "Cannot connect to server"
**Sebep:** SQL Server çalışmıyor veya yanlış server adı  
**Çözüm:** 
- SQL Server'ın çalıştığından emin olun
- Server adını kontrol edin (appsettings.json'daki ile aynı olmalı)

---

## 📝 HIZLI TEST

Stored procedure'ları test etmek için:

```sql
-- 1. Mevcut bir member_id bulun
SELECT TOP 1 MemberId FROM dbo.Members WHERE Status = 'Active';

-- 2. Mevcut bir available copy_id bulun
SELECT TOP 1 CopyId FROM dbo.Copies WHERE Status = 'Available';

-- 3. sp_borrow_copy'yi test edin (yukarıdaki ID'leri kullanın)
DECLARE @member_id INT = 1;  -- Yukarıdan aldığınız ID
DECLARE @copy_id INT = 1;    -- Yukarıdan aldığınız ID

EXEC dbo.sp_borrow_copy @member_id = @member_id, @copy_id = @copy_id;

-- 4. Sonuç kontrolü
SELECT TOP 1 LoanId, ReturnedAt FROM dbo.Loans 
WHERE MemberId = @member_id AND CopyId = @copy_id 
ORDER BY LoanId DESC;
```

---

## 💡 İPUÇLARI

1. **Her zaman veritabanını seçin:** Query penceresinin üstündeki dropdown'dan
2. **GO komutlarına dikkat:** Dosyadaki her `GO` komutu ayrı bir batch'tir
3. **Hata durumunda:** Messages sekmesini kontrol edin, sadece Results'a bakmayın
4. **Tüm dosyayı çalıştırın:** Parça parça değil, tüm dosyayı seçip çalıştırın

---

## 🎯 BAŞARI KONTROL LİSTESİ

- [ ] SSMS'e başarıyla bağlandım
- [ ] KutuphaneOtomasyonu veritabanını seçtim
- [ ] sp.sql dosyasını açtım
- [ ] F5 ile çalıştırdım
- [ ] "Commands completed successfully" mesajını gördüm
- [ ] Doğrulama sorgusu 2 stored procedure gösterdi
- [ ] Test sorgusu başarıyla çalıştı

Hepsi tamamlandıysa, stored procedure'lar başarıyla oluşturulmuştur! 🎉




