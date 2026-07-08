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
    /// <see cref="EstadoSolicitud.Cancelada"/>), fija <c>FechaRevision</c> y guarda la
    /// nota opcional. No crea el Socio: eso lo hace el admin en el alta, tomando esta
    /// solicitud como punto de partida. Idempotente sobre el estado destino.
    /// </summary>
    Task<bool> ResolverAsync(int id, EstadoSolicitud estado, string? nota);

    /// <summary>
    /// Registra una acción de gestión (contacto/nota) en el historial de la solicitud.
    /// <paramref name="usuario"/> y la fecha los fija el llamante/servidor (auditoría; no
    /// vienen del formulario). Si la solicitud estaba <see cref="EstadoSolicitud.Pendiente"/>,
    /// pasa a <see cref="EstadoSolicitud.Gestionando"/> (no retrocede estados ya resueltos).
    /// Devuelve false si la solicitud no existe o la nota está vacía.
    /// </summary>
    Task<bool> RegistrarAccionAsync(int solicitudId, TipoAccionSolicitud tipo, string nota, string usuario);

    /// <summary>
    /// Vincula la solicitud a un socio EXISTENTE (el admin ha confirmado que la persona ya
    /// colabora). Fija <c>SocioId</c>. No cambia el estado (la aprobación es aparte).
    /// Devuelve false si la solicitud o el socio no existen.
    /// </summary>
    Task<bool> VincularSocioAsync(int solicitudId, int socioId);

    /// <summary>Solicitudes vinculadas a un socio, más recientes primero (para su ficha).</summary>
    Task<IReadOnlyList<SolicitudColaboracion>> ListarPorSocioAsync(int socioId);

    /// <summary>
    /// Crea la <see cref="Colaboracion"/> que corresponde al tipo de una solicitud, para el
    /// socio al que está vinculada, con el importe (y, si es cuota domiciliada, la
    /// periodicidad e IBAN) que confirma el admin. Enlaza la solicitud con la colaboración
    /// creada (<c>ColaboracionId</c>) para no duplicarla. La microdonación (Teaming) no
    /// genera colaboración (se gestiona en Teaming). Ver <see cref="ResultadoCrearColaboracion"/>.
    /// </summary>
    Task<ResultadoCrearColaboracion> CrearColaboracionDesdeSolicitudAsync(
        int solicitudId, decimal importe, ModalidadCuota modalidad, string? iban);
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

/// <summary>Resultado de crear la colaboración a partir de una solicitud.</summary>
public enum ResultadoCrearColaboracion
{
    /// <summary>Colaboración creada y enlazada a la solicitud.</summary>
    Creada,
    /// <summary>La solicitud no existe.</summary>
    SolicitudNoEncontrada,
    /// <summary>La solicitud no está vinculada a ningún socio (hay que vincularla antes).</summary>
    SinSocioVinculado,
    /// <summary>La solicitud ya tiene una colaboración creada (no se duplica).</summary>
    YaTieneColaboracion,
    /// <summary>El tipo de la solicitud es microdonación/Teaming: no genera colaboración aquí.</summary>
    TipoSinColaboracion,
    /// <summary>El importe debe ser mayor que cero.</summary>
    ImporteInvalido,
    /// <summary>La cuota domiciliada requiere un IBAN válido (mod-97).</summary>
    IbanInvalido,
}
