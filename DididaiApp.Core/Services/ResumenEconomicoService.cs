using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DididaiApp.Core.Services;

/// <summary>
/// Implementación de <see cref="IResumenEconomicoService"/> sobre EF Core. Las
/// agregaciones que SQLite no resuelve bien en el servidor (agrupar por mes) se
/// hacen en memoria tras traer los datos: el volumen de una ONG pequeña lo permite.
/// </summary>
public class ResumenEconomicoService : IResumenEconomicoService
{
    private readonly AppDbContext _db;

    public ResumenEconomicoService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ProyeccionMes>> ProyectarAsync(DateTime desde, int meses)
    {
        if (meses < 1) return [];

        // Ingresos proyectados/mes: el recurrente actual (cuotas domiciliadas activas, anual/12).
        var cuotasActivas = await _db.Colaboraciones.AsNoTracking()
            .Where(c => c.Activa).OfType<CuotaDomiciliada>().ToListAsync();
        decimal ingresoMes = cuotasActivas
            .Sum(c => c.Modalidad == ModalidadCuota.Anual ? c.Importe / 12m : c.Importe);

        // Gastos proyectados/mes: media mensual sobre los meses que tienen gasto (si no hay, 0).
        var gastos = await _db.Gastos.AsNoTracking().Select(g => new { g.Importe, g.Fecha }).ToListAsync();
        decimal gastoMes = 0m;
        if (gastos.Count > 0)
        {
            int mesesConGasto = gastos.Select(g => $"{g.Fecha.Year:D4}-{g.Fecha.Month:D2}").Distinct().Count();
            gastoMes = gastos.Sum(g => g.Importe) / mesesConGasto;
        }

        var serie = new List<ProyeccionMes>(meses);
        for (int i = 0; i < meses; i++)
        {
            var mes = desde.AddMonths(i);
            serie.Add(new ProyeccionMes($"{mes.Year:D4}-{mes.Month:D2}", ingresoMes, gastoMes));
        }
        return serie;
    }

    public async Task<ResumenEconomico> ObtenerAsync()
    {
        // Colaboraciones activas (los ingresos vivos). Se traen a memoria para poder
        // usar el patrón de tipos (is CuotaDomiciliada) y agrupar por mes sin fricción.
        var activas = await _db.Colaboraciones.AsNoTracking().Where(c => c.Activa).ToListAsync();

        // 1) Ingreso recurrente mensual: solo cuotas domiciliadas activas, anual/12.
        decimal recurrente = activas
            .OfType<CuotaDomiciliada>()
            .Sum(c => c.Modalidad == ModalidadCuota.Anual ? c.Importe / 12m : c.Importe);

        // 2) Ingresos por tipo (solo activas).
        var porTipo = new Dictionary<TipoColaboracion, decimal>
        {
            [TipoColaboracion.CuotaDomiciliada] = activas.OfType<CuotaDomiciliada>().Sum(c => c.Importe),
            [TipoColaboracion.AportacionUnica] = activas.OfType<AportacionUnica>().Sum(c => c.Importe),
            [TipoColaboracion.Teaming] = activas.OfType<Teaming>().Sum(c => c.Importe),
        };

        // 3) Socios activos (no de baja) con al menos una colaboración activa.
        var socioIdsActivos = await _db.Socios.AsNoTracking()
            .Where(s => s.FechaBaja == null).Select(s => s.Id).ToListAsync();
        int sociosConColab = activas
            .Select(c => c.SocioId)
            .Where(id => socioIdsActivos.Contains(id))
            .Distinct()
            .Count();

        // 4) Altas de colaboraciones por mes (todas, activas o no), orden cronológico.
        var fechas = await _db.Colaboraciones.AsNoTracking().Select(c => c.FechaInicio).ToListAsync();
        var altasPorMes = fechas
            .GroupBy(f => $"{f.Year:D4}-{f.Month:D2}")
            .OrderBy(g => g.Key)
            .Select(g => new AltasMes(g.Key, g.Count()))
            .ToList();

        // Totales y balance.
        decimal totalIngresos = activas.Sum(c => c.Importe);
        var gastos = await _db.Gastos.AsNoTracking().ToListAsync();
        decimal totalGastos = gastos.Sum(g => g.Importe);
        var gastosPorCategoria = gastos
            .GroupBy(g => g.Categoria)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Importe));

        return new ResumenEconomico
        {
            IngresoRecurrenteMensual = recurrente,
            IngresosPorTipo = porTipo,
            SociosActivosConColaboracion = sociosConColab,
            AltasPorMes = altasPorMes,
            TotalIngresos = totalIngresos,
            TotalGastos = totalGastos,
            GastosPorCategoria = gastosPorCategoria,
        };
    }
}
