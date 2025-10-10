using KutuphaneOtomasyonu.Data;
using KutuphaneOtomasyonu.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

// Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Hata sayfası ve statik dosyalar
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
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
    
    KutuphaneOtomasyonu.Data.SeedData.Initialize(db);
    authService.CreateDefaultAdminAsync().Wait();
}

app.Run();


