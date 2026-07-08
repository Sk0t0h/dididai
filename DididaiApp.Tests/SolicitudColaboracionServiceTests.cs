using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DididaiApp.Tests;

/// <summary>
/// Pruebas de integración de <see cref="SolicitudColaboracionService"/> sobre SQLite
/// en memoria. Cubren las reglas de las solicitudes públicas: consentimiento RGPD
/// obligatorio, teléfono E.164, coherencia de periodicidad por tipo, estado inicial
/// fijado por el servidor y el flujo de resolución (aprobar/rechazar) del admin.
/// </summary>
public class SolicitudColaboracionServiceTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly SolicitudColaboracionService _sut;

    public SolicitudColaboracionServiceTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _sut = new SolicitudColaboracionService(_db);
    }

    private static SolicitudColaboracion Valida() => new()
    {
        Nombre = "Ada", Apellidos = "Lovelace", Email = "ada@example.org",
        Telefono = "+34600111222", Tipo = TipoColaboracionSolicitada.Donacion,
        Importe = 20m, AceptaPrivacidad = true,
    };

    [Fact]
    public async Task Crear_SinConsentimiento_DevuelveFaltaConsentimiento()
    {
        var s = Valida();
        s.AceptaPrivacidad = false;

        var r = await _sut.CrearAsync(s);

        Assert.Equal(ResultadoSolicitud.FaltaConsentimiento, r);
        Assert.Equal(0, await _db.SolicitudesColaboracion.CountAsync());
    }

    [Fact]
    public async Task Crear_TelefonoNoE164_DevuelveTelefonoInvalido()
    {
        var s = Valida();
        s.Telefono = "600 no es e164";

        var r = await _sut.CrearAsync(s);

        Assert.Equal(ResultadoSolicitud.TelefonoInvalido, r);
        Assert.Equal(0, await _db.SolicitudesColaboracion.CountAsync());
    }

    [Fact]
    public async Task Crear_Valida_RegistraPendienteYFijaFecha()
    {
        var r = await _sut.CrearAsync(Valida());

        Assert.Equal(ResultadoSolicitud.Registrada, r);
        var guardada = await _db.SolicitudesColaboracion.SingleAsync();
        Assert.Equal(EstadoSolicitud.Pendiente, guardada.Estado);
        Assert.NotEqual(default, guardada.FechaSolicitud);
        Assert.Null(guardada.FechaRevision);
    }

    [Fact]
    public async Task Crear_NoSocio_DescartaPeriodicidad()
    {
        // Aunque el POST traiga periodicidad, en donación/microdonación no aplica.
        var s = Valida();
        s.Tipo = TipoColaboracionSolicitada.Microdonacion;
        s.Periodicidad = ModalidadCuota.Anual;

        await _sut.CrearAsync(s);

        var guardada = await _db.SolicitudesColaboracion.SingleAsync();
        Assert.Null(guardada.Periodicidad);
    }

    [Fact]
    public async Task Crear_Socio_ConservaPeriodicidad()
    {
        var s = Valida();
        s.Tipo = TipoColaboracionSolicitada.Socio;
        s.Periodicidad = ModalidadCuota.Mensual;

        await _sut.CrearAsync(s);

        var guardada = await _db.SolicitudesColaboracion.SingleAsync();
        Assert.Equal(ModalidadCuota.Mensual, guardada.Periodicidad);
    }

    [Fact]
    public async Task Crear_SocioSinPeriodicidad_AsumeMensual()
    {
        // El formulario público puede enviarse sin elegir periodicidad; para socio se
        // asume mensual (el admin la ajusta al aprobar), nunca queda null.
        var s = Valida();
        s.Tipo = TipoColaboracionSolicitada.Socio;
        s.Periodicidad = null;

        await _sut.CrearAsync(s);

        var guardada = await _db.SolicitudesColaboracion.SingleAsync();
        Assert.Equal(ModalidadCuota.Mensual, guardada.Periodicidad);
    }

    [Fact]
    public async Task Crear_NoConfiaEnEstadoDelCliente()
    {
        // Un cliente malicioso podría intentar enviar la solicitud ya "Aprobada".
        var s = Valida();
        s.Estado = EstadoSolicitud.Aprobada;
        s.FechaRevision = DateTime.UtcNow;

        await _sut.CrearAsync(s);

        var guardada = await _db.SolicitudesColaboracion.SingleAsync();
        Assert.Equal(EstadoSolicitud.Pendiente, guardada.Estado);
        Assert.Null(guardada.FechaRevision);
    }

    [Fact]
    public async Task Resolver_Aprobar_FijaEstadoFechaYNota()
    {
        await _sut.CrearAsync(Valida());
        var id = (await _db.SolicitudesColaboracion.SingleAsync()).Id;

        var ok = await _sut.ResolverAsync(id, EstadoSolicitud.Aprobada, "  contactada  ");

        Assert.True(ok);
        var s = await _db.SolicitudesColaboracion.SingleAsync();
        Assert.Equal(EstadoSolicitud.Aprobada, s.Estado);
        Assert.Equal("contactada", s.NotaRevision);
        Assert.NotNull(s.FechaRevision);
    }

    [Fact]
    public async Task Resolver_EstadoNoResolutorio_Rechaza()
    {
        await _sut.CrearAsync(Valida());
        var id = (await _db.SolicitudesColaboracion.SingleAsync()).Id;

        var ok = await _sut.ResolverAsync(id, EstadoSolicitud.Pendiente, null);

        Assert.False(ok);
    }

    [Fact]
    public async Task Resolver_IdInexistente_DevuelveFalse()
    {
        var ok = await _sut.ResolverAsync(999, EstadoSolicitud.Cancelada, null);
        Assert.False(ok);
    }

    [Fact]
    public async Task ContarPendientes_CuentaSoloPendientes()
    {
        await _sut.CrearAsync(Valida());
        await _sut.CrearAsync(Valida());
        var id = (await _db.SolicitudesColaboracion.FirstAsync()).Id;
        await _sut.ResolverAsync(id, EstadoSolicitud.Cancelada, null);

        Assert.Equal(1, await _sut.ContarPendientesAsync());
    }

    public void Dispose()
    {
        _db.Dispose();
        _conn.Dispose();
    }
}
