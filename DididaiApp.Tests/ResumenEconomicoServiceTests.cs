using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DididaiApp.Tests;

/// <summary>
/// Pruebas del cálculo económico (TDD): normalización anual→mensual del recurrente,
/// desglose por tipo, socios activos con colaboración, altas por mes y balance.
/// SQLite en memoria. Escritas antes de la implementación del servicio.
/// </summary>
public class ResumenEconomicoServiceTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly ResumenEconomicoService _sut;

    public ResumenEconomicoServiceTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options);
        _db.Database.EnsureCreated();
        _sut = new ResumenEconomicoService(_db);
    }

    private Socio NuevoSocio(bool activo = true)
    {
        var s = new Socio
        {
            Nombre = "N", Apellidos = "A", TipoDocumento = TipoDocumento.Otro, Dni = System.Guid.NewGuid().ToString()[..8],
            Telefono = "+34600111222", Email = "n@x.com", Direccion = "c", CodigoPostal = "1", Localidad = "M",
            PaisResidencia = "ES", AceptaPrivacidad = true, FechaAlta = new DateTime(2026, 1, 1),
            FechaBaja = activo ? null : new DateTime(2026, 6, 1),
        };
        _db.Socios.Add(s);
        _db.SaveChanges();
        return s;
    }

    private void AgregarCuota(int socioId, decimal importe, ModalidadCuota modalidad, bool activa, DateTime inicio)
    {
        _db.Colaboraciones.Add(new CuotaDomiciliada
        {
            SocioId = socioId, Importe = importe, Modalidad = modalidad, Iban = "ES9121000418450200051332",
            Activa = activa, FechaInicio = inicio, FechaFin = activa ? null : inicio.AddMonths(1),
        });
        _db.SaveChanges();
    }

    private void AgregarAportacion(int socioId, decimal importe, DateTime inicio)
    {
        _db.Colaboraciones.Add(new AportacionUnica { SocioId = socioId, Importe = importe, Activa = true, FechaInicio = inicio, Fecha = inicio });
        _db.SaveChanges();
    }

    [Fact]
    public async Task RecurrenteMensual_SoloCuotasActivas_NormalizaAnualAMensual()
    {
        var s = NuevoSocio();
        AgregarCuota(s.Id, 10m, ModalidadCuota.Mensual, activa: true, new DateTime(2026, 1, 10)); // +10 /mes
        AgregarCuota(s.Id, 120m, ModalidadCuota.Anual, activa: true, new DateTime(2026, 2, 10));   // 120/12 = +10 /mes
        AgregarCuota(s.Id, 50m, ModalidadCuota.Mensual, activa: false, new DateTime(2026, 3, 10)); // inactiva: no cuenta
        AgregarAportacion(s.Id, 500m, new DateTime(2026, 1, 5));                                    // única: no recurrente

        var r = await _sut.ObtenerAsync();

        Assert.Equal(20m, r.IngresoRecurrenteMensual); // 10 + 10
    }

    [Fact]
    public async Task IngresosPorTipo_DesglosaSoloActivas()
    {
        var s = NuevoSocio();
        AgregarCuota(s.Id, 10m, ModalidadCuota.Mensual, activa: true, new DateTime(2026, 1, 10));
        AgregarAportacion(s.Id, 500m, new DateTime(2026, 1, 5));
        _db.Colaboraciones.Add(new Teaming { SocioId = s.Id, Importe = 1m, Activa = true, FechaInicio = new DateTime(2026, 1, 20) });
        _db.SaveChanges();

        var r = await _sut.ObtenerAsync();

        Assert.Equal(10m, r.IngresosPorTipo[TipoColaboracion.CuotaDomiciliada]);
        Assert.Equal(500m, r.IngresosPorTipo[TipoColaboracion.AportacionUnica]);
        Assert.Equal(1m, r.IngresosPorTipo[TipoColaboracion.Teaming]);
    }

    [Fact]
    public async Task SociosActivosConColaboracion_CuentaSocioActivoConColaboracionActiva()
    {
        var activo = NuevoSocio(activo: true);
        AgregarCuota(activo.Id, 10m, ModalidadCuota.Mensual, activa: true, new DateTime(2026, 1, 10));

        var deBaja = NuevoSocio(activo: false); // socio de baja: no cuenta aunque tenga colaboración activa
        AgregarCuota(deBaja.Id, 10m, ModalidadCuota.Mensual, activa: true, new DateTime(2026, 1, 10));

        var sinColab = NuevoSocio(activo: true); // activo pero sin colaboración: no cuenta

        var soloInactiva = NuevoSocio(activo: true); // activo pero su única colaboración está inactiva
        AgregarCuota(soloInactiva.Id, 10m, ModalidadCuota.Mensual, activa: false, new DateTime(2026, 1, 10));

        var r = await _sut.ObtenerAsync();

        Assert.Equal(1, r.SociosActivosConColaboracion);
    }

    [Fact]
    public async Task AltasPorMes_AgrupaYCuentaEnOrdenCronologico()
    {
        var s = NuevoSocio();
        AgregarCuota(s.Id, 10m, ModalidadCuota.Mensual, activa: true, new DateTime(2026, 1, 5));
        AgregarAportacion(s.Id, 20m, new DateTime(2026, 1, 20));
        AgregarAportacion(s.Id, 30m, new DateTime(2026, 3, 2));

        var r = await _sut.ObtenerAsync();

        Assert.Equal(2, r.AltasPorMes.Count);
        Assert.Equal("2026-01", r.AltasPorMes[0].Mes);
        Assert.Equal(2, r.AltasPorMes[0].Cantidad);
        Assert.Equal("2026-03", r.AltasPorMes[1].Mes);
        Assert.Equal(1, r.AltasPorMes[1].Cantidad);
    }

    [Fact]
    public async Task Balance_EsIngresosMenosGastos()
    {
        var s = NuevoSocio();
        AgregarCuota(s.Id, 10m, ModalidadCuota.Mensual, activa: true, new DateTime(2026, 1, 5));
        AgregarAportacion(s.Id, 500m, new DateTime(2026, 1, 20)); // ingresos activos = 510
        _db.Gastos.Add(new Gasto { Concepto = "x", Importe = 200m, Fecha = new DateTime(2026, 1, 15), Categoria = CategoriaGasto.AccionDirecta });
        _db.SaveChanges();

        var r = await _sut.ObtenerAsync();

        Assert.Equal(510m, r.TotalIngresos);
        Assert.Equal(200m, r.TotalGastos);
        Assert.Equal(310m, r.Balance);
    }

    [Fact]
    public async Task GastosPorCategoria_AgrupaYSuma()
    {
        _db.Gastos.Add(new Gasto { Concepto = "a", Importe = 100m, Fecha = new DateTime(2026, 1, 1), Categoria = CategoriaGasto.AccionDirecta });
        _db.Gastos.Add(new Gasto { Concepto = "b", Importe = 50m, Fecha = new DateTime(2026, 1, 2), Categoria = CategoriaGasto.AccionDirecta });
        _db.Gastos.Add(new Gasto { Concepto = "c", Importe = 30m, Fecha = new DateTime(2026, 1, 3), Categoria = CategoriaGasto.Administracion });
        _db.SaveChanges();

        var r = await _sut.ObtenerAsync();

        Assert.Equal(150m, r.GastosPorCategoria[CategoriaGasto.AccionDirecta]);
        Assert.Equal(30m, r.GastosPorCategoria[CategoriaGasto.Administracion]);
        // Categorías sin gastos no tienen por qué aparecer; si aparecen, deben ser 0.
        Assert.False(r.GastosPorCategoria.TryGetValue(CategoriaGasto.Personal, out var p) && p != 0m);
    }

    [Fact]
    public async Task Proyectar_IngresosRecurrentesConstantesYGastosMediaMensual()
    {
        var s = NuevoSocio();
        // Recurrente = 10/mes (una cuota mensual activa).
        AgregarCuota(s.Id, 10m, ModalidadCuota.Mensual, activa: true, new DateTime(2026, 1, 10));
        // Gastos en 2 meses distintos: 100 (ene) y 300 (feb) -> media mensual = 200.
        _db.Gastos.Add(new Gasto { Concepto = "a", Importe = 100m, Fecha = new DateTime(2026, 1, 5), Categoria = CategoriaGasto.AccionDirecta });
        _db.Gastos.Add(new Gasto { Concepto = "b", Importe = 300m, Fecha = new DateTime(2026, 2, 5), Categoria = CategoriaGasto.AccionDirecta });
        _db.SaveChanges();

        var proy = await _sut.ProyectarAsync(new DateTime(2026, 3, 1), 3);

        Assert.Equal(3, proy.Count);
        Assert.Equal("2026-03", proy[0].Mes);
        Assert.Equal("2026-04", proy[1].Mes);
        Assert.Equal("2026-05", proy[2].Mes);
        Assert.All(proy, p => Assert.Equal(10m, p.IngresosProyectados));   // recurrente constante
        Assert.All(proy, p => Assert.Equal(200m, p.GastosProyectados));    // media 2 meses
    }

    [Fact]
    public async Task Proyectar_SinGastos_GastoProyectadoCero()
    {
        var s = NuevoSocio();
        AgregarCuota(s.Id, 25m, ModalidadCuota.Mensual, activa: true, new DateTime(2026, 1, 10));

        var proy = await _sut.ProyectarAsync(new DateTime(2026, 2, 1), 2);

        Assert.Equal(2, proy.Count);
        Assert.All(proy, p => Assert.Equal(25m, p.IngresosProyectados));
        Assert.All(proy, p => Assert.Equal(0m, p.GastosProyectados));
    }

    [Fact]
    public async Task SinDatos_DevuelveCeros()
    {
        var r = await _sut.ObtenerAsync();
        Assert.Equal(0m, r.IngresoRecurrenteMensual);
        Assert.Equal(0, r.SociosActivosConColaboracion);
        Assert.Empty(r.AltasPorMes);
        Assert.Equal(0m, r.Balance);
    }

    public void Dispose() { _db.Dispose(); _conn.Dispose(); }
}
