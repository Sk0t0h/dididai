using System.ComponentModel.DataAnnotations;
using DididaiApp.Core.Models.Validation;

namespace DididaiApp.Core.Models;

/// <summary>
/// Tipo de colaboración que solicita una persona desde el formulario público.
/// Es una <em>intención</em>, no una <see cref="Colaboracion"/> real: el admin la
/// revisa y, al aprobarla, decide el subtipo concreto (y pide el IBAN si procede).
/// </summary>
public enum TipoColaboracionSolicitada
{
    [Display(Name = "Hacerme socio/a")]
    Socio = 0,

    [Display(Name = "Donación")]
    Donacion = 1,

    [Display(Name = "Microdonación (Teaming)")]
    Microdonacion = 2,
}

/// <summary>Estado de revisión de una <see cref="SolicitudColaboracion"/>.</summary>
public enum EstadoSolicitud
{
    [Display(Name = "Pendiente")]
    Pendiente = 0,

    [Display(Name = "Aprobada")]
    Aprobada = 1,

    [Display(Name = "Rechazada")]
    Rechazada = 2,
}

/// <summary>
/// Solicitud de colaboración enviada desde el formulario PÚBLICO de la web (anónimo,
/// sin autenticar). Es deliberadamente una entidad aparte de <see cref="Socio"/>: el
/// formulario público NO da de alta un socio directamente, sino que crea una solicitud
/// que un administrador revisa y, si procede, convierte en Socio + Colaboración desde
/// el back. Esto mantiene el alta real bajo control humano y evita que datos no
/// verificados entren directos en el modelo de negocio.
///
/// <para><b>Seguridad / RGPD:</b> aquí NUNCA se guarda el IBAN. La domiciliación, si la
/// hay, se recoge en el paso de confirmación del admin, no en el formulario público.
/// El campo honeypot antispam no se persiste (no es una propiedad de la entidad).</para>
/// </summary>
public class SolicitudColaboracion
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    [Display(Name = "Apellidos")]
    public string Apellidos { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    [Display(Name = "Teléfono")]
    [TelefonoE164]
    public string Telefono { get; set; } = string.Empty;

    /// <summary>Tipo de colaboración que la persona dice querer. Intención, no alta.</summary>
    [Required]
    [Display(Name = "Tipo de colaboración")]
    public TipoColaboracionSolicitada Tipo { get; set; } = TipoColaboracionSolicitada.Socio;

    /// <summary>
    /// Importe orientativo que la persona indica (opcional). Es una referencia para el
    /// admin; el importe real se fija al crear la colaboración en el back. En
    /// microdonación es fijo (1 €/mes) y en donación/socio lo indica la persona.
    /// </summary>
    [Range(0, 1_000_000, ErrorMessage = "Introduce un importe válido.")]
    [Display(Name = "Importe (€)")]
    public decimal? Importe { get; set; }

    /// <summary>
    /// Periodicidad de la cuota, SOLO cuando <see cref="Tipo"/> es
    /// <see cref="TipoColaboracionSolicitada.Socio"/>. Null en donación (puntual) y en
    /// microdonación (mensual fija por convención). Es una preferencia orientativa: la
    /// cuota real la configura el admin al dar de alta la colaboración.
    /// </summary>
    [Display(Name = "Periodicidad")]
    public ModalidadCuota? Periodicidad { get; set; }

    /// <summary>Consentimiento de la política de privacidad (RGPD). Obligatorio.</summary>
    [Display(Name = "Acepto la política de privacidad")]
    public bool AceptaPrivacidad { get; set; }

    /// <summary>Fecha en que se recibió la solicitud. La fija la aplicación.</summary>
    public DateTime FechaSolicitud { get; set; }

    /// <summary>Estado de revisión. Toda solicitud nace <see cref="EstadoSolicitud.Pendiente"/>.</summary>
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

    /// <summary>Fecha en que el admin resolvió (aprobó/rechazó) la solicitud; null si pendiente.</summary>
    public DateTime? FechaRevision { get; set; }

    /// <summary>Nota interna opcional del admin al resolver (motivo del rechazo, etc.).</summary>
    [StringLength(500)]
    [Display(Name = "Nota del revisor")]
    public string? NotaRevision { get; set; }
}
