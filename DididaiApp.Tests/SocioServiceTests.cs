using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DididaiApp.Tests;

/// <summary>
/// Pruebas de integración de <see cref="SocioService"/> sobre SQLite en memoria. Aquí se
/// cubre la búsqueda de posibles coincidencias por email/teléfono (matching del flujo de
/// solicitudes); las reglas de alta/DNI están cubiertas indirectamente por otros tests.
/// </summary>
public class SocioServiceTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly SocioService _sut;

    public SocioServiceTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _sut = new SocioService(_db);
    }

    private async Task<Socio> SembrarAsync(string dni, string email, string telefono, DateTime? baja = null)
    {
        var socio = new Socio
        {
            Nombre = "Test", Apellidos = "Socio", TipoDocumento = TipoDocumento.DniEspanol,
            Dni = dni, Email = email, Telefono = telefono, PaisResidencia = "ES",
            AceptaPrivacidad = true, FechaAlta = DateTime.UtcNow, FechaBaja = baja,
        };
        _db.Socios.Add(socio);
        await _db.SaveChangesAsync();
        return socio;
    }

    [Fact]
    public async Task Buscar_CoincidePorEmail()
    {
        await SembrarAsync("12345678Z", "ada@x.com", "+34600111222");

        var r = await _sut.BuscarPosiblesCoincidenciasAsync("ada@x.com", "+34999999999");

        Assert.Single(r);
    }

    [Fact]
    public async Task Buscar_CoincidePorTelefono()
    {
        await SembrarAsync("12345678Z", "ada@x.com", "+34600111222");

        var r = await _sut.BuscarPosiblesCoincidenciasAsync("otro@x.com", "+34600111222");

        Assert.Single(r);
    }

    [Fact]
    public async Task Buscar_EmailInsensibleAMayusculasYEspacios()
    {
        await SembrarAsync("12345678Z", "ada@x.com", "+34600111222");

        var r = await _sut.BuscarPosiblesCoincidenciasAsync("  ADA@X.COM ", null);

        Assert.Single(r);
    }

    [Fact]
    public async Task Buscar_NoDuplicaSiCoincidenAmbos()
    {
        await SembrarAsync("12345678Z", "ada@x.com", "+34600111222");

        var r = await _sut.BuscarPosiblesCoincidenciasAsync("ada@x.com", "+34600111222");

        Assert.Single(r); // el mismo socio, no dos veces
    }

    [Fact]
    public async Task Buscar_ExcluyeSociosDeBaja()
    {
        await SembrarAsync("12345678Z", "ada@x.com", "+34600111222", baja: DateTime.UtcNow);

        var r = await _sut.BuscarPosiblesCoincidenciasAsync("ada@x.com", "+34600111222");

        Assert.Empty(r);
    }

    [Fact]
    public async Task Buscar_SinCriterio_DevuelveVacio()
    {
        await SembrarAsync("12345678Z", "ada@x.com", "+34600111222");

        var r = await _sut.BuscarPosiblesCoincidenciasAsync(null, "   ");

        Assert.Empty(r);
    }

    [Fact]
    public async Task Buscar_SinCoincidencias_DevuelveVacio()
    {
        await SembrarAsync("12345678Z", "ada@x.com", "+34600111222");

        var r = await _sut.BuscarPosiblesCoincidenciasAsync("nadie@x.com", "+34111111111");

        Assert.Empty(r);
    }

    // Construye un Socio DESACOPLADO (instancia nueva con el mismo Id) para simular lo que
    // llega del model binding en producción. Si se reusara la instancia tracked de EF, el
    // identity map haría que "actual" y el entrante fueran el mismo objeto y el diff no vería
    // los cambios.
    private static Socio Editable(Socio origen) => new()
    {
        Id = origen.Id, Nombre = origen.Nombre, Apellidos = origen.Apellidos,
        TipoDocumento = origen.TipoDocumento, Dni = origen.Dni, Email = origen.Email,
        Telefono = origen.Telefono, PaisResidencia = origen.PaisResidencia,
        Direccion = origen.Direccion, CodigoPostal = origen.CodigoPostal,
        Localidad = origen.Localidad, AceptaPrivacidad = origen.AceptaPrivacidad,
    };

    [Fact]
    public async Task Actualizar_RegistraSoloLosCamposCambiados()
    {
        var socio = await SembrarAsync("12345678Z", "ada@x.com", "+34600111222");

        // Se edita: mismo DNI/teléfono/país; cambian nombre y email.
        var edit = Editable(socio);
        edit.Nombre = "AdaEditado";
        edit.Email = "nuevo@x.com";
        var r = await _sut.ActualizarAsync(edit);

        Assert.Equal(ResultadoActualizacion.Actualizado, r.Resultado);
        Assert.NotNull(r.Cambios);
        Assert.Contains("Nombre", r.Cambios);
        Assert.Contains("AdaEditado", r.Cambios);
        Assert.Contains("Email", r.Cambios);
        // No se tocó el teléfono → no debe figurar en el diff.
        Assert.DoesNotContain("Teléfono", r.Cambios);
    }

    [Fact]
    public async Task Actualizar_SinCambios_DevuelveCambiosNull()
    {
        var socio = await SembrarAsync("12345678Z", "ada@x.com", "+34600111222");

        // Se reenvía idéntico (el DNI se normaliza igual): no cambia nada.
        var r = await _sut.ActualizarAsync(Editable(socio));

        Assert.Equal(ResultadoActualizacion.Actualizado, r.Resultado);
        Assert.Null(r.Cambios);
    }

    public void Dispose()
    {
        _db.Dispose();
        _conn.Dispose();
    }
}
