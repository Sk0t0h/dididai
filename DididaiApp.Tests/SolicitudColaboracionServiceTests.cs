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

    // ---- Acciones de gestión (bloque B) ----

    [Fact]
    public async Task RegistrarAccion_FijaUsuarioYFechaEnServidor()
    {
        await _sut.CrearAsync(Valida());
        var id = (await _db.SolicitudesColaboracion.SingleAsync()).Id;

        var ok = await _sut.RegistrarAccionAsync(id, TipoAccionSolicitud.Email, "  Contactada por email  ", "admin@dididai.org");

        Assert.True(ok);
        var accion = await _db.AccionesSolicitud.SingleAsync();
        Assert.Equal(id, accion.SolicitudId);
        Assert.Equal(TipoAccionSolicitud.Email, accion.Tipo);
        Assert.Equal("Contactada por email", accion.Nota);       // recortada
        Assert.Equal("admin@dididai.org", accion.Usuario);       // la fija el servidor
        Assert.NotEqual(default, accion.Fecha);
    }

    [Fact]
    public async Task RegistrarAccion_PrimeraAccion_PasaPendienteAGestionando()
    {
        await _sut.CrearAsync(Valida());
        var id = (await _db.SolicitudesColaboracion.SingleAsync()).Id;
        Assert.Equal(EstadoSolicitud.Pendiente, (await _db.SolicitudesColaboracion.SingleAsync()).Estado);

        await _sut.RegistrarAccionAsync(id, TipoAccionSolicitud.Telefono, "Llamada", "admin@dididai.org");

        Assert.Equal(EstadoSolicitud.Gestionando, (await _db.SolicitudesColaboracion.SingleAsync()).Estado);
    }

    [Fact]
    public async Task RegistrarAccion_NoRetrocedeEstadoSiYaResuelta()
    {
        await _sut.CrearAsync(Valida());
        var id = (await _db.SolicitudesColaboracion.SingleAsync()).Id;
        await _sut.ResolverAsync(id, EstadoSolicitud.Aprobada, null);

        // Registrar una acción sobre una solicitud ya aprobada no debe devolverla a Gestionando.
        await _sut.RegistrarAccionAsync(id, TipoAccionSolicitud.Nota, "Seguimiento posterior", "admin@dididai.org");

        Assert.Equal(EstadoSolicitud.Aprobada, (await _db.SolicitudesColaboracion.SingleAsync()).Estado);
    }

    [Fact]
    public async Task RegistrarAccion_NotaVacia_DevuelveFalseYNoGuarda()
    {
        await _sut.CrearAsync(Valida());
        var id = (await _db.SolicitudesColaboracion.SingleAsync()).Id;

        var ok = await _sut.RegistrarAccionAsync(id, TipoAccionSolicitud.Nota, "   ", "admin@dididai.org");

        Assert.False(ok);
        Assert.Equal(0, await _db.AccionesSolicitud.CountAsync());
    }

    [Fact]
    public async Task RegistrarAccion_SolicitudInexistente_DevuelveFalse()
    {
        var ok = await _sut.RegistrarAccionAsync(999, TipoAccionSolicitud.Nota, "x", "admin@dididai.org");
        Assert.False(ok);
    }

    // ---- Vinculación a socio existente (bloque C1) ----

    [Fact]
    public async Task VincularSocio_FijaSocioId()
    {
        await _sut.CrearAsync(Valida());
        var solId = (await _db.SolicitudesColaboracion.SingleAsync()).Id;
        var socio = new Socio
        {
            Nombre = "Ada", Apellidos = "L", TipoDocumento = TipoDocumento.DniEspanol,
            Dni = "12345678Z", Email = "ada@x.com", Telefono = "+34600111222",
            PaisResidencia = "ES", AceptaPrivacidad = true, FechaAlta = DateTime.UtcNow,
        };
        _db.Socios.Add(socio);
        await _db.SaveChangesAsync();

        var ok = await _sut.VincularSocioAsync(solId, socio.Id);

        Assert.True(ok);
        Assert.Equal(socio.Id, (await _db.SolicitudesColaboracion.SingleAsync()).SocioId);
    }

    [Fact]
    public async Task VincularSocio_SocioInexistente_DevuelveFalse()
    {
        await _sut.CrearAsync(Valida());
        var solId = (await _db.SolicitudesColaboracion.SingleAsync()).Id;

        var ok = await _sut.VincularSocioAsync(solId, 999);

        Assert.False(ok);
        Assert.Null((await _db.SolicitudesColaboracion.SingleAsync()).SocioId);
    }

    [Fact]
    public async Task VincularSocio_SolicitudInexistente_DevuelveFalse()
    {
        var ok = await _sut.VincularSocioAsync(999, 1);
        Assert.False(ok);
    }

    // ---- Crear colaboración desde solicitud (bloque C2a) ----

    private async Task<Socio> SembrarSocioAsync()
    {
        var socio = new Socio
        {
            Nombre = "Ada", Apellidos = "L", TipoDocumento = TipoDocumento.DniEspanol,
            Dni = "12345678Z", Email = "ada@x.com", Telefono = "+34600111222",
            PaisResidencia = "ES", AceptaPrivacidad = true, FechaAlta = DateTime.UtcNow,
        };
        _db.Socios.Add(socio);
        await _db.SaveChangesAsync();
        return socio;
    }

    [Fact]
    public async Task CrearColaboracion_Donacion_CreaAportacionUnicaYEnlaza()
    {
        var s = Valida();
        s.Tipo = TipoColaboracionSolicitada.Donacion;
        await _sut.CrearAsync(s);
        var solId = (await _db.SolicitudesColaboracion.SingleAsync()).Id;
        var socio = await SembrarSocioAsync();
        await _sut.VincularSocioAsync(solId, socio.Id);

        var r = await _sut.CrearColaboracionDesdeSolicitudAsync(solId, 50m, ModalidadCuota.Mensual, null);

        Assert.Equal(ResultadoCrearColaboracion.Creada, r);
        var colab = await _db.Colaboraciones.SingleAsync();
        Assert.IsType<AportacionUnica>(colab);
        Assert.Equal(50m, colab.Importe);
        Assert.Equal(socio.Id, colab.SocioId);
        var sol = await _db.SolicitudesColaboracion.SingleAsync();
        Assert.Equal(colab.Id, sol.ColaboracionId);
        Assert.Equal(EstadoSolicitud.Aprobada, sol.Estado);
    }

    [Fact]
    public async Task CrearColaboracion_Socio_ExigeIbanValido()
    {
        var s = Valida();
        s.Tipo = TipoColaboracionSolicitada.Socio;
        await _sut.CrearAsync(s);
        var solId = (await _db.SolicitudesColaboracion.SingleAsync()).Id;
        var socio = await SembrarSocioAsync();
        await _sut.VincularSocioAsync(solId, socio.Id);

        var malo = await _sut.CrearColaboracionDesdeSolicitudAsync(solId, 20m, ModalidadCuota.Mensual, "ES00 MAL");
        Assert.Equal(ResultadoCrearColaboracion.IbanInvalido, malo);

        var bueno = await _sut.CrearColaboracionDesdeSolicitudAsync(solId, 20m, ModalidadCuota.Mensual, "ES9121000418450200051332");
        Assert.Equal(ResultadoCrearColaboracion.Creada, bueno);
        Assert.IsType<CuotaDomiciliada>(await _db.Colaboraciones.SingleAsync());
    }

    [Fact]
    public async Task CrearColaboracion_Microdonacion_NoGeneraColaboracion()
    {
        var s = Valida();
        s.Tipo = TipoColaboracionSolicitada.Microdonacion;
        await _sut.CrearAsync(s);
        var solId = (await _db.SolicitudesColaboracion.SingleAsync()).Id;
        var socio = await SembrarSocioAsync();
        await _sut.VincularSocioAsync(solId, socio.Id);

        var r = await _sut.CrearColaboracionDesdeSolicitudAsync(solId, 1m, ModalidadCuota.Mensual, null);

        Assert.Equal(ResultadoCrearColaboracion.TipoSinColaboracion, r);
        Assert.Equal(0, await _db.Colaboraciones.CountAsync());
    }

    [Fact]
    public async Task CrearColaboracion_SinSocioVinculado_Devuelve()
    {
        var s = Valida();
        s.Tipo = TipoColaboracionSolicitada.Donacion;
        await _sut.CrearAsync(s);
        var solId = (await _db.SolicitudesColaboracion.SingleAsync()).Id;

        var r = await _sut.CrearColaboracionDesdeSolicitudAsync(solId, 50m, ModalidadCuota.Mensual, null);

        Assert.Equal(ResultadoCrearColaboracion.SinSocioVinculado, r);
    }

    [Fact]
    public async Task CrearColaboracion_NoDuplica()
    {
        var s = Valida();
        s.Tipo = TipoColaboracionSolicitada.Donacion;
        await _sut.CrearAsync(s);
        var solId = (await _db.SolicitudesColaboracion.SingleAsync()).Id;
        var socio = await SembrarSocioAsync();
        await _sut.VincularSocioAsync(solId, socio.Id);
        await _sut.CrearColaboracionDesdeSolicitudAsync(solId, 50m, ModalidadCuota.Mensual, null);

        var segunda = await _sut.CrearColaboracionDesdeSolicitudAsync(solId, 99m, ModalidadCuota.Mensual, null);

        Assert.Equal(ResultadoCrearColaboracion.YaTieneColaboracion, segunda);
        Assert.Equal(1, await _db.Colaboraciones.CountAsync());
    }

    [Fact]
    public async Task CrearColaboracion_ImporteInvalido_Devuelve()
    {
        var s = Valida();
        s.Tipo = TipoColaboracionSolicitada.Donacion;
        await _sut.CrearAsync(s);
        var solId = (await _db.SolicitudesColaboracion.SingleAsync()).Id;
        var socio = await SembrarSocioAsync();
        await _sut.VincularSocioAsync(solId, socio.Id);

        var r = await _sut.CrearColaboracionDesdeSolicitudAsync(solId, 0m, ModalidadCuota.Mensual, null);

        Assert.Equal(ResultadoCrearColaboracion.ImporteInvalido, r);
    }

    public void Dispose()
    {
        _db.Dispose();
        _conn.Dispose();
    }
}
