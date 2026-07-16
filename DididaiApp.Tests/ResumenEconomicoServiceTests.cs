using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DididaiApp.Tests;

/// <summary>
/// Pruebas del cálculo económico (TDD) con el modelo de DEVENGO por rango: cada
/// ingreso/gasto cuenta lo que realmente genera entre dos fechas (las recurrentes por
/// meses vivos, las puntuales una vez en su fecha). Además: ritmo recurrente (foto
/// instantánea), series mensuales y proyección. SQLite en memoria.
/// </summary>
public class ResumenEconomicoServiceTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly ResumenEconomicoService _sut;

    // Rango de referencia: año natural 2026 completo.
    private static readonly DateTime Ene = new(2026, 1, 1);
    private static readonly DateTime Dic = new(2026, 12, 31);

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

    private void AgregarCuota(int socioId, decimal importe, ModalidadCuota modalidad, bool activa, DateTime inicio, DateTime? fin = null)
    {
        _db.Colaboraciones.Add(new CuotaDomiciliada
        {
            SocioId = socioId, Importe = importe, Modalidad = modalidad, Iban = "ES9121000418450200051332",
            Activa = activa, FechaInicio = inicio, FechaFin = activa ? fin : (fin ?? inicio.AddMonths(1)),
        });
        _db.SaveChanges();
    }

    private void AgregarAportacion(int socioId, decimal importe, DateTime fecha)
    {
        _db.Colaboraciones.Add(new AportacionUnica { SocioId = socioId, Importe = importe, Activa = true, FechaInicio = fecha, Fecha = fecha });
        _db.SaveChanges();
    }

    private void AgregarTeaming(int socioId, decimal importe, DateTime inicio, DateTime? fin = null)
    {
        _db.Colaboraciones.Add(new Teaming { SocioId = socioId, Importe = importe, Activa = fin is null, FechaInicio = inicio, FechaFin = fin });
        _db.SaveChanges();
    }

    private void AgregarGasto(decimal importe, DateTime fecha, PeriodicidadGasto periodicidad = PeriodicidadGasto.Puntual, DateTime? fin = null, CategoriaGasto cat = CategoriaGasto.AccionDirecta)
    {
        _db.Gastos.Add(new Gasto { Concepto = "x", Importe = importe, Fecha = fecha, Periodicidad = periodicidad, FechaFin = fin, Categoria = cat });
        _db.SaveChanges();
    }

    // --- DEVENGO DE INGRESOS ----------------------------------------------------------

    [Fact]
    public async Task CuotaMensual_DevengaImportePorCadaMesVivoEnElRango()
    {
        var s = NuevoSocio();
        // Cuota mensual de 10 €, viva de marzo a agosto (6 meses) de 2026.
        AgregarCuota(s.Id, 10m, ModalidadCuota.Mensual, activa: false, new DateTime(2026, 3, 1), new DateTime(2026, 8, 31));

        var r = await _sut.ObtenerAsync(Ene, Dic);

        Assert.Equal(60m, r.IngresosPorTipo[TipoColaboracion.CuotaDomiciliada]); // 10 × 6 meses
        Assert.Equal(60m, r.TotalIngresos);
    }

    [Fact]
    public async Task CuotaAnual_DevengaImporteDoceavoPorMesVivo()
    {
        var s = NuevoSocio();
        // Cuota anual de 120 € (=10/mes), viva todo el año -> 120.
        AgregarCuota(s.Id, 120m, ModalidadCuota.Anual, activa: true, new DateTime(2026, 1, 1));

        var r = await _sut.ObtenerAsync(Ene, Dic);

        Assert.Equal(120m, r.IngresosPorTipo[TipoColaboracion.CuotaDomiciliada]); // 10/mes × 12
    }

    [Fact]
    public async Task CuotaAnual_ParcialEnElRango_DevengaSoloLosMesesVivos()
    {
        var s = NuevoSocio();
        // Anual de 120 (=10/mes) que empieza en octubre -> solo oct, nov, dic = 30.
        AgregarCuota(s.Id, 120m, ModalidadCuota.Anual, activa: true, new DateTime(2026, 10, 1));

        var r = await _sut.ObtenerAsync(Ene, Dic);

        Assert.Equal(30m, r.TotalIngresos); // 10 × 3 meses
    }

    [Fact]
    public async Task AportacionUnica_DevengaSoloSiSuFechaCaeEnElRango()
    {
        var s = NuevoSocio();
        AgregarAportacion(s.Id, 500m, new DateTime(2026, 5, 10));  // dentro
        AgregarAportacion(s.Id, 999m, new DateTime(2025, 5, 10));  // fuera (año anterior)

        var r = await _sut.ObtenerAsync(Ene, Dic);

        Assert.Equal(500m, r.IngresosPorTipo[TipoColaboracion.AportacionUnica]);
        Assert.Equal(500m, r.TotalIngresos);
    }

    [Fact]
    public async Task Teaming_DevengaPorMesVivo()
    {
        var s = NuevoSocio();
        AgregarTeaming(s.Id, 1m, new DateTime(2026, 1, 1)); // 1 €/mes todo el año = 12

        var r = await _sut.ObtenerAsync(Ene, Dic);

        Assert.Equal(12m, r.IngresosPorTipo[TipoColaboracion.Teaming]);
    }

    [Fact]
    public async Task IngresosPorTipo_ConDevengo_LasTresMagnitudesSonComparables()
    {
        var s = NuevoSocio();
        AgregarCuota(s.Id, 10m, ModalidadCuota.Mensual, activa: true, new DateTime(2026, 1, 1)); // 120/año
        AgregarTeaming(s.Id, 1m, new DateTime(2026, 1, 1));                                        // 12/año
        AgregarAportacion(s.Id, 50m, new DateTime(2026, 6, 1));                                    // 50 puntual

        var r = await _sut.ObtenerAsync(Ene, Dic);

        Assert.Equal(120m, r.IngresosPorTipo[TipoColaboracion.CuotaDomiciliada]);
        Assert.Equal(12m, r.IngresosPorTipo[TipoColaboracion.Teaming]);
        Assert.Equal(50m, r.IngresosPorTipo[TipoColaboracion.AportacionUnica]);
        Assert.Equal(182m, r.TotalIngresos);
    }

    // --- DEVENGO DE GASTOS ------------------------------------------------------------

    [Fact]
    public async Task GastoPuntual_CuentaUnaVezSiEstaEnRango()
    {
        AgregarGasto(200m, new DateTime(2026, 4, 1), PeriodicidadGasto.Puntual);
        AgregarGasto(999m, new DateTime(2025, 4, 1), PeriodicidadGasto.Puntual); // fuera

        var r = await _sut.ObtenerAsync(Ene, Dic);

        Assert.Equal(200m, r.TotalGastos);
    }

    [Fact]
    public async Task GastoMensual_DevengaCadaMesVivo()
    {
        // Alquiler de 800 €/mes vivo todo el año -> 9600.
        AgregarGasto(800m, new DateTime(2026, 1, 1), PeriodicidadGasto.Mensual);

        var r = await _sut.ObtenerAsync(Ene, Dic);

        Assert.Equal(9600m, r.TotalGastos); // 800 × 12
    }

    [Fact]
    public async Task GastoAnual_DevengaDoceavoPorMes()
    {
        // Seguro anual de 1200 € (=100/mes) vivo todo el año -> 1200.
        AgregarGasto(1200m, new DateTime(2026, 1, 1), PeriodicidadGasto.Anual);

        var r = await _sut.ObtenerAsync(Ene, Dic);

        Assert.Equal(1200m, r.TotalGastos);
    }

    [Fact]
    public async Task Balance_EsIngresosMenosGastosDevengadosEnElRango()
    {
        var s = NuevoSocio();
        AgregarCuota(s.Id, 100m, ModalidadCuota.Mensual, activa: true, new DateTime(2026, 1, 1)); // 1200/año
        AgregarGasto(800m, new DateTime(2026, 1, 1), PeriodicidadGasto.Mensual);                   // 9600/año

        var r = await _sut.ObtenerAsync(Ene, Dic);

        Assert.Equal(1200m, r.TotalIngresos);
        Assert.Equal(9600m, r.TotalGastos);
        Assert.Equal(-8400m, r.Balance);
    }

    [Fact]
    public async Task GastosPorCategoria_UsaDevengoYOmiteCategoriasSinGasto()
    {
        AgregarGasto(100m, new DateTime(2026, 1, 1), PeriodicidadGasto.Mensual, cat: CategoriaGasto.AccionDirecta); // 1200
        AgregarGasto(30m, new DateTime(2026, 1, 3), PeriodicidadGasto.Puntual, cat: CategoriaGasto.Administracion); // 30

        var r = await _sut.ObtenerAsync(Ene, Dic);

        Assert.Equal(1200m, r.GastosPorCategoria[CategoriaGasto.AccionDirecta]);
        Assert.Equal(30m, r.GastosPorCategoria[CategoriaGasto.Administracion]);
        Assert.False(r.GastosPorCategoria.ContainsKey(CategoriaGasto.Personal));
    }

    // --- RITMO RECURRENTE (foto instantánea, no depende del rango) ---------------------

    [Fact]
    public async Task RecurrenteMensual_SoloCuotasActivas_NormalizaAnualAMensual()
    {
        var s = NuevoSocio();
        AgregarCuota(s.Id, 10m, ModalidadCuota.Mensual, activa: true, new DateTime(2026, 1, 10)); // +10
        AgregarCuota(s.Id, 120m, ModalidadCuota.Anual, activa: true, new DateTime(2026, 2, 10));  // 120/12 = +10
        AgregarCuota(s.Id, 50m, ModalidadCuota.Mensual, activa: false, new DateTime(2026, 3, 10)); // inactiva: no
        AgregarAportacion(s.Id, 500m, new DateTime(2026, 1, 5));                                    // única: no

        var r = await _sut.ObtenerAsync(Ene, Dic);

        Assert.Equal(20m, r.IngresoRecurrenteMensual);
        Assert.Equal(240m, r.IngresoRecurrenteAnual);
    }

    [Fact]
    public async Task SociosActivosConColaboracion_CuentaSocioActivoConColaboracionActiva()
    {
        var activo = NuevoSocio(activo: true);
        AgregarCuota(activo.Id, 10m, ModalidadCuota.Mensual, activa: true, new DateTime(2026, 1, 10));
        var deBaja = NuevoSocio(activo: false);
        AgregarCuota(deBaja.Id, 10m, ModalidadCuota.Mensual, activa: true, new DateTime(2026, 1, 10));
        NuevoSocio(activo: true); // sin colaboración
        var soloInactiva = NuevoSocio(activo: true);
        AgregarCuota(soloInactiva.Id, 10m, ModalidadCuota.Mensual, activa: false, new DateTime(2026, 1, 10));

        var r = await _sut.ObtenerAsync(Ene, Dic);

        Assert.Equal(1, r.SociosActivosConColaboracion);
    }

    // --- SERIES MENSUALES -------------------------------------------------------------

    [Fact]
    public async Task AltasPorMes_CuentaColaboracionesIniciadasCadaMesDelRango()
    {
        var s = NuevoSocio();
        AgregarCuota(s.Id, 10m, ModalidadCuota.Mensual, activa: true, new DateTime(2026, 1, 5));
        AgregarAportacion(s.Id, 20m, new DateTime(2026, 1, 20));
        AgregarAportacion(s.Id, 30m, new DateTime(2026, 3, 2));

        var r = await _sut.ObtenerAsync(Ene, Dic);

        Assert.Equal(12, r.AltasPorMes.Count);                 // un punto por mes del rango
        Assert.Equal("2026-01", r.AltasPorMes[0].Mes);
        Assert.Equal(2, r.AltasPorMes[0].Cantidad);            // enero: cuota + aportación
        Assert.Equal(1, r.AltasPorMes[2].Cantidad);            // marzo: 1 aportación
        Assert.Equal(0, r.AltasPorMes[1].Cantidad);            // febrero: 0
    }

    [Fact]
    public async Task IngresosPorMes_ReflejaElDevengoMesAMes()
    {
        var s = NuevoSocio();
        // Cuota mensual de 10 desde marzo -> de marzo a diciembre devenga 10/mes; ene y feb = 0.
        AgregarCuota(s.Id, 10m, ModalidadCuota.Mensual, activa: true, new DateTime(2026, 3, 1));

        var r = await _sut.ObtenerAsync(Ene, Dic);

        Assert.Equal(12, r.IngresosPorMes.Count);
        Assert.Equal(0m, r.IngresosPorMes[0].Valor);   // enero
        Assert.Equal(0m, r.IngresosPorMes[1].Valor);   // febrero
        Assert.Equal(10m, r.IngresosPorMes[2].Valor);  // marzo
        Assert.Equal(10m, r.IngresosPorMes[11].Valor); // diciembre
    }

    // --- PROYECCIÓN -------------------------------------------------------------------

    [Fact]
    public async Task Proyectar_IngresoRecurrenteConstante_GastoRecurrenteDevengado()
    {
        var s = NuevoSocio();
        AgregarCuota(s.Id, 10m, ModalidadCuota.Mensual, activa: true, new DateTime(2026, 1, 10)); // recurrente 10/mes
        AgregarGasto(200m, new DateTime(2026, 1, 1), PeriodicidadGasto.Mensual);                   // gasto recurrente 200/mes

        var proy = await _sut.ProyectarAsync(new DateTime(2026, 3, 1), 3);

        Assert.Equal(3, proy.Count);
        Assert.All(proy, p => Assert.Equal(10m, p.IngresosProyectados));
        Assert.All(proy, p => Assert.Equal(200m, p.GastosProyectados)); // gasto mensual vigente
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
        var r = await _sut.ObtenerAsync(Ene, Dic);
        Assert.Equal(0m, r.IngresoRecurrenteMensual);
        Assert.Equal(0, r.SociosActivosConColaboracion);
        Assert.Equal(0m, r.TotalIngresos);
        Assert.Equal(0m, r.Balance);
    }

    public void Dispose() { _db.Dispose(); _conn.Dispose(); }
}
