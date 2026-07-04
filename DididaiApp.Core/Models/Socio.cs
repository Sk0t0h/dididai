using System.ComponentModel.DataAnnotations;

namespace DididaiApp.Core.Models;

/// <summary>
/// Persona colaboradora de la ONG. Representa la identidad estable del socio
/// (datos personales, contacto y domicilio). Las formas concretas de aportación
/// económica se modelan aparte en <see cref="Colaboracion"/> (relación 1:N),
/// de modo que un mismo socio puede tener varias colaboraciones a lo largo del
/// tiempo (cuota domiciliada, aportación única, Teaming, etc.).
/// </summary>
public class Socio
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Apellidos { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Dni { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(30)]
    public string Telefono { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Direccion { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string CodigoPostal { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Localidad { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Pais { get; set; } = string.Empty;

    /// <summary>Consentimiento de la política de privacidad (RGPD).</summary>
    public bool AceptaPrivacidad { get; set; }

    /// <summary>Fecha de alta como colaborador. La fija la aplicación al crear el registro.</summary>
    public DateTime FechaAlta { get; set; }

    /// <summary>
    /// Fecha de baja del socio (borrado lógico). <c>null</c> = socio activo; con
    /// valor = el socio se ha dado de baja del todo y no aparece en el listado por
    /// defecto, pero se conserva el registro (trazabilidad, RGPD). No debe
    /// confundirse con la baja de una <see cref="Colaboracion"/> concreta (dejar de
    /// pagar una cuota), que vive en la propia colaboración.
    /// </summary>
    public DateTime? FechaBaja { get; set; }

    /// <summary>Indica si el socio está activo (no dado de baja). Derivado de <see cref="FechaBaja"/>.</summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool Activo => FechaBaja is null;

    /// <summary>Colaboraciones (formas de aportación) asociadas a este socio.</summary>
    public ICollection<Colaboracion> Colaboraciones { get; set; } = new List<Colaboracion>();
}
