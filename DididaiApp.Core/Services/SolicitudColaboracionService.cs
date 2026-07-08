using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DididaiApp.Core.Services;

/// <summary>
/// Implementación de <see cref="ISolicitudColaboracionService"/> sobre EF Core
/// (<see cref="AppDbContext"/>). Concentra las reglas de las solicitudes públicas:
/// fecha automática, estado inicial pendiente, normalización de teléfono y
/// validación de negocio en servidor (además de la de cliente del formulario).
/// </summary>
public class SolicitudColaboracionService : ISolicitudColaboracionService
{
    private readonly AppDbContext _db;

    public SolicitudColaboracionService(AppDbContext db) => _db = db;

    public async Task<ResultadoSolicitud> CrearAsync(SolicitudColaboracion solicitud)
    {
        // El consentimiento RGPD es obligatorio: sin él no se puede tratar el dato.
        if (!solicitud.AceptaPrivacidad)
            return ResultadoSolicitud.FaltaConsentimiento;

        solicitud.Telefono = ValidacionIdentidad.NormalizarTelefono(solicitud.Telefono);
        if (!ValidacionIdentidad.TelefonoValido(solicitud.Telefono))
            return ResultadoSolicitud.TelefonoInvalido;

        // Coherencia de campos por tipo: la periodicidad solo aplica al alta de socio.
        // En donación (puntual) y microdonación (mensual fija) se descarta cualquier
        // valor que hubiera podido colarse en el POST. En socio, si no se indicó una
        // preferencia, se asume mensual (el admin puede cambiarla al aprobar) para que
        // la solicitud nunca quede sin periodicidad.
        if (solicitud.Tipo != TipoColaboracionSolicitada.Socio)
            solicitud.Periodicidad = null;
        else
            solicitud.Periodicidad ??= ModalidadCuota.Mensual;

        // Estado y traza los fija SIEMPRE el servidor, nunca el formulario público.
        solicitud.Id = 0;
        solicitud.Estado = EstadoSolicitud.Pendiente;
        solicitud.FechaRevision = null;
        solicitud.NotaRevision = null;
        solicitud.FechaSolicitud = DateTime.UtcNow;

        _db.SolicitudesColaboracion.Add(solicitud);
        await _db.SaveChangesAsync();
        return ResultadoSolicitud.Registrada;
    }

    public async Task<IReadOnlyList<SolicitudColaboracion>> ListarAsync(EstadoSolicitud? estado = null)
    {
        IQueryable<SolicitudColaboracion> query = _db.SolicitudesColaboracion.AsNoTracking();
        if (estado is not null)
            query = query.Where(s => s.Estado == estado);
        return await query.OrderByDescending(s => s.FechaSolicitud).ToListAsync();
    }

    public Task<int> ContarPendientesAsync() =>
        _db.SolicitudesColaboracion.CountAsync(s => s.Estado == EstadoSolicitud.Pendiente);

    public Task<SolicitudColaboracion?> ObtenerAsync(int id) =>
        _db.SolicitudesColaboracion
            .Include(s => s.Acciones)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<bool> ResolverAsync(int id, EstadoSolicitud estado, string? nota)
    {
        // Solo son estados de resolución válidos Aprobada/Cancelada; Pendiente/Gestionando
        // no "resuelven".
        if (estado is not (EstadoSolicitud.Aprobada or EstadoSolicitud.Cancelada))
            return false;

        var solicitud = await _db.SolicitudesColaboracion.FirstOrDefaultAsync(s => s.Id == id);
        if (solicitud is null)
            return false;

        solicitud.Estado = estado;
        solicitud.NotaRevision = string.IsNullOrWhiteSpace(nota) ? null : nota.Trim();
        solicitud.FechaRevision = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RegistrarAccionAsync(int solicitudId, TipoAccionSolicitud tipo, string nota, string usuario)
    {
        if (string.IsNullOrWhiteSpace(nota))
            return false;

        var solicitud = await _db.SolicitudesColaboracion.FirstOrDefaultAsync(s => s.Id == solicitudId);
        if (solicitud is null)
            return false;

        _db.AccionesSolicitud.Add(new AccionSolicitud
        {
            SolicitudId = solicitudId,
            Tipo = tipo,
            Nota = nota.Trim(),
            Usuario = usuario,               // lo fija el llamante con el admin autenticado
            Fecha = DateTime.UtcNow,
        });

        // La primera gestión saca la solicitud de "Pendiente"; nunca retrocede un estado
        // ya resuelto (Aprobada/Cancelada) ni re-mueve una que ya está en Gestionando.
        if (solicitud.Estado == EstadoSolicitud.Pendiente)
            solicitud.Estado = EstadoSolicitud.Gestionando;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> VincularSocioAsync(int solicitudId, int socioId)
    {
        var solicitud = await _db.SolicitudesColaboracion.FirstOrDefaultAsync(s => s.Id == solicitudId);
        if (solicitud is null)
            return false;

        var existeSocio = await _db.Socios.AnyAsync(s => s.Id == socioId);
        if (!existeSocio)
            return false;

        solicitud.SocioId = socioId;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<SolicitudColaboracion>> ListarPorSocioAsync(int socioId) =>
        await _db.SolicitudesColaboracion.AsNoTracking()
            .Where(s => s.SocioId == socioId)
            .OrderByDescending(s => s.FechaSolicitud)
            .ToListAsync();

    public async Task<ResultadoCrearColaboracion> CrearColaboracionDesdeSolicitudAsync(
        int solicitudId, decimal importe, ModalidadCuota modalidad, string? iban)
    {
        var solicitud = await _db.SolicitudesColaboracion.FirstOrDefaultAsync(s => s.Id == solicitudId);
        if (solicitud is null)
            return ResultadoCrearColaboracion.SolicitudNoEncontrada;
        if (solicitud.SocioId is not int socioId)
            return ResultadoCrearColaboracion.SinSocioVinculado;
        if (solicitud.ColaboracionId is not null)
            return ResultadoCrearColaboracion.YaTieneColaboracion;
        if (importe <= 0)
            return ResultadoCrearColaboracion.ImporteInvalido;

        // El tipo de la solicitud decide el subtipo de colaboración (TPH). La microdonación
        // (Teaming) se gestiona en la plataforma de Teaming: aquí no se crea colaboración.
        Colaboracion colaboracion;
        switch (solicitud.Tipo)
        {
            case TipoColaboracionSolicitada.Socio:
                var norm = ValidacionIban.Normalizar(iban ?? string.Empty);
                if (!ValidacionIban.EsValido(norm))
                    return ResultadoCrearColaboracion.IbanInvalido;
                colaboracion = new CuotaDomiciliada { Modalidad = modalidad, Iban = norm };
                break;

            case TipoColaboracionSolicitada.Donacion:
                colaboracion = new AportacionUnica { Fecha = DateTime.UtcNow };
                break;

            default: // Microdonacion / Teaming
                return ResultadoCrearColaboracion.TipoSinColaboracion;
        }

        colaboracion.SocioId = socioId;
        colaboracion.Importe = importe;
        colaboracion.FechaInicio = DateTime.UtcNow;
        colaboracion.FechaFin = null;
        colaboracion.Activa = true;

        _db.Colaboraciones.Add(colaboracion);
        await _db.SaveChangesAsync();   // asigna Id a la colaboración

        // Enlaza la solicitud con la colaboración creada (evita duplicar) y, si aún no
        // estaba resuelta, la deja aprobada (el socio ya tiene su colaboración real).
        solicitud.ColaboracionId = colaboracion.Id;
        if (solicitud.Estado != EstadoSolicitud.Cancelada)
            solicitud.Estado = EstadoSolicitud.Aprobada;
        await _db.SaveChangesAsync();

        return ResultadoCrearColaboracion.Creada;
    }
}
