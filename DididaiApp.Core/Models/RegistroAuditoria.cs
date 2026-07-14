using System.ComponentModel.DataAnnotations;

namespace DididaiApp.Core.Models;

/// <summary>
/// Tipo de acción de gestión que queda registrada en el log de auditoría transversal.
/// A diferencia de <see cref="TipoAccionSolicitud"/> (gestión manual anotada por el admin
/// sobre UNA solicitud), estas acciones las registra el sistema automáticamente cada vez
/// que ocurre una operación relevante de gestión, sea cual sea el módulo.
/// </summary>
public enum TipoAccionAuditoria
{
    [Display(Name = "Alta de socio")]
    SocioAlta = 0,

    [Display(Name = "Edición de socio")]
    SocioEdicion = 1,

    [Display(Name = "Baja de socio")]
    SocioBaja = 2,

    [Display(Name = "Reactivación de socio")]
    SocioReactivacion = 3,

    [Display(Name = "Alta de colaboración")]
    ColaboracionAlta = 4,

    [Display(Name = "Edición de colaboración")]
    ColaboracionEdicion = 5,

    [Display(Name = "Baja de colaboración")]
    ColaboracionBaja = 6,

    [Display(Name = "Solicitud aprobada")]
    SolicitudAprobada = 7,

    [Display(Name = "Solicitud cancelada")]
    SolicitudCancelada = 8,

    [Display(Name = "Solicitud vinculada a socio")]
    SolicitudVinculada = 9,

    [Display(Name = "Alta de administrador")]
    AdminAlta = 10,

    [Display(Name = "Desactivación de administrador")]
    AdminDesactivacion = 11,

    [Display(Name = "Reactivación de administrador")]
    AdminReactivacion = 12,
}

/// <summary>
/// Una entrada del log de auditoría: quién (<see cref="Usuario"/>) hizo qué
/// (<see cref="Accion"/>) sobre qué entidad (<see cref="Entidad"/>/<see cref="EntidadId"/>)
/// y cuándo (<see cref="Fecha"/>), con un <see cref="Detalle"/> legible.
///
/// <para>Es un registro <b>inmutable</b>: se inserta pero no se edita ni se borra desde la
/// aplicación. La <see cref="Fecha"/> y el <see cref="Usuario"/> los fija SIEMPRE el
/// servidor (el usuario, con el admin autenticado de la petición); nunca provienen de un
/// formulario.</para>
/// </summary>
public class RegistroAuditoria
{
    public int Id { get; set; }

    /// <summary>Fecha/hora (UTC) en que ocurrió la acción. La fija el servidor.</summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Identidad del administrador que ejecutó la acción (su nombre de usuario/email). La
    /// fija el servidor a partir de la identidad de la petición; NO proviene del formulario.
    /// </summary>
    [Required]
    [StringLength(256)]
    public string Usuario { get; set; } = string.Empty;

    /// <summary>Qué acción de gestión se registró.</summary>
    public TipoAccionAuditoria Accion { get; set; }

    /// <summary>
    /// Nombre corto del tipo de entidad afectada ("Socio", "Colaboración", "Solicitud",
    /// "Administrador"). Es descriptivo para la vista; el vínculo real lo da
    /// <see cref="EntidadId"/>.
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Entidad { get; set; } = string.Empty;

    /// <summary>
    /// Identificador de la entidad afectada, como texto. Se usa string y no int porque los
    /// ids no son homogéneos: socios/colaboraciones/solicitudes usan enteros, pero los
    /// administradores (usuarios de Identity) usan GUID.
    /// </summary>
    [StringLength(64)]
    public string EntidadId { get; set; } = string.Empty;

    /// <summary>
    /// Descripción legible de lo ocurrido, para leer el log sin cruzar con otras tablas
    /// (p. ej. «Socio Juan Pérez (DNI 12345678Z)»).
    /// </summary>
    [StringLength(500)]
    public string Detalle { get; set; } = string.Empty;

    /// <summary>
    /// Detalle de los cambios en una edición, como JSON campo→(antes, después). Solo lo
    /// rellenan las acciones de edición (socio, colaboración); <c>null</c> en las demás. Lo
    /// genera <see cref="ConstructorCambios"/>. No tiene límite de longitud (una edición con
    /// muchos campos puede ser larga); se muestra formateado en la ficha de auditoría.
    /// </summary>
    public string? Cambios { get; set; }
}
