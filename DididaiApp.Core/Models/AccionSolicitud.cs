using System.ComponentModel.DataAnnotations;

namespace DididaiApp.Core.Models;

/// <summary>Tipo de acción de gestión registrada sobre una solicitud.</summary>
public enum TipoAccionSolicitud
{
    [Display(Name = "Contacto por email")]
    Email = 0,

    [Display(Name = "Contacto por teléfono")]
    Telefono = 1,

    [Display(Name = "Nota")]
    Nota = 2,

    [Display(Name = "Otro")]
    Otro = 3,
}

/// <summary>
/// Una acción de gestión registrada por un administrador sobre una
/// <see cref="SolicitudColaboracion"/> (un contacto por email/teléfono, una nota de
/// seguimiento…). Forma un historial ordenado por fecha en la ficha de la solicitud.
///
/// <para>Es un registro de auditoría: <see cref="Usuario"/> y <see cref="Fecha"/> los fija
/// SIEMPRE el servidor con el admin autenticado y la hora actual; no son editables desde el
/// formulario. Registrar la primera acción mueve la solicitud de
/// <see cref="EstadoSolicitud.Pendiente"/> a <see cref="EstadoSolicitud.Gestionando"/>.</para>
/// </summary>
public class AccionSolicitud
{
    public int Id { get; set; }

    /// <summary>Solicitud a la que pertenece esta acción.</summary>
    public int SolicitudId { get; set; }

    /// <summary>Navegación a la solicitud (ver <see cref="SolicitudId"/>).</summary>
    public SolicitudColaboracion? Solicitud { get; set; }

    [Required]
    [Display(Name = "Tipo de acción")]
    public TipoAccionSolicitud Tipo { get; set; } = TipoAccionSolicitud.Nota;

    /// <summary>Detalle libre de la acción (qué se hizo/habló, motivo, seguimiento…).</summary>
    [Required(ErrorMessage = "Escribe una nota para la acción.")]
    [StringLength(1000)]
    [Display(Name = "Nota")]
    public string Nota { get; set; } = string.Empty;

    /// <summary>
    /// Identidad del administrador que registró la acción (su nombre de usuario/email). La
    /// fija el servidor con el usuario autenticado; NO es editable (auditoría).
    /// </summary>
    [Required]
    [StringLength(256)]
    public string Usuario { get; set; } = string.Empty;

    /// <summary>Fecha/hora (UTC) en que se registró. La fija el servidor.</summary>
    public DateTime Fecha { get; set; }
}
