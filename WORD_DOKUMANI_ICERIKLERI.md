# WORD DÖKÜMANI İÇERİKLERİ - TABLO FORMATINDA

## 1. AKIŞ DİYAGRAMLARI

### Ödünç Verme Akışı

```
1. BAŞLA
2. Üye ID ve Kopya ID al
3. Transaction başlat
4. Kopya kontrolü:
   - Kopya mevcut mu? → HAYIR → Hata: "Kopya bulunamadı" → Transaction ROLLBACK → BİTİR
   - Kopya durumu 'Available' mı? → HAYIR → Hata: "Kopya müsait değil" → Transaction ROLLBACK → BİTİR
5. Üye kontrolü:
   - Üye mevcut mu? → HAYIR → Hata: "Üye bulunamadı" → Transaction ROLLBACK → BİTİR
   - Üye durumu 'Active' mı? → HAYIR → Hata: "Üye aktif değil" → Transaction ROLLBACK → BİTİR
6. Aktif ödünç kontrolü:
   - Üyenin aktif ödünç sayısı >= 3 mü? → EVET → Hata: "Ödünç limiti dolmuş" → Transaction ROLLBACK → BİTİR
7. Loan kaydı oluştur:
   - LoanedAt = Şimdiki zaman
   - DueAt = LoanedAt + 14 gün
   - ReturnedAt = NULL
8. Kopya durumunu 'Loaned' yap
9. Transaction COMMIT
10. Başarı mesajı döndür
11. BİTİR
```

### İade Alma Akışı

```
1. BAŞLA
2. Loan ID al
3. Transaction başlat
4. Loan kaydı kontrolü:
   - Loan mevcut mu? → HAYIR → Hata: "Ödünç kaydı bulunamadı" → Transaction ROLLBACK → BİTİR
   - Loan zaten iade edilmiş mi? → EVET → Hata: "Zaten iade edilmiş" → Transaction ROLLBACK → BİTİR
5. ReturnedAt alanını güncelle:
   - ReturnedAt = Şimdiki zaman (Trigger tetiklenecek - Audit log kaydı oluşacak)
6. Kopya durumunu 'Available' yap
7. Transaction COMMIT
8. Başarı mesajı döndür
9. BİTİR
```

---

## 2. SAKLI YORDAMLAR TABLOSU

| Yordam Adı | Parametreler | İş Mantığı Özeti | Hata Durumları |
|------------|--------------|------------------|----------------|
| **sp_borrow_copy** | @member_id INT<br>@copy_id INT | 1. Kopyanın mevcut ve 'Available' durumunda olduğunu kontrol eder<br>2. Üyenin mevcut ve 'Active' durumunda olduğunu kontrol eder<br>3. Üyenin aktif ödünç sayısını kontrol eder (maksimum 3)<br>4. Tüm kontroller başarılıysa:<br>   - Loan kaydı oluşturur (DueAt = LoanedAt + 14 gün)<br>   - Kopya durumunu 'Loaned' olarak günceller<br>5. Transaction içinde atomik işlem yapar | • Kopya bulunamadı veya müsait değil<br>• Üye bulunamadı veya aktif değil<br>• Üyenin aktif ödünç limiti dolmuş (maksimum 3)<br>• Transaction hataları (rollback) |
| **sp_return_copy** | @loan_id INT | 1. Loan kaydının mevcut olduğunu kontrol eder<br>2. Loan kaydının daha önce iade edilmediğini kontrol eder<br>3. ReturnedAt alanını güncel tarih/saat ile günceller (trigger tetiklenecek)<br>4. Kopya durumunu 'Available' olarak günceller<br>5. Transaction içinde atomik işlem yapar | • Ödünç kaydı bulunamadı<br>• Bu ödünç kaydı zaten iade edilmiş<br>• Transaction hataları (rollback) |

---

## 3. GÖRÜNÜMLER (VIEWS) TABLOSU

| Görünüm Adı | Sütunlar | Kaynak Sorgu (kısa) | Kullanım |
|-------------|----------|---------------------|----------|
| **vw_active_loans** | member_name (NVARCHAR)<br>book_title (NVARCHAR)<br>due_at (DATETIME2)<br>delay_days (INT)<br>LoanId (INT)<br>LoanedAt (DATETIME2)<br>ShelfLocation (NVARCHAR) | SELECT m.FullName AS member_name,<br>b.Title AS book_title,<br>l.DueAt AS due_at,<br>CASE WHEN l.DueAt < GETUTCDATE() <br>THEN DATEDIFF(DAY, l.DueAt, GETUTCDATE()) <br>ELSE 0 END AS delay_days,<br>l.LoanId, l.LoanedAt, c.ShelfLocation<br>FROM Loans l<br>INNER JOIN Members m ON m.MemberId = l.MemberId<br>INNER JOIN Copies c ON c.CopyId = l.CopyId<br>INNER JOIN Books b ON b.BookId = c.BookId<br>WHERE l.ReturnedAt IS NULL | Aktif ödünçleri listelemek için kullanılır. Üye adı, kitap başlığı, vade tarihi ve gecikme gün sayısını gösterir. Admin panelinde ve raporlama sayfasında kullanılır. |
| **vw_top_books_last30** | title (NVARCHAR)<br>borrow_count (INT) | SELECT TOP (10)<br>b.Title AS title,<br>COUNT(*) AS borrow_count<br>FROM Loans l<br>INNER JOIN Copies c ON c.CopyId = l.CopyId<br>INNER JOIN Books b ON b.BookId = c.BookId<br>WHERE l.LoanedAt >= DATEADD(DAY, -30, GETUTCDATE())<br>GROUP BY b.Title<br>ORDER BY COUNT(*) DESC, b.Title ASC | Son 30 günde en çok ödünç alınan ilk 10 kitabı listeler. Raporlama sayfasında kullanılır. |

---

## 4. RAPOR 1 — SON 30 GÜN TOP 10 KİTAP TABLOSU

| Sorgu | Filtre | Çıktı Alanları | Örnek Sonuç |
|-------|--------|----------------|-------------|
| SELECT TOP (10)<br>b.Title AS title,<br>COUNT(*) AS borrow_count<br>FROM dbo.Loans l<br>INNER JOIN dbo.Copies c ON c.CopyId = l.CopyId<br>INNER JOIN dbo.Books b ON b.BookId = c.BookId<br>WHERE l.LoanedAt >= DATEADD(DAY, -30, GETUTCDATE())<br>GROUP BY b.Title<br>ORDER BY COUNT(*) DESC, b.Title ASC | • l.LoanedAt >= Son 30 gün içinde<br>• Sadece ödünç alınan kitaplar (iade edilmiş/edilmemiş fark etmez) | • title: Kitap başlığı (NVARCHAR)<br>• borrow_count: Ödünç sayısı (INT) | **Örnek 1:**<br>title: "Suç ve Ceza"<br>borrow_count: 15<br><br>**Örnek 2:**<br>title: "Simyacı"<br>borrow_count: 12<br><br>**Örnek 3:**<br>title: "Beyaz Zambaklar Ülkesinde"<br>borrow_count: 10<br><br>**Not:** En çok ödünç alınan kitap en üstte, aynı sayıda ödünç alınan kitaplarda alfabetik sıralama yapılır. |

---

## EK BİLGİLER

### FUNCTION (Fonksiyon)

**fn_top_books_between**
- **Tip:** Inline Table-Valued Function
- **Parametreler:** @start_date DATETIME2, @end_date DATETIME2
- **Açıklama:** Belirtilen tarih aralığında ödünç alınan kitapları, ödünç sayısına göre gruplar ve döndürür.
- **Kullanım:** SELECT * FROM fn_top_books_between('2024-01-01', '2024-12-31') ORDER BY borrow_count DESC;

### TRIGGER (Tetikleyici)

**tr_loans_audit**
- **Tablo:** Loans
- **Tip:** AFTER INSERT, UPDATE
- **Açıklama:** 
  - INSERT işleminde: Yeni ödünç kaydı için Audit_Log tablosuna 'BORROW' kaydı ekler
  - UPDATE işleminde: ReturnedAt alanı NULL'dan değerli bir tarihe güncellenirse 'RETURN' kaydı ekler
- **Amaç:** Tüm ödünç verme ve iade alma işlemlerini otomatik olarak loglar.

