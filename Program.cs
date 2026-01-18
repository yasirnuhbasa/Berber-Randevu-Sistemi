using Microsoft.EntityFrameworkCore;
using BerberRandevuSistemi.Data;
using BerberRandevuSistemi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

// --- SQL SERVER BAĞLANTISI ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ============================================================
// VERİTABANI SIFIRLAMA VE ADMİN OLUŞTURMA (TEK SEFERLİK)
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // 1. ADIM: Veritabanını SIFIRLA (Yeni tabloların gelmesi için şart)
        // NOT: Proje tamamen bitince bu satırı yorum satırı yapabilirsin.
        // context.Database.EnsureDeleted(); // <-- PERSIST DATA: Do not wipe database
        context.Database.EnsureCreated();

        Console.WriteLine("--> Veritabanı kontrol edildi.");

        // 2. ADIM: Varsayılan Admin Yoksa Ekle (Seed Data)
        // Eğer Users tablosu boşsa, otomatik bir Admin ekler.
        if (!context.Users.Any())
        {
            var adminUser = new BerberRandevuSistemi.Models.User
            {
                FullName = "Sistem Yöneticisi",
                Email = "admin@berber.com",
                Password = "123", // Gerçek projede şifrelenmelidir
                Role = "Admin"
            };
            
            context.Users.Add(adminUser);
            context.SaveChanges();
            Console.WriteLine("--> Varsayılan Admin Eklendi: admin@berber.com / 123");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı işlemleri sırasında hata oluştu.");
    }
}
// ============================================================

app.Run();