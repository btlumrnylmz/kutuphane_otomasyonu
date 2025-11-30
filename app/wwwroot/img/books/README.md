# Kitap Kapak Resimleri

Bu klasör kitap kapak resimlerini tutar.

## Kullanım

1. **Resim Yükleme:**
   - Kitap kapak resimlerinizi bu klasöre yükleyin
   - Desteklenen formatlar: `.jpg`, `.jpeg`, `.png`
   - Önerilen boyut: 400x600 piksel veya benzer oran

2. **Otomatik Kitap Ekleme:**
   - Resimleri yükledikten sonra, admin panelinden "Resimlerden Kitapları Ekle" butonuna tıklayın
   - Sistem otomatik olarak resim dosya adlarından kitap bilgilerini çıkarır
   - Her resim için bir kitap ve en az 1 kopya oluşturulur

3. **Manuel Kitap Ekleme (Gelişmiş):**
   - Resim dosya adlarını ve kitap bilgilerini eşleştirmek için `SeedData.AddBooksFromImageMapping()` metodunu kullanabilirsiniz
   - Bu yöntem daha doğru sonuçlar verir

## Örnek Resim Dosya Adları

- `kurk-mantolu-madonna-sabahattin-ali-roman.jpg`
- `simyaci-paulo-coelho-kisisel-gelisim.jpg`
- `suc-ve-ceza-dostoyevski-roman.jpg`

**Not:** Dosya adlarından kitap bilgileri çıkarılır. Daha doğru sonuçlar için manuel eşleştirme yapmanız önerilir.

