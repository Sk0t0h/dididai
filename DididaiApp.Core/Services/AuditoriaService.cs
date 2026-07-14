using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DididaiApp.Core.Services;

/// <summary>
/// Implementación de <see cref="IAuditoriaService"/> sobre EF Core (<see cref="AppDbContext"/>).
/// El log es solo-inserción y solo-lectura: no hay actualizar ni borrar (es inmutable).
/// </summary>
public class AuditoriaService : IAuditoriaService
{
    private readonly AppDbContext _db;

    public AuditoriaService(AppDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task RegistrarAsync(
        TipoAccionAuditoria accion, string entidad, string entidadId, string detalle, string usuario)
    {
        _db.RegistrosAuditoria.Add(new RegistroAuditoria
        {
            Fecha = DateTime.UtcNow,
            Usuario = string.IsNullOrWhiteSpace(usuario) ? "desconocido" : usuario,
            Accion = accion,
            Entidad = entidad,
            EntidadId = entidadId,
            Detalle = detalle.Length > 500 ? detalle[..500] : detalle,
        });
        await _db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<PaginaAuditoria> ListarAsync(
        string? usuario = null,
        TipoAccionAuditoria? accion = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        int pagina = 1,
        int tamanoPagina = 50)
    {
        if (pagina < 1) pagina = 1;
        if (tamanoPagina < 1) tamanoPagina = 50;

        IQueryable<RegistroAuditoria> query = _db.RegistrosAuditoria.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(usuario))
            query = query.Where(r => r.Usuario.Contains(usuario));
        if (accion is not null)
            query = query.Where(r => r.Accion == accion);
        if (desde is not null)
            query = query.Where(r => r.Fecha >= desde);
        if (hasta is not null)
            // "hasta" es inclusivo por día: se compara contra el final del día indicado.
            query = query.Where(r => r.Fecha < hasta.Value.Date.AddDays(1));

        var total = await query.CountAsync();

        var registros = await query
            .OrderByDescending(r => r.Fecha)
            .ThenByDescending(r => r.Id)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync();

        return new PaginaAuditoria(registros, total, pagina, tamanoPagina);
    }
}
