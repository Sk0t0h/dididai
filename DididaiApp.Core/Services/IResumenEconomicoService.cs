using DididaiApp.Core.Models;

namespace DididaiApp.Core.Services;

/// <summary>
/// Cálculos agregados del módulo económico. Distingue dos magnitudes que antes se
/// mezclaban:
/// <list type="bullet">
///   <item><b>Devengo en un rango</b>: dinero realmente generado/gastado ENTRE dos fechas.
///   Una cuota mensual viva 6 meses del rango devenga importe×6; una anual, su parte
///   proporcional; una aportación puntual, su importe solo si su fecha cae en el rango.
///   Es la magnitud coherente para "ingresos", "gastos", "balance" y el desglose por tipo.</item>
///   <item><b>Ritmo recurrente</b>: foto instantánea del compromiso mensual/anual de las
///   cuotas domiciliadas activas hoy (no depende del rango). Es el "sueldo fijo".</item>
/// </list>
/// Toda la agregación vive aquí (no en las páginas) y está cubierta por tests.
/// </summary>
public interface IResumenEconomicoService
{
    /// <summary>
    /// Resumen económico devengado en el rango <paramref name="desde"/>–<paramref name="hasta"/>
    /// (ambos inclusive, se consideran meses completos). Incluye también el ritmo recurrente
    /// actual (independiente del rango) y la evolución mensual de ingresos y de altas.
    /// </summary>
    Task<ResumenEconomico> ObtenerAsync(DateTime desde, DateTime hasta);

    /// <summary>
    /// Proyección "si todo sigue igual" de los próximos <paramref name="meses"/> meses a
    /// partir de <paramref name="desde"/> (inclusive): ingresos = recurrente mensual actual
    /// (cuotas domiciliadas activas); gastos = devengo mensual de los gastos recurrentes
    /// vigentes + prorrateo de los puntuales según su media. No es una predicción estadística.
    /// </summary>
    Task<IReadOnlyList<ProyeccionMes>> ProyectarAsync(DateTime desde, int meses);
}

/// <summary>Un mes de la proyección económica.</summary>
public record ProyeccionMes(string Mes, decimal IngresosProyectados, decimal GastosProyectados);

/// <summary>Ingresos (o cualquier serie) de un mes concreto (clave "yyyy-MM").</summary>
public record SerieMes(string Mes, decimal Valor);

/// <summary>Resultado agregado del módulo económico para un rango dado.</summary>
public record ResumenEconomico
{
    /// <summary>Rango consultado (meses completos, inclusive).</summary>
    public DateTime Desde { get; init; }
    public DateTime Hasta { get; init; }

    /// <summary>Ingreso recurrente mensual: cuotas domiciliadas activas HOY normalizadas a mes (anual/12). Foto instantánea, no depende del rango.</summary>
    public decimal IngresoRecurrenteMensual { get; init; }

    /// <summary>Ingreso recurrente anual: el mensual × 12. Foto instantánea.</summary>
    public decimal IngresoRecurrenteAnual => IngresoRecurrenteMensual * 12m;

    /// <summary>Ingresos DEVENGADOS en el rango, por tipo de colaboración.</summary>
    public IReadOnlyDictionary<TipoColaboracion, decimal> IngresosPorTipo { get; init; }
        = new Dictionary<TipoColaboracion, decimal>();

    /// <summary>Nº de socios activos (no dados de baja) con al menos una colaboración activa hoy.</summary>
    public int SociosActivosConColaboracion { get; init; }

    /// <summary>Altas de colaboraciones por mes dentro del rango (captación), orden cronológico.</summary>
    public IReadOnlyList<AltasMes> AltasPorMes { get; init; } = [];

    /// <summary>Ingresos devengados mes a mes dentro del rango (evolución), orden cronológico.</summary>
    public IReadOnlyList<SerieMes> IngresosPorMes { get; init; } = [];

    /// <summary>Gastos DEVENGADOS en el rango (recurrentes prorrateados + puntuales en su fecha).</summary>
    public decimal TotalGastos { get; init; }

    /// <summary>Gastos devengados en el rango, por categoría (solo las categorías con gasto).</summary>
    public IReadOnlyDictionary<CategoriaGasto, decimal> GastosPorCategoria { get; init; }
        = new Dictionary<CategoriaGasto, decimal>();

    /// <summary>Ingresos DEVENGADOS en el rango (todos los tipos).</summary>
    public decimal TotalIngresos { get; init; }

    /// <summary>Balance devengado en el rango = ingresos − gastos.</summary>
    public decimal Balance => TotalIngresos - TotalGastos;
}

/// <summary>Tipo de colaboración a efectos de desglose de ingresos.</summary>
public enum TipoColaboracion { CuotaDomiciliada, AportacionUnica, Teaming }

/// <summary>Altas de colaboraciones en un mes.</summary>
public record AltasMes(string Mes, int Cantidad);
