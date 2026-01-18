using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BerberRandevuSistemi.Models;

/// <summary>
/// Randevu kayıtlarını tutan ana veri modeli.
/// </summary>
public class Appointment
{
    /// <summary>
    /// Randevunun benzersiz kimlik numarası.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Randevuyu alan müşterinin adı.
    /// </summary>
    [Required(ErrorMessage = "Müşteri Adı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Müşteri Adı")]
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Randevuyu alan müşterinin telefon numarası.
    /// </summary>
    [Required(ErrorMessage = "Müşteri Telefonu zorunludur.")]
    [StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir.")]
    [Display(Name = "Müşteri Telefonu")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    public string CustomerPhone { get; set; } = string.Empty;

    /// <summary>
    /// Randevunun tarihi ve saati.
    /// </summary>
    [Required(ErrorMessage = "Randevu Tarihi zorunludur.")]
    [Display(Name = "Randevu Tarihi")]
    [DataType(DataType.DateTime)]
    public DateTime AppointmentDate { get; set; }

    /// <summary>
    /// Seçilen berberin ID'si.
    /// </summary>
    [Required(ErrorMessage = "Berber seçimi zorunludur.")]
    [Display(Name = "Berber")]
    public int BarberId { get; set; }

    /// <summary>
    /// Seçilen hizmetin ID'si.
    /// </summary>
    [Required(ErrorMessage = "Hizmet seçimi zorunludur.")]
    [Display(Name = "Hizmet")]
    public int ServiceId { get; set; }

    /// <summary>
    /// Randevunun oluşturulduğu tarih.
    /// </summary>
    [Display(Name = "Oluşturulma Tarihi")]
    [DataType(DataType.DateTime)]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // Navigation properties
    /// <summary>
    /// Randevunun ilişkili olduğu berber nesnesi.
    /// </summary>
    [ForeignKey("BarberId")]
    public virtual Barber? Barber { get; set; }

    /// <summary>
    /// Randevunun ilişkili olduğu hizmet nesnesi.
    /// </summary>
    [ForeignKey("ServiceId")]
    public virtual Service? Service { get; set; }

    // User Relationship
    /// <summary>
    /// Randevuyu alan kullanıcının ID'si (Opsiyonel).
    /// </summary>
    public int? UserId { get; set; }
    
    /// <summary>
    /// Randevuyu alan kullanıcı nesnesi.
    /// </summary>
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }
}
