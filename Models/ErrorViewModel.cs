namespace BerberRandevuSistemi.Models;

/// <summary>
/// Hata sayfalarında görüntülenen model.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Hata isteğinin benzersiz kimliği.
    /// </summary>
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
