using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DididaiApp.Core.Services;

/// <summary>
/// Implementación de <see cref="IGastoService"/> sobre EF Core. A diferencia de socios
/// y colaboraciones, el gasto usa borrado físico: no tiene ciclo de vida que conservar
/// (un gasto mal registrado se corrige eliminándolo), y no hay obligación de trazar su
/// baja como en los datos personales (RGPD) o el histórico de pagos.
/// </summary>
public class GastoService : IGastoService
{
    private readonly AppDbContext _db;

    public GastoService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Gasto>> ListarAsync()
        => await _db.Gastos.AsNoTracking().OrderByDescending(g => g.Fecha).ToListAsync();

    public Task<Gasto?> ObtenerAsync(int id)
        => _db.Gastos.FirstOrDefaultAsync(g => g.Id == id);

    public async Task<bool> CrearAsync(Gasto gasto)
    {
        if (gasto.Importe <= 0) return false;
        _db.Gastos.Add(gasto);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task EliminarAsync(int id)
    {
        var gasto = await _db.Gastos.FirstOrDefaultAsync(g => g.Id == id);
        if (gasto is null) return;
        _db.Gastos.Remove(gasto);
        await _db.SaveChangesAsync();
    }
}
