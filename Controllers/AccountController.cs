using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BerberRandevuSistemi.Data;
using BerberRandevuSistemi.Models;

namespace BerberRandevuSistemi.Controllers;

/// <summary>
/// Kullanıcı işlemleri (Giriş, Kayıt, Profil, Çıkış) için kontrolcü sınıfı.
/// </summary>
public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    /// <summary>
    /// Giriş sayfasını görüntüler.
    /// </summary>
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// Kullanıcı girişi işlemini doğrular ve oturum başlatır.
    /// </summary>
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        // Veritabanından kullanıcıyı sorgula
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

        if (user != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // 1. Check for ReturnUrl (e.g. user came from Booking page)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            // 2. Standard Redirect
            return RedirectToAction("Index", "Home");
        }

        ViewData["Error"] = "Geçersiz e-posta veya şifre.";
        return View();
    }

    [HttpGet]
    /// <summary>
    /// Kayıt olma sayfasını görüntüler.
    /// </summary>
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// Yeni kullanıcı kaydı oluşturur.
    /// </summary>
    public async Task<IActionResult> Register([Bind("FullName,Email,Password")] User user)
    {
        if (ModelState.IsValid)
        {
            // E-posta adresi kullanımda mı kontrol et
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
                return View(user);
            }

            user.Role = "Member"; // Varsayılan rol: Üye
            _context.Add(user);
            await _context.SaveChangesAsync();

            // Auto login after register (optional, but good UX)
            // For now, let's redirect to Login
            TempData["Success"] = "Kayıt başarılı! Lütfen giriş yapınız.";
            return RedirectToAction(nameof(Login));
        }
        return View(user);
    }

    /// <summary>
    /// Kullanıcı oturumunu sonlandırır.
    /// </summary>
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    /// <summary>
    /// Kullanıcının randevularını listeler. Geçmiş randevuları otomatik temizler atar.
    /// </summary>
    public async Task<IActionResult> MyAppointments()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user == null) return RedirectToAction("Login");

        // Otomatik Temizleme: Geçmiş randevuları sil
        var pastAppointments = _context.Appointments
            .Where(a => a.UserId == user.Id && a.AppointmentDate < DateTime.Now);
        
        if (pastAppointments.Any())
        {
            _context.Appointments.RemoveRange(pastAppointments);
            await _context.SaveChangesAsync();
        }

        var appointments = await _context.Appointments
            .Include(a => a.Barber)
            .Include(a => a.Service)
            .Where(a => a.UserId == user.Id)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        return View(appointments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize]
    /// <summary>
    /// Belirtilen randevuyu iptal eder (siler). Sadece gelecek tarihli randevular iptal edilebilir.
    /// </summary>
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user == null) return RedirectToAction("Login");

        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment != null && appointment.UserId == user.Id)
        {
            // Opsiyonel: Geçmiş randevu kontrolü
            if (appointment.AppointmentDate > DateTime.Now)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Randevunuz başarıyla iptal edildi.";
            }
            else
            {
                TempData["Error"] = "Geçmiş randevular iptal edilemez.";
            }
        }
        else
        {
            TempData["Error"] = "Randevu bulunamadı veya size ait değil.";
        }

        return RedirectToAction(nameof(MyAppointments));
    }
    
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet]
    /// <summary>
    /// Kullanıcı profil sayfasını görüntüler.
    /// </summary>
    public async Task<IActionResult> Profile()
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user == null) return RedirectToAction("Login");

        return View(user);
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// Kullanıcı profil bilgilerini (Ad Soyad, Telefon) günceller.
    /// </summary>
    public async Task<IActionResult> Profile([Bind("Id,FullName,Email,PhoneNumber")] User model)
    {
        // Email should generally not change or needs re-verification. 
        // For simplicity here, we assume email is read-only in view or handled carefully.
        // We re-fetch user to update only allowed fields.
        var user = await _context.Users.FindAsync(model.Id);
        if (user == null) return RedirectToAction("Login");

        // Simple validation or just update
        user.FullName = model.FullName;
        user.PhoneNumber = model.PhoneNumber;

        _context.Update(user);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Profil bilgileriniz güncellendi.";
        return RedirectToAction(nameof(Profile));
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    /// <summary>
    /// Kullanıcı şifresini değiştirir.
    /// </summary>
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(userEmail)) return RedirectToAction("Login");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user == null) return RedirectToAction("Login");

        if (newPassword != confirmPassword)
        {
             TempData["PasswordError"] = "Yeni şifreler eşleşmiyor.";
             return RedirectToAction(nameof(Profile));
        }

        // Verify current password (plain text for this demo, usually hashed)
        if (user.Password != currentPassword)
        {
            TempData["PasswordError"] = "Mevcut şifreniz yanlış.";
            return RedirectToAction(nameof(Profile));
        }

        user.Password = newPassword;
        _context.Update(user);
        await _context.SaveChangesAsync();

        TempData["PasswordSuccess"] = "Şifreniz başarıyla değiştirildi.";
        return RedirectToAction(nameof(Profile));
    }
}
