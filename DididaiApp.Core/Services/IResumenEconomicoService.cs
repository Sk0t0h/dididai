using DididaiApp.Core.Models;

namespace DididaiApp.Core.Services;

/// <summary>
/// Cálculos agregados del módulo económico: ingresos (desde las colaboraciones),
/// gastos y las métricas para los dashboards. Toda la agregación vive aquí (no en
/// las páginas) y está cubierta por tests.
/// </summary>
public interface IResumenEconomicoService
{
    Task<ResumenEconomico> ObtenerAsync();
}

/// <summary>Resultado agregado del módulo económico en un momento dado.</summary>
public record ResumenEconomico
{
    /// <summary>Ingreso recurrente mensual: cuotas domiciliadas activas normalizadas a mes (anual/12).</summary>
    public decimal IngresoRecurrenteMensual { get; init; }

    /// <summary>Total de ingresos por tipo de colaboración (solo activas).</summary>
    public IReadOnlyDictionary<TipoColaboracion, decimal> IngresosPorTipo { get; init; }
        = new Dictionary<TipoColaboracion, decimal>();

    /// <summary>Nº de socios activos (no dados de baja) con al menos una colaboración activa.</summary>
    public int SociosActivosConColaboracion { get; init; }

    /// <summary>Altas de colaboraciones por mes (clave "yyyy-MM"), orden cronológico.</summary>
    public IReadOnlyList<AltasMes> AltasPorMes { get; init; } = [];

    /// <summary>Suma de todos los gastos registrados.</summary>
    public decimal TotalGastos { get; init; }

    /// <summary>Total de gastos por categoría (solo las categorías con gasto).</summary>
    public IReadOnlyDictionary<CategoriaGasto, decimal> GastosPorCategoria { get; init; }
        = new Dictionary<CategoriaGasto, decimal>();

    /// <summary>Suma de todos los ingresos (todas las colaboraciones activas).</summary>
    public decimal TotalIngresos { get; init; }

    /// <summary>Balance = ingresos − gastos.</summary>
    public decimal Balance => TotalIngresos - TotalGastos;
}

/// <summary>Tipo de colaboración a efectos de desglose de ingresos.</summary>
public enum TipoColaboracion { CuotaDomiciliada, AportacionUnica, Teaming }

/// <summary>Altas de colaboraciones en un mes.</summary>
public record AltasMes(string Mes, int Cantidad);
