using DididaiApp.Core.Models;

namespace DididaiApp.Core.Services;

/// <summary>
/// Gestión de los gastos de la ONG (contrapartida de los ingresos en el módulo
/// económico). CRUD simple; la agregación económica vive en
/// <see cref="IResumenEconomicoService"/>.
/// </summary>
public interface IGastoService
{
    /// <summary>Lista los gastos, más recientes primero.</summary>
    Task<IReadOnlyList<Gasto>> ListarAsync();

    /// <summary>Obtiene un gasto por id, o <c>null</c> si no existe.</summary>
    Task<Gasto?> ObtenerAsync(int id);

    /// <summary>Crea un gasto. Devuelve <c>false</c> si el importe no es válido (&lt;= 0).</summary>
    Task<bool> CrearAsync(Gasto gasto);

    /// <summary>Elimina un gasto por id (borrado físico: un gasto mal metido se corrige quitándolo).</summary>
    Task EliminarAsync(int id);
}
