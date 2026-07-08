using DididaiApp.Core.Models;

namespace DididaiApp.Core.Services;

/// <summary>
/// Operaciones sobre las <see cref="SolicitudColaboracion"/> que llegan del formulario
/// público. La capa de presentación (páginas Razor) depende de esta abstracción y nunca
/// accede al <c>AppDbContext</c> directamente.
///
/// <para>El alta (<see cref="CrearAsync"/>) la invoca el formulario PÚBLICO anónimo; el
/// resto de operaciones (listar, resolver) son del back de gestión, protegidas por el
/// rol Admin en las páginas.</para>
/// </summary>
public interface ISolicitudColaboracionService
{
    /// <summary>
    /// Registra una solicitud entrante del formulario público. Fija
    /// <c>FechaSolicitud</c> y estado <see cref="EstadoSolicitud.Pendiente"/>, normaliza
    /// el teléfono y valida los campos de negocio. NUNCA guarda IBAN (no está en el modelo).
    /// </summary>
    Task<ResultadoSolicitud> CrearAsync(SolicitudColaboracion solicitud);

    /// <summary>
    /// Lista solicitudes ordenadas de más reciente a más antigua. Con
    /// <paramref name="estado"/> filtra por estado; null = todas.
    /// </summary>
    Task<IReadOnlyList<SolicitudColaboracion>> ListarAsync(EstadoSolicitud? estado = null);

    /// <summary>Nº de solicitudes pendientes (para el badge del panel admin).</summary>
    Task<int> ContarPendientesAsync();

    /// <summary>Obtiene una solicitud por id, o <c>null</c> si no existe.</summary>
    Task<SolicitudColaboracion?> ObtenerAsync(int id);

    /// <summary>
    /// Marca una solicitud como resuelta (<see cref="EstadoSolicitud.Aprobada"/> o
    /// <see cref="EstadoSolicitud.Rechazada"/>), fija <c>FechaRevision</c> y guarda la
    /// nota opcional. No crea el Socio: eso lo hace el admin en el alta, tomando esta
    /// solicitud como punto de partida. Idempotente sobre el estado destino.
    /// </summary>
    Task<bool> ResolverAsync(int id, EstadoSolicitud estado, string? nota);
}

/// <summary>Resultado del alta de una solicitud pública.</summary>
public enum ResultadoSolicitud
{
    /// <summary>Solicitud registrada correctamente.</summary>
    Registrada,
    /// <summary>Falta el consentimiento RGPD (obligatorio).</summary>
    FaltaConsentimiento,
    /// <summary>El teléfono no está en formato internacional E.164.</summary>
    TelefonoInvalido,
}
