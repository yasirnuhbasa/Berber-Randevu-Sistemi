using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BerberRandevuSistemi.Models;
using BerberRandevuSistemi.Data;
using System.Globalization;

namespace BerberRandevuSistemi.Controllers;

/// <summary>
/// Ana sayfa ve genel kullanıcı işlemlerini (Randevu alma vb.) yöneten kontrolcü.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Ana sayfa. Hizmetleri ve öne çıkan berberleri listeler.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var services = await _context.Services.ToListAsync();
        var featuredBarbers = await _context.Barbers
            .Where(b => b.IsAvailable)
            .Take(4)
            .ToListAsync();

        ViewBag.Services = services;
        ViewBag.FeaturedBarbers = featuredBarbers;

        return View();
    }

    // GET: Home/BookAppointment
    /// <summary>
    /// Randevu alma sayfasını görüntüler.
    /// </summary>
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> BookAppointment()
    {
        ViewBag.Barbers = await _context.Barbers
            .Where(b => b.IsAvailable)
            .OrderBy(b => b.FullName)
            .ToListAsync();

        ViewBag.Services = await _context.Services
            .OrderBy(s => s.ServiceName)
            .ToListAsync();

        var appointment = new Appointment();
        // Auto-fill logged in user info
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        
        if (!string.IsNullOrEmpty(userEmail))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user != null)
            {
                appointment.CustomerName = user.FullName;
            }
        }

        return View(appointment);
    }

    // POST: Home/BookAppointment
    /// <summary>
    /// Randevu oluşturma işlemini gerçekleştirir. İş kurallarını (çakışma, mesai saatleri vb.) kontrol eder.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> BookAppointment([Bind("CustomerName,CustomerPhone,AppointmentDate,BarberId,ServiceId")] Appointment appointment)
    {
        // 1. Working Hours Validation
        // Pazar günü kontrolü
        if (appointment.AppointmentDate.DayOfWeek == DayOfWeek.Sunday)
        {
            ModelState.AddModelError("AppointmentDate", "Pazar günleri kapalıyız.");
        }

        // Mesai saatleri (10:00 - 22:00) kontrolü
        TimeSpan time = appointment.AppointmentDate.TimeOfDay;
        if (time < new TimeSpan(10, 0, 0) || time > new TimeSpan(22, 0, 0))
        {
            ModelState.AddModelError("AppointmentDate", "Çalışma saatlerimiz 10:00 - 22:00 arasındadır.");
        }

        if (ModelState.IsValid)
        {
            // 2. Conflict Detection
            var service = await _context.Services.FindAsync(appointment.ServiceId);
            if (service != null)
            {
                var duration = service.DurationMinutes;
                var newStart = appointment.AppointmentDate;
                var newEnd = newStart.AddMinutes(duration);

                // Kapanış saati kontrolü (Kural A)
                var closingTime = newStart.Date.AddHours(22);
                if (newEnd > closingTime)
                {
                    ModelState.AddModelError("", "Seçilen işlem çalışma saatleri içinde bitmiyor. (Kapanış: 22:00)");
                }
                else
                {
                    // Çakışma kontrolü (Kural B)
                    // Mantık: (Yeni Başlangıç < Mevcut Bitiş) VE (Yeni Bitiş > Mevcut Başlangıç)
                    var conflict = await _context.Appointments
                        .Include(a => a.Service)
                        .Where(a => a.BarberId == appointment.BarberId)
                        .Where(a => a.AppointmentDate < newEnd && 
                                    a.AppointmentDate.AddMinutes(a.Service.DurationMinutes) > newStart)
                        .FirstOrDefaultAsync();

                    if (conflict != null)
                    {
                        var conflictEnd = conflict.AppointmentDate.AddMinutes(conflict.Service.DurationMinutes);
                        ModelState.AddModelError("", $"Seçilen berber bu saat aralığında ({conflict.AppointmentDate:HH:mm} - {conflictEnd:HH:mm}) başka bir işlemde. Lütfen başka bir saat seçiniz.");
                    }
                    else
                    {
                        // Kullanıcı oturumu açık ise ID'yi bağla
                        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                        bool canBook = true;

                        if (!string.IsNullOrEmpty(userEmail))
                        {
                            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                            if (user != null)
                            {
                                appointment.UserId = user.Id;

                                // Günlük Limit Kontrolü: Günde en fazla 1 randevu
                                var dailyCount = await _context.Appointments
                                    .CountAsync(a => a.UserId == user.Id && a.AppointmentDate.Date == appointment.AppointmentDate.Date);

                                if (dailyCount > 0)
                                {
                                    ModelState.AddModelError("", "Üzgünüz, bir günde en fazla 1 adet randevu alabilirsiniz.");
                                    canBook = false;
                                }
                            }
                        }

                        if (canBook)
                        {
                            appointment.CreatedDate = DateTime.Now;
                            _context.Add(appointment);
                            await _context.SaveChangesAsync();
                            TempData["Success"] = "Randevunuz başarıyla oluşturuldu! Teşekkür ederiz.";
                            return RedirectToAction("MyAppointments", "Account");
                        }
                    }
                }
            }
        }

        ViewBag.Barbers = await _context.Barbers
            .Where(b => b.IsAvailable)
            .OrderBy(b => b.FullName)
            .ToListAsync();

        ViewBag.Services = await _context.Services
            .OrderBy(s => s.ServiceName)
            .ToListAsync();

        return View(appointment);
    }

    [HttpGet]
    /// <summary>
    /// Seçilen tarih, berber ve hizmete göre uygun saat dilimlerini hesaplar ve döndürür.
    /// </summary>
    public async Task<IActionResult> GetAvailableTimeSlots(int barberId, int serviceId, string dateString)
    {
        if (barberId == 0 || serviceId == 0 || string.IsNullOrEmpty(dateString))
        {
             return Json(new { hasExistingAppointment = false, slots = new List<object>() });
        }

        // Adım 1: Tarihi ayrıştır
        if (!DateTime.TryParseExact(dateString, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedDate))
        {
             return Json(new { hasExistingAppointment = false, slots = new List<object>() });
        }

        // Giriş yapmış kullanıcının o gün başka randevusu var mı kontrol et
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(userEmail))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user != null)
            {
                 var hasExisting = await _context.Appointments
                    .AnyAsync(a => a.UserId == user.Id && a.AppointmentDate.Date == selectedDate.Date);

                 if (hasExisting)
                 {
                     // Eğer varsa direkt true döndür (Frontend uyarı gösterecek)
                     return Json(new { hasExistingAppointment = true, slots = new List<object>() });
                 }
            }
        }

        // Pazar günü kontrolü
        if (selectedDate.DayOfWeek == DayOfWeek.Sunday)
        {
             return Json(new { hasExistingAppointment = false, slots = new List<object>() });
        }

        // Hizmet ve süresini getir
        var service = await _context.Services.FindAsync(serviceId);
        if (service == null) return Json(new { hasExistingAppointment = false, slots = new List<object>() });
        var duration = service.DurationMinutes;

        // O günkü mevcut randevuları getir
        var existingAppointments = await _context.Appointments
            .Include(a => a.Service)
            .Where(a => a.BarberId == barberId && a.AppointmentDate.Date == selectedDate.Date)
            .ToListAsync();

        var slots = new List<object>();

        // Başlangıç ve Bitiş Saatleri (10:00 - 22:00)
        TimeSpan startHour = new TimeSpan(10, 0, 0);
        TimeSpan endHour = new TimeSpan(22, 0, 0);
        
        DateTime currentSlot = selectedDate.Date.Add(startHour);
        DateTime endTime = selectedDate.Date.Add(endHour);

        // Döngü: 15 dakikalık aralıklarla kontrol et
        while (currentSlot < endTime)
        {
            DateTime proposedStart = currentSlot;
            DateTime proposedEnd = currentSlot.AddMinutes(duration);
            
            bool isAvailable = true;
            string reason = ""; // Debug için

            // Kontrol 1: Geçmiş zaman kontrolü (Eğer bugün ise)
            if (selectedDate.Date == DateTime.Today && proposedStart < DateTime.Now)
            {
                isAvailable = false;
                reason = "Past time";
            }

            // Kontrol 2: Kapanış saati kontrolü
            if (isAvailable && proposedEnd > endTime)
            {
                isAvailable = false;
                reason = "Exceeds closing time";
            }

            // Kontrol 3: Çakışma kontrolü
            if (isAvailable)
            {
                foreach (var existing in existingAppointments)
                {
                    // Mevcut randevu aralığı
                    DateTime existingStart = existing.AppointmentDate;
                    DateTime existingEnd = existingStart.AddMinutes(existing.Service.DurationMinutes);

                    // Çakışma Formülü: (StartA < EndB) && (EndA > StartB)
                    if (proposedStart < existingEnd && proposedEnd > existingStart)
                    {
                        isAvailable = false;
                        reason = "Collision with existing appointment";
                        break;
                    }
                }
            }

            // Listeye ekle
            slots.Add(new
            {
                time = proposedStart.ToString("HH:mm"),
                isAvailable = isAvailable,
                debugWait = reason
            });

            // Bir sonraki dilime geç
            currentSlot = currentSlot.AddMinutes(15);
        }

        return Json(new { hasExistingAppointment = false, slots = slots });
    }

    /// <summary>
    /// Gizlilik politikası sayfasını görüntüler.
    /// </summary>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Hata sayfasını görüntüler.
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
