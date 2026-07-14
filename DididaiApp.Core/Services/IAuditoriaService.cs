using DididaiApp.Core.Models;

namespace DididaiApp.Core.Services;

/// <summary>
/// Una página de resultados del log de auditoría, con el total para paginar en la vista.
/// </summary>
public record PaginaAuditoria(IReadOnlyList<RegistroAuditoria> Registros, int Total, int Pagina, int TamanoPagina)
{
    public int TotalPaginas => TamanoPagina <= 0 ? 1 : (int)Math.Ceiling(Total / (double)TamanoPagina);
}

/// <summary>
/// Servicio del log de auditoría transversal: registra las acciones de gestión relevantes
/// y permite consultarlas (solo lectura). Las escrituras las disparan las páginas del back
/// tras completar cada acción, pasando el admin autenticado; el servicio no conoce la sesión.
/// </summary>
public interface IAuditoriaService
{
    /// <summary>
    /// Registra una entrada de auditoría. <paramref name="usuario"/> es el admin autenticado
    /// (lo pasa la página); la fecha (UTC) la fija el servicio. Se invoca DESPUÉS de que la
    /// acción principal ya se ha confirmado, en su propia transacción. <paramref name="cambios"/>
    /// es el detalle JSON de una edición (antes/después); <c>null</c> en el resto de acciones.
    /// </summary>
    Task RegistrarAsync(TipoAccionAuditoria accion, string entidad, string entidadId, string detalle, string usuario, string? cambios = null);

    /// <summary>
    /// Lista el log filtrado y paginado, más reciente primero. Todos los filtros son opcionales.
    /// </summary>
    Task<PaginaAuditoria> ListarAsync(
        string? usuario = null,
        TipoAccionAuditoria? accion = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        int pagina = 1,
        int tamanoPagina = 50);
}
