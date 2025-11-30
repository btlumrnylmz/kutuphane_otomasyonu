# Kütüphane Otomasyonu

Modern, kullanıcı dostu ve profesyonel bir kütüphane yönetim sistemi. ASP.NET Core MVC ve SQL Server ile geliştirilmiştir.

## 📋 Özellikler

### Temel Özellikler
- ✅ **Kitap Yönetimi**: Kitap ekleme, düzenleme, silme, arama ve filtreleme
- ✅ **Kopya Yönetimi**: Fiziksel kopya takibi, durum yönetimi (Available, Loaned, Damaged, Lost)
- ✅ **Üye Yönetimi**: Üye kaydı, profil yönetimi, aktif/pasif durum takibi
- ✅ **Ödünç İşlemleri**: Ödünç verme, iade alma, gecikme takibi
- ✅ **Rezervasyon Sistemi**: Kitap rezervasyonu ve bildirim sistemi
- ✅ **Favori Kitaplar**: Kullanıcıların favori kitaplarını kaydetme
- ✅ **Gecikme Ödemeleri**: Gecikmiş kitaplar için ceza hesaplama ve ödeme takibi

### Raporlama
- 📊 Son 30 günde en çok ödünç alınan 10 kitap
- 📊 Aktif ödünç listesi ve gecikme gün sayısı
- 📊 Üye bazında toplam ödünç sayısı
- 📊 Audit log (ödünç/iade işlem kayıtları)
- 📊 Rezervasyon kuyruğu

### Güvenlik ve Yetkilendirme
- 🔐 Rol tabanlı erişim kontrolü (Admin, User)
- 🔐 Session tabanlı kimlik doğrulama
- 🔐 Şifre hash'leme (SHA256)
- 🔐 Güvenli şifre yönetimi

### Yönetim Özellikleri
- 🛠️ Yedekleme ve geri yükleme (JSON ve SQL Server BAK)
- 🛠️ Veri export/import (Excel, CSV)
- 🛠️ Email bildirimleri (SMTP entegrasyonu)
- 🛠️ Logging sistemi
- 🛠️ Sistem ayarları

## 🚀 Kurulum

### Gereksinimler
- .NET 8.0 SDK
- SQL Server 2019 veya üzeri (veya PostgreSQL)
- Visual Studio 2022 veya VS Code

### Adımlar

1. **Projeyi klonlayın veya indirin**
   ```bash
   git clone <repository-url>
   cd kutuphane_otomasyonu_vtys2
   ```

2. **Veritabanı bağlantı dizesini yapılandırın**
   
   `app/appsettings.json` dosyasını düzenleyin:
   ```json
   {
     "ConnectionStrings": {
       "SqlServer": "Server=YOUR_SERVER;Database=KutuphaneOtomasyonu;Trusted_Connection=True;..."
     }
   }
   ```

3. **Veritabanı migration'larını çalıştırın**
   ```bash
   cd app
   dotnet ef database update
   ```

4. **Programmability objelerini oluşturun**
   
   SQL Server Management Studio'da veya `sqlcmd` ile:
   ```bash
   cd programmability
   sqlcmd -S YOUR_SERVER -d KutuphaneOtomasyonu -E -i triggers.sql
   sqlcmd -S YOUR_SERVER -d KutuphaneOtomasyonu -E -i views.sql
   sqlcmd -S YOUR_SERVER -d KutuphaneOtomasyonu -E -i functions.sql
   sqlcmd -S YOUR_SERVER -d KutuphaneOtomasyonu -E -i sp.sql
   ```
   
   Veya toplu çalıştırma için:
   ```bash
   run_all_programmability.bat
   ```
   (Batch dosyasındaki SERVER_NAME ve DATABASE_NAME değerlerini düzenleyin)

5. **Uygulamayı çalıştırın**
   ```bash
   dotnet run
   ```

6. **Varsayılan admin hesabı ile giriş yapın**
   - Kullanıcı adı: `admin`
   - Şifre: `admin123`
   - ⚠️ **İlk girişten sonra şifreyi değiştirin!**

## 📁 Proje Yapısı

```
kutuphane_otomasyonu_vtys2/
├── app/                          # Ana uygulama
│   ├── Controllers/             # MVC Controller'lar
│   ├── Models/                  # Veri modelleri
│   ├── Views/                   # Razor view'lar
│   ├── Data/                    # DbContext ve SeedData
│   ├── Services/                # İş mantığı servisleri
│   ├── Migrations/              # EF Core migrations
│   └── appsettings.json         # Yapılandırma
├── programmability/             # SQL programmability objeleri
│   ├── triggers.sql            # Trigger'lar
│   ├── views.sql               # View'lar
│   ├── functions.sql           # Function'lar
│   ├── sp.sql                  # Stored Procedure'lar
│   └── run_all_programmability.bat  # Toplu çalıştırma
├── docs/                        # Dokümantasyon
│   └── ERD.md                  # Entity Relationship Diagram
└── README.md                    # Bu dosya
```

## 🔧 Yapılandırma

### Email Ayarları

`appsettings.json` dosyasında email ayarlarını yapılandırın:

```json
{
  "Email": {
    "Enabled": true,
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Kütüphane Otomasyonu"
  }
}
```

**Not:** Gmail kullanıyorsanız, "Uygulama Şifresi" oluşturmanız gerekebilir.

### Logging

Logging seviyesi `appsettings.json` içinde yapılandırılabilir:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## 📊 Veritabanı Yapısı

### Ana Tablolar
- **Books**: Kitap bilgileri (ISBN, başlık, yazar, kategori)
- **Copies**: Fiziksel kopyalar (raf konumu, durum)
- **Members**: Üye bilgileri
- **Loans**: Ödünç kayıtları
- **Users**: Sistem kullanıcıları (Admin/User)
- **Payments**: Gecikme ödemeleri
- **ReturnRequests**: İade talepleri
- **Favorites**: Favori kitaplar
- **Reservations**: Rezervasyonlar
- **Audit_Log**: İşlem logları

### Programmability Objeleri
- **Views**: `vw_active_loans`, `vw_top_books_last30`
- **Functions**: `fn_top_books_between`
- **Stored Procedures**: `sp_borrow_copy`, `sp_return_copy`
- **Triggers**: `tr_loans_audit` (audit log için)

## 🔐 Güvenlik

- Şifreler SHA256 ile hash'lenir
- Session tabanlı kimlik doğrulama
- Rol tabanlı yetkilendirme
- CSRF koruması (AntiForgeryToken)
- SQL Injection koruması (parametreli sorgular)

## 📝 İş Kuralları

- Bir üye aynı anda en fazla **3 kitap** ödünç alabilir
- Ödünç süresi **14 gün**dür
- Sadece `Available` durumundaki kopyalar ödünç verilebilir
- 60 günden fazla gecikmiş ve ödeme yapılmamış kitaplar için yeni ödünç alınamaz
- Gecikme cezası: **5.00 TL/gün**

## 🧪 Test

### Manuel Test Senaryoları

1. **Ödünç Verme**
   - Available kopya seç
   - Üye limiti kontrolü (max 3)
   - Transaction güvenliği

2. **İade Alma**
   - Return request oluştur
   - Admin onayı
   - Kopya durumu güncelleme

3. **Raporlar**
   - Top 10 kitaplar
   - Aktif ödünçler
   - Üye bazında ödünç sayıları

## 📦 Yedekleme

### JSON Yedekleme
- Admin panelinden "Yedekleme" sayfasına gidin
- "JSON Yedekleme İndir" butonuna tıklayın
- Tüm veriler JSON formatında indirilir

### SQL Server Yedekleme (BAK)
- "SQL Server Yedekleme" bölümünden SQL komutunu alın
- SQL Server Management Studio'da çalıştırın
- `.bak` dosyası oluşturulur

### Geri Yükleme
- "Geri Yükleme" sayfasından yedek dosya yolunu girin
- Oluşturulan SQL komutunu SSMS'de çalıştırın

## 🐛 Sorun Giderme

### Veritabanı Bağlantı Hatası
- SQL Server'ın çalıştığından emin olun
- Bağlantı dizesini kontrol edin
- Firewall ayarlarını kontrol edin

### Migration Hatası
```bash
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Programmability Objeleri Hatası
- SQL script'lerini doğru sırayla çalıştırın: triggers → views → functions → sp
- Veritabanı yetkilerini kontrol edin

## 📄 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

## 👥 Katkıda Bulunanlar

- Proje geliştiricileri

## 📞 Destek

Sorularınız için issue açabilir veya iletişime geçebilirsiniz.

---

**Not:** Bu proje, VTYS2 dersi için geliştirilmiş bir kütüphane otomasyon sistemidir. PDF gereksinimlerini karşılamak üzere tasarlanmıştır.




