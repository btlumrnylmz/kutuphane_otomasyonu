using KutuphaneOtomasyonu.Data;
using KutuphaneOtomasyonu.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;

// Serilog yapılandırması
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/kutuphane-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Kütüphane Otomasyonu başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog'u kullan
    builder.Host.UseSerilog();

    // Logging
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Information);

// MVC
builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<LibraryContext>();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Memory Cache
builder.Services.AddMemoryCache();

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<CacheService>();
builder.Services.AddScoped<RateLimitingService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Hata sayfası ve statik dosyalar
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Status code sayfaları
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

app.UseStaticFiles();

app.UseSession();

app.UseRouting();

// Varsayılan route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    
    KutuphaneOtomasyonu.Data.SeedData.Initialize(db);
    authService.CreateDefaultAdminAsync().Wait();
    
    // Yüklenen resimlerden kitapları ekle
    KutuphaneOtomasyonu.Data.SeedData.AddUploadedBooksFromImages(db, env);
}

Log.Information("Kütüphane Otomasyonu başarıyla başlatıldı.");

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama başlatılırken kritik hata oluştu.");
}
finally
{
    Log.CloseAndFlush();
}


