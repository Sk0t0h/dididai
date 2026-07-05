using System.ComponentModel.DataAnnotations;
using DididaiApp.Core.Models.Validation;

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

    /// <summary>
    /// Tipo de documento declarado por el socio. Decide cómo se valida <see cref="Dni"/>
    /// (DNI/NIE con letra; pasaporte/otro laxo). Independiente del país de residencia.
    /// </summary>
    [Required]
    [Display(Name = "Tipo de documento")]
    public TipoDocumento TipoDocumento { get; set; } = TipoDocumento.DniEspanol;

    /// <summary>
    /// Número/código del documento de identidad. Se valida según <see cref="TipoDocumento"/>.
    /// Es la clave única del socio (dos socios con el mismo documento son la misma persona).
    /// </summary>
    [Required]
    [StringLength(20)]
    [Display(Name = "Documento de identidad")]
    [DocumentoPorTipo(nameof(TipoDocumento))]
    public string Dni { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    [Display(Name = "Teléfono")]
    [TelefonoE164]
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

    /// <summary>
    /// País de residencia del socio, en código ISO 3166-1 alpha-2 (<c>ES</c>, <c>GB</c>…).
    /// Es el domicilio, NO la nacionalidad: no decide la validación del documento (eso lo
    /// hace <see cref="TipoDocumento"/>). El nombre para mostrar se resuelve con
    /// <see cref="Paises.Nombre"/>. Único sitio donde vive el país; NO el idioma de la UI.
    /// </summary>
    [Required(ErrorMessage = "Selecciona un país de residencia de la lista.")]
    [StringLength(2, MinimumLength = 2, ErrorMessage = "Selecciona un país de residencia válido de la lista.")]
    [Display(Name = "País de residencia")]
    public string PaisResidencia { get; set; } = Paises.CodigoPorDefecto;

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
