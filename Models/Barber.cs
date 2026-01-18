using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BerberRandevuSistemi.Models;

/// <summary>
/// Sistemde kayıtlı berberleri temsil eden sınıf.
/// </summary>
public class Barber
{
    /// <summary>
    /// Berberin benzersiz kimlik numarası.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Berberin tam adı.
    /// </summary>
    [Required(ErrorMessage = "Ad Soyad zorunludur.")]
    [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Berberin müsaitlik durumu.
    /// </summary>
    [Display(Name = "Müsait")]
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Berberin profil resminin URL'si.
    /// </summary>
    [StringLength(500, ErrorMessage = "Resim URL en fazla 500 karakter olabilir.")]
    [Display(Name = "Resim URL")]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Bu berbere ait randevuların listesi.
    /// </summary>
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
