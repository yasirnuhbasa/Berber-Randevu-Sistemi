using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BerberRandevuSistemi.Models;

/// <summary>
/// Sunulan berber hizmetlerini (Saç kesimi, sakal vb.) temsil eder.
/// </summary>
public class Service
{
    /// <summary>
    /// Hizmetin benzersiz kimlik numarası.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Hizmetin adı (Örn: Saç Kesimi).
    /// </summary>
    [Required(ErrorMessage = "Hizmet Adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Hizmet Adı en fazla 100 karakter olabilir.")]
    [Display(Name = "Hizmet Adı")]
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Hizmetin fiyatı.
    /// </summary>
    [Required(ErrorMessage = "Fiyat zorunludur.")]
    [DataType(DataType.Currency)]
    [Display(Name = "Fiyat")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    /// <summary>
    /// Hizmetin süresi (dakika cinsinden).
    /// </summary>
    [Required(ErrorMessage = "Süre zorunludur.")]
    [Display(Name = "Süre (Dakika)")]
    [Range(1, int.MaxValue, ErrorMessage = "Süre 1 dakikadan az olamaz.")]
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Hizmet hakkında detaylı açıklama.
    /// </summary>
    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    // Navigation property
    /// <summary>
    /// Bu hizmeti içeren randevuların listesi.
    /// </summary>
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
