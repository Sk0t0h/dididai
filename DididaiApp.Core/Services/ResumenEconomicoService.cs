using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DididaiApp.Core.Services;

/// <summary>
/// Implementación de <see cref="IResumenEconomicoService"/> sobre EF Core. Las
/// agregaciones se hacen en memoria tras traer los datos (el volumen de una ONG
/// pequeña lo permite) para poder aplicar el patrón de tipos (TPH) y el cálculo de
/// devengo por meses sin fricción de SQL.
///
/// <para><b>Modelo de devengo.</b> Una colaboración o gasto <i>recurrente</i> devenga su
/// importe en cada mes del rango consultado en que está vivo (su período
/// [inicio, fin] solapa ese mes). Mensual devenga el importe entero; anual, importe/12.
/// Los importes <i>puntuales</i> (aportación única, gasto puntual) devengan una sola vez,
/// en el mes de su fecha, si cae dentro del rango.</para>
/// </summary>
public class ResumenEconomicoService : IResumenEconomicoService
{
    private readonly AppDbContext _db;

    public ResumenEconomicoService(AppDbContext db) => _db = db;

    // --- Utilidades de rango en meses -------------------------------------------------

    /// <summary>Primer día del mes de una fecha.</summary>
    private static DateTime InicioMes(DateTime f) => new(f.Year, f.Month, 1);

    /// <summary>Lista de "yyyy-MM" de cada mes entre desde y hasta (inclusive).</summary>
    private static List<DateTime> MesesEntre(DateTime desde, DateTime hasta)
    {
        var meses = new List<DateTime>();
        var m = InicioMes(desde);
        var fin = InicioMes(hasta);
        while (m <= fin) { meses.Add(m); m = m.AddMonths(1); }
        return meses;
    }

    /// <summary>
    /// Nº de meses del rango [desde, hasta] en que un período recurrente [inicio, fin]
    /// está vivo. <paramref name="fin"/> null = sigue vigente. Un mes cuenta si el período
    /// solapa cualquier día de ese mes.
    /// </summary>
    private static int MesesVivos(DateTime desde, DateTime hasta, DateTime inicio, DateTime? fin)
    {
        int n = 0;
        foreach (var mes in MesesEntre(desde, hasta))
        {
            var finMes = mes.AddMonths(1).AddDays(-1);
            bool empezoAntesDeAcabarElMes = inicio <= finMes;
            bool noHabiaAcabadoAlEmpezarElMes = fin is null || fin.Value >= mes;
            if (empezoAntesDeAcabarElMes && noHabiaAcabadoAlEmpezarElMes) n++;
        }
        return n;
    }

    /// <summary>Importe mensual de una cuota según su modalidad (anual → /12).</summary>
    private static decimal ImporteMensualCuota(CuotaDomiciliada c)
        => c.Modalidad == ModalidadCuota.Anual ? c.Importe / 12m : c.Importe;

    /// <summary>Importe mensual de un gasto recurrente según su periodicidad (anual → /12).</summary>
    private static decimal ImporteMensualGasto(Gasto g)
        => g.Periodicidad == PeriodicidadGasto.Anual ? g.Importe / 12m : g.Importe;

    /// <summary>True si una fecha cae dentro del rango de meses [desde, hasta] (inclusive).</summary>
    private static bool EnRango(DateTime f, DateTime desde, DateTime hasta)
        => f >= InicioMes(desde) && f <= InicioMes(hasta).AddMonths(1).AddDays(-1);

    // --- Cálculo principal ------------------------------------------------------------

    public async Task<ResumenEconomico> ObtenerAsync(DateTime desde, DateTime hasta)
    {
        // Normaliza el rango por si viene invertido.
        if (hasta < desde) (desde, hasta) = (hasta, desde);

        var colaboraciones = await _db.Colaboraciones.AsNoTracking().ToListAsync();
        var gastos = await _db.Gastos.AsNoTracking().ToListAsync();

        // 1) Ingresos DEVENGADOS en el rango, por tipo.
        decimal IngresoTipo<T>() where T : Colaboracion =>
            colaboraciones.OfType<T>().Sum(c => DevengoColaboracion(c, desde, hasta));

        var porTipo = new Dictionary<TipoColaboracion, decimal>
        {
            [TipoColaboracion.CuotaDomiciliada] = IngresoTipo<CuotaDomiciliada>(),
            [TipoColaboracion.AportacionUnica] = IngresoTipo<AportacionUnica>(),
            [TipoColaboracion.Teaming] = IngresoTipo<Teaming>(),
        };
        decimal totalIngresos = porTipo.Values.Sum();

        // 2) Gastos DEVENGADOS en el rango (total y por categoría).
        decimal totalGastos = gastos.Sum(g => DevengoGasto(g, desde, hasta));
        var gastosPorCategoria = gastos
            .GroupBy(g => g.Categoria)
            .Select(grp => (grp.Key, Suma: grp.Sum(g => DevengoGasto(g, desde, hasta))))
            .Where(x => x.Suma > 0m)
            .ToDictionary(x => x.Key, x => x.Suma);

        // 3) Ingreso recurrente mensual: foto instantánea (cuotas domiciliadas activas HOY). No usa el rango.
        decimal recurrente = colaboraciones
            .OfType<CuotaDomiciliada>()
            .Where(c => c.Activa)
            .Sum(ImporteMensualCuota);

        // 4) Socios activos (no de baja) con al menos una colaboración activa.
        var socioIdsActivos = await _db.Socios.AsNoTracking()
            .Where(s => s.FechaBaja == null).Select(s => s.Id).ToListAsync();
        int sociosConColab = colaboraciones
            .Where(c => c.Activa && socioIdsActivos.Contains(c.SocioId))
            .Select(c => c.SocioId)
            .Distinct()
            .Count();

        // 5) Series mensuales dentro del rango: altas (captación) e ingresos devengados (evolución).
        var meses = MesesEntre(desde, hasta);
        var altasPorMes = meses
            .Select(m => new AltasMes(
                $"{m.Year:D4}-{m.Month:D2}",
                colaboraciones.Count(c => InicioMes(c.FechaInicio) == m)))
            .ToList();

        var ingresosPorMes = meses
            .Select(m =>
            {
                var finMes = m.AddMonths(1).AddDays(-1);
                decimal ingresoMes = colaboraciones.Sum(c => DevengoColaboracion(c, m, finMes));
                return new SerieMes($"{m.Year:D4}-{m.Month:D2}", ingresoMes);
            })
            .ToList();

        return new ResumenEconomico
        {
            Desde = InicioMes(desde),
            Hasta = InicioMes(hasta),
            IngresoRecurrenteMensual = recurrente,
            IngresosPorTipo = porTipo,
            SociosActivosConColaboracion = sociosConColab,
            AltasPorMes = altasPorMes,
            IngresosPorMes = ingresosPorMes,
            TotalIngresos = totalIngresos,
            TotalGastos = totalGastos,
            GastosPorCategoria = gastosPorCategoria,
        };
    }

    /// <summary>Importe devengado por una colaboración en el rango [desde, hasta].</summary>
    private static decimal DevengoColaboracion(Colaboracion c, DateTime desde, DateTime hasta) => c switch
    {
        // Aportación única: puntual, devenga en el mes de su fecha si cae en el rango.
        AportacionUnica a => EnRango(a.Fecha, desde, hasta) ? a.Importe : 0m,
        // Cuota domiciliada: recurrente, importe mensual × meses vivos en el rango.
        CuotaDomiciliada q => ImporteMensualCuota(q) * MesesVivos(desde, hasta, q.FechaInicio, q.FechaFin),
        // Teaming: microdonación recurrente mensual, importe × meses vivos en el rango.
        Teaming t => t.Importe * MesesVivos(desde, hasta, t.FechaInicio, t.FechaFin),
        _ => 0m,
    };

    /// <summary>Importe devengado por un gasto en el rango [desde, hasta].</summary>
    private static decimal DevengoGasto(Gasto g, DateTime desde, DateTime hasta) => g.Periodicidad switch
    {
        PeriodicidadGasto.Puntual => EnRango(g.Fecha, desde, hasta) ? g.Importe : 0m,
        _ => ImporteMensualGasto(g) * MesesVivos(desde, hasta, g.Fecha, g.FechaFin),
    };

    // --- Proyección -------------------------------------------------------------------

    public async Task<IReadOnlyList<ProyeccionMes>> ProyectarAsync(DateTime desde, int meses)
    {
        if (meses < 1) return [];

        // Ingresos proyectados/mes: el recurrente actual (cuotas domiciliadas activas, anual/12).
        var cuotasActivas = await _db.Colaboraciones.AsNoTracking()
            .Where(c => c.Activa).OfType<CuotaDomiciliada>().ToListAsync();
        decimal ingresoMes = cuotasActivas.Sum(ImporteMensualCuota);

        // Gastos proyectados: para cada mes futuro, el devengo de los gastos recurrentes vigentes
        // ese mes + una parte prorrateada de los puntuales (media mensual de los puntuales del
        // último año, para que un pago puntual no distorsione un único mes de la previsión).
        var gastos = await _db.Gastos.AsNoTracking().ToListAsync();
        var puntuales = gastos.Where(g => g.Periodicidad == PeriodicidadGasto.Puntual).ToList();
        decimal mediaPuntualMes = 0m;
        if (puntuales.Count > 0)
        {
            int mesesDistintos = puntuales.Select(g => $"{g.Fecha.Year:D4}-{g.Fecha.Month:D2}").Distinct().Count();
            mediaPuntualMes = puntuales.Sum(g => g.Importe) / Math.Max(1, mesesDistintos);
        }

        var serie = new List<ProyeccionMes>(meses);
        for (int i = 0; i < meses; i++)
        {
            var mes = InicioMes(desde).AddMonths(i);
            var finMes = mes.AddMonths(1).AddDays(-1);
            decimal gastoRecurrenteMes = gastos
                .Where(g => g.Periodicidad != PeriodicidadGasto.Puntual)
                .Sum(g => DevengoGasto(g, mes, finMes));
            serie.Add(new ProyeccionMes($"{mes.Year:D4}-{mes.Month:D2}", ingresoMes, gastoRecurrenteMes + mediaPuntualMes));
        }
        return serie;
    }
}
