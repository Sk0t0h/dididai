using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DididaiApp.Tests;

/// <summary>
/// Pruebas de integración de <see cref="ColaboracionService"/> sobre una BD SQLite
/// en memoria (respeta las restricciones relacionales, a diferencia del proveedor
/// InMemory). Cubren las reglas de negocio: socio inexistente, importe/IBAN inválidos,
/// alta correcta y baja lógica (finalizar sin borrar).
/// </summary>
public class ColaboracionServiceTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly ColaboracionService _sut;

    public ColaboracionServiceTests()
    {
        // Conexión en memoria compartida durante la vida del test (se cierra en Dispose).
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_conn)
            .Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _sut = new ColaboracionService(_db);
    }

    private async Task<int> SembrarSocioAsync()
    {
        var socio = new Socio
        {
            Nombre = "Test", Apellidos = "Socio", TipoDocumento = TipoDocumento.DniEspanol,
            Dni = "12345678Z", Telefono = "+34600111222", Email = "t@x.com",
            Direccion = "c", CodigoPostal = "28001", Localidad = "Madrid", PaisResidencia = "ES",
            AceptaPrivacidad = true, FechaAlta = DateTime.UtcNow
        };
        _db.Socios.Add(socio);
        await _db.SaveChangesAsync();
        return socio.Id;
    }

    [Fact]
    public async Task Crear_SocioInexistente_DevuelveSocioNoEncontrado()
    {
        var r = await _sut.CrearAsync(new AportacionUnica { SocioId = 999, Importe = 10 });
        Assert.Equal(ResultadoColaboracion.SocioNoEncontrado, r);
    }

    [Fact]
    public async Task Crear_ImporteCeroONegativo_DevuelveImporteInvalido()
    {
        var socioId = await SembrarSocioAsync();
        Assert.Equal(ResultadoColaboracion.ImporteInvalido,
            await _sut.CrearAsync(new AportacionUnica { SocioId = socioId, Importe = 0 }));
        Assert.Equal(ResultadoColaboracion.ImporteInvalido,
            await _sut.CrearAsync(new AportacionUnica { SocioId = socioId, Importe = -5 }));
    }

    [Fact]
    public async Task Crear_CuotaConIbanInvalido_DevuelveIbanInvalido()
    {
        var socioId = await SembrarSocioAsync();
        var r = await _sut.CrearAsync(new CuotaDomiciliada
        {
            SocioId = socioId, Importe = 10, Modalidad = ModalidadCuota.Mensual,
            Iban = "ES0000000000000000000000" // control inválido
        });
        Assert.Equal(ResultadoColaboracion.IbanInvalido, r);
    }

    [Fact]
    public async Task Crear_CuotaValida_CreaYNormalizaIban_YFijaFechaInicio()
    {
        var socioId = await SembrarSocioAsync();
        var r = await _sut.CrearAsync(new CuotaDomiciliada
        {
            SocioId = socioId, Importe = 10, Modalidad = ModalidadCuota.Mensual,
            Iban = "es91 2100 0418 4502 0005 1332" // minúsculas + espacios
        });
        Assert.Equal(ResultadoColaboracion.Creado, r);

        var creada = (CuotaDomiciliada)(await _sut.ListarPorSocioAsync(socioId))[0];
        Assert.Equal("ES9121000418450200051332", creada.Iban); // normalizado
        Assert.True(creada.Activa);
        Assert.NotEqual(default, creada.FechaInicio);
        Assert.Null(creada.FechaFin);
    }

    [Fact]
    public async Task Crear_AportacionUnicaValida_Crea()
    {
        var socioId = await SembrarSocioAsync();
        var r = await _sut.CrearAsync(new AportacionUnica { SocioId = socioId, Importe = 50 });
        Assert.Equal(ResultadoColaboracion.Creado, r);
    }

    [Fact]
    public async Task DarDeBaja_ColaboracionActiva_LaFinalizaSinBorrar()
    {
        var socioId = await SembrarSocioAsync();
        await _sut.CrearAsync(new AportacionUnica { SocioId = socioId, Importe = 50 });
        var col = (await _sut.ListarPorSocioAsync(socioId))[0];

        await _sut.DarDeBajaAsync(col.Id);

        var tras = await _sut.ObtenerAsync(col.Id);
        Assert.NotNull(tras);                 // sigue existiendo (baja lógica)
        Assert.False(tras!.Activa);
        Assert.NotNull(tras.FechaFin);
    }

    [Fact]
    public async Task DarDeBaja_Idempotente_NoFallaSiYaEstaDeBajaONoExiste()
    {
        var socioId = await SembrarSocioAsync();
        await _sut.CrearAsync(new AportacionUnica { SocioId = socioId, Importe = 50 });
        var col = (await _sut.ListarPorSocioAsync(socioId))[0];

        await _sut.DarDeBajaAsync(col.Id);
        var fechaFin = (await _sut.ObtenerAsync(col.Id))!.FechaFin;
        await _sut.DarDeBajaAsync(col.Id);    // segunda vez: no cambia nada
        await _sut.DarDeBajaAsync(123456);    // inexistente: no lanza

        Assert.Equal(fechaFin, (await _sut.ObtenerAsync(col.Id))!.FechaFin);
    }

    public void Dispose()
    {
        _db.Dispose();
        _conn.Dispose();
    }
}
