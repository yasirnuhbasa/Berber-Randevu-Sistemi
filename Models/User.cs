using System.ComponentModel.DataAnnotations;

namespace BerberRandevuSistemi.Models;

/// <summary>
/// Sistemdeki kullanıcıları (Müşteri ve Yönetici) temsil eden sınıf.
/// </summary>
public class User
{
    /// <summary>
    /// Kullanıcının benzersiz kimlik numarası.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Kullanıcının tam adı (Ad Soyad).
    /// </summary>
    [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
    [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Kullanıcının e-posta adresi.
    /// </summary>
    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(100, ErrorMessage = "E-posta en fazla 100 karakter olabilir.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Kullanıcının iletişim numarası.
    /// </summary>
    [Display(Name = "Telefon Numarası")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir.")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Kullanıcının giriş şifresi.
    /// </summary>
    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Kullanıcının rolü (Admin veya Member).
    /// </summary>
    [Required]
    [Display(Name = "Rol")]
    public string Role { get; set; } = "Member"; // Admin or Member
}
