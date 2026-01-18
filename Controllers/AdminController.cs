using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BerberRandevuSistemi.Data;
using BerberRandevuSistemi.Models;
using Microsoft.AspNetCore.Authorization; // Güvenlik kütüphanesi

namespace BerberRandevuSistemi.Controllers;

[Authorize] // Sadece giriş yapanlar görebilsin
/// <summary>
/// Yönetim paneli işlemlerini (Randevu, Berber, Hizmet, Kullanıcı yönetimi) kontrol eden sınıf.
/// </summary>
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;

    public AdminController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    // Dashboard - List appointments with filters
    /// <summary>
    /// Yönetim paneli ana sayfası. Randevuları listeler ve filtreleme imkanı sunar.
    /// </summary>
    public async Task<IActionResult> Index(int? barberId, DateTime? date)
    {
        var query = _context.Appointments
            .Include(a => a.Barber)
            .Include(a => a.Service)
            .AsQueryable();

        // Date Filter
        if (!date.HasValue)
        {
            date = DateTime.Today;
        }

        // Tarih filtresi uygula (Sadece tarih kısmı, saat hariç)
        query = query.Where(x => x.AppointmentDate.Date == date.Value.Date);

        // Barber Filter
        if (barberId.HasValue && barberId > 0)
        {
            query = query.Where(x => x.BarberId == barberId);
        }

        // Ordering
        query = query.OrderBy(a => a.AppointmentDate);

        // ViewBag population
        ViewBag.SelectedDate = date.Value.ToString("yyyy-MM-dd");
        ViewBag.SelectedBarberId = barberId;
        ViewBag.Barbers = await _context.Barbers.ToListAsync();

        return View(await query.ToListAsync());
    }

    #region User Management

    // GET: Admin/Users
    /// <summary>
    /// Kayıtlı tüm kullanıcıları listeler.
    /// </summary>
    public async Task<IActionResult> Users()
    {
        var users = await _context.Users.OrderBy(u => u.FullName).ToListAsync();
        return View(users);
    }

    // POST: Admin/DeleteUser/5
    /// <summary>
    /// Seçilen kullanıcıyı ve ilişkili randevularını siler.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            try
            {
                // Önce kullanıcıya ait randevuları sil (FK Çakışmasını önlemek için)
                var appointments = await _context.Appointments.Where(a => a.UserId == id).ToListAsync();
                if (appointments.Any())
                {
                    _context.Appointments.RemoveRange(appointments);
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kullanıcı başarıyla silindi.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Kullanıcı silinirken bir hata oluştu veya bu kullanıcının silinemeyen kayıtları var.";
            }
        }
        else
        {
            TempData["Error"] = "Kullanıcı bulunamadı.";
        }
        return RedirectToAction(nameof(Users));
    }

    #endregion

    #region Barber CRUD

    // GET: Admin/Barbers
    /// <summary>
    /// Tüm berberleri listeler.
    /// </summary>
    public async Task<IActionResult> Barbers()
    {
        return View(await _context.Barbers.ToListAsync());
    }

    // GET: Admin/BarberDetails/5
    /// <summary>
    /// Seçilen berberin detaylarını görüntüler.
    /// </summary>
    public async Task<IActionResult> BarberDetails(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var barber = await _context.Barbers
            .FirstOrDefaultAsync(m => m.Id == id);

        if (barber == null)
        {
            return NotFound();
        }

        return View(barber);
    }

    // GET: Admin/CreateBarber
    /// <summary>
    /// Yeni berber ekleme sayfasını görüntüler.
    /// </summary>
    public IActionResult CreateBarber()
    {
        return View();
    }

    // POST: Admin/CreateBarber
    /// <summary>
    /// Yeni berber kaydını veritabanına ekler. Resim yükleme işlemini yönetir.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBarber([Bind("FullName,IsAvailable")] Barber barber, IFormFile? file)
    {
        if (ModelState.IsValid)
        {
            if (file != null)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string path = Path.Combine(wwwRootPath, "img", "barbers");
                
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                
                barber.ImageUrl = @"/img/barbers/" + fileName;
            }
            else
            {
                // Varsayılan resim ata
                 barber.ImageUrl = "/img/default-barber.png"; 
            }

            _context.Add(barber);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Berber başarıyla eklendi.";
            return RedirectToAction(nameof(Barbers));
        }
        return View(barber);
    }

    // GET: Admin/EditBarber/5
    /// <summary>
    /// Berber düzenleme sayfasını görüntüler.
    /// </summary>
    public async Task<IActionResult> EditBarber(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var barber = await _context.Barbers.FindAsync(id);
        if (barber == null)
        {
            return NotFound();
        }
        return View(barber);
    }

    // POST: Admin/EditBarber/5
    /// <summary>
    /// Berber bilgilerini günceller. Resim değişikliği varsa eski resmi siler ve yenisini yükler.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBarber(int id, Barber barber, IFormFile? file)
    {
        if (id != barber.Id)
        {
            return NotFound();
        }

        // 1. Mevcut veriyi çek (Tracking olmadan)
        // Update işleminde çakışma olmaması için AsNoTracking kullanılır
        var existingBarber = await _context.Barbers.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        
        if (existingBarber == null)
        {
             return NotFound();
        }

        // 2. Dosya Yükleme İşlemi
        if (file != null)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string path = Path.Combine(wwwRootPath, "img", "barbers");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Eski resim varsa temizle (default değilse)
            if (!string.IsNullOrEmpty(existingBarber.ImageUrl))
            {
                var oldPath = Path.Combine(wwwRootPath, existingBarber.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath) && !oldPath.Contains("default-barber"))
                {
                     System.IO.File.Delete(oldPath);
                }
            }

            using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            barber.ImageUrl = @"/img/barbers/" + fileName;
        }
        else
        {
            // Yeni dosya yüklenmediyse eskisini koru
            barber.ImageUrl = existingBarber.ImageUrl;
        }

        // 3. ImageUrl doğrulamasını kaldır (Manuel halledildi)
        ModelState.Remove("ImageUrl");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(barber);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Berber başarıyla güncellendi.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BarberExists(barber.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Barbers));
        }
        return View(barber);
    }

    // GET: Admin/DeleteBarber/5
    /// <summary>
    /// Berber silme onay sayfasını görüntüler.
    /// </summary>
    public async Task<IActionResult> DeleteBarber(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var barber = await _context.Barbers
            .FirstOrDefaultAsync(m => m.Id == id);
        if (barber == null)
        {
            return NotFound();
        }

        return View(barber);
    }

    // POST: Admin/DeleteBarber/5
    /// <summary>
    /// Berberi veritabanından siler.
    /// </summary>
    [HttpPost, ActionName("DeleteBarber")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBarberConfirmed(int id)
    {
        var barber = await _context.Barbers.FindAsync(id);
        if (barber != null)
        {
            _context.Barbers.Remove(barber);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Berber başarıyla silindi.";
        }

        return RedirectToAction(nameof(Barbers));
    }

    private bool BarberExists(int id)
    {
        return _context.Barbers.Any(e => e.Id == id);
    }

    #endregion

    #region Service CRUD

    // GET: Admin/Services
    /// <summary>
    /// Tüm hizmetleri listeler.
    /// </summary>
    public async Task<IActionResult> Services()
    {
        return View(await _context.Services.ToListAsync());
    }

    // GET: Admin/ServiceDetails/5
    /// <summary>
    /// Seçilen hizmetin detaylarını görüntüler.
    /// </summary>
    public async Task<IActionResult> ServiceDetails(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var service = await _context.Services
            .FirstOrDefaultAsync(m => m.Id == id);

        if (service == null)
        {
            return NotFound();
        }

        return View(service);
    }

    // GET: Admin/CreateService
    /// <summary>
    /// Yeni hizmet ekleme sayfasını görüntüler.
    /// </summary>
    public IActionResult CreateService()
    {
        return View();
    }

    // POST: Admin/CreateService
    /// <summary>
    /// Yeni hizmet kaydını oluşturur.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateService([Bind("ServiceName,Price,DurationMinutes,Description")] Service service)
    {
        if (ModelState.IsValid)
        {
            _context.Add(service);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Hizmet başarıyla eklendi.";
            return RedirectToAction(nameof(Services));
        }
        return View(service);
    }

    // GET: Admin/EditService/5
    /// <summary>
    /// Hizmet düzenleme sayfasını görüntüler.
    /// </summary>
    public async Task<IActionResult> EditService(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var service = await _context.Services.FindAsync(id);
        if (service == null)
        {
            return NotFound();
        }
        return View(service);
    }

    // POST: Admin/EditService/5
    /// <summary>
    /// Hizmet bilgilerini günceller.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditService(int id, [Bind("Id,ServiceName,Price,DurationMinutes,Description")] Service service)
    {
        if (id != service.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(service);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Hizmet başarıyla güncellendi.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceExists(service.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Services));
        }
        return View(service);
    }

    // GET: Admin/DeleteService/5
    /// <summary>
    /// Hizmet silme onay sayfasını görüntüler.
    /// </summary>
    public async Task<IActionResult> DeleteService(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var service = await _context.Services
            .FirstOrDefaultAsync(m => m.Id == id);
        if (service == null)
        {
            return NotFound();
        }

        return View(service);
    }

    // POST: Admin/DeleteService/5
    /// <summary>
    /// Hizmeti siler. Önce ilişkili randevu olup olmadığını kontrol eder.
    /// </summary>
    [HttpPost, ActionName("DeleteService")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteServiceConfirmed(int id)
    {
        // İlişkili randevu kontrolü (FK Hatasını önlemek için)
        var hasAppointments = await _context.Appointments.AnyAsync(a => a.ServiceId == id);
        if (hasAppointments)
        {
            TempData["Error"] = "Bu hizmete ait kayıtlı randevular bulunduğu için silme işlemi gerçekleştirilemez. Önce ilgili randevuları silmelisiniz.";
            return RedirectToAction(nameof(Services));
        }

        var service = await _context.Services.FindAsync(id);
        if (service != null)
        {
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Hizmet başarıyla silindi.";
        }

        return RedirectToAction(nameof(Services));
    }

    private bool ServiceExists(int id)
    {
        return _context.Services.Any(e => e.Id == id);
    }

    #endregion
}