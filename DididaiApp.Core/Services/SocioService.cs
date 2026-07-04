using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DididaiApp.Core.Services;

/// <summary>
/// Implementación de <see cref="ISocioService"/> sobre EF Core (<see cref="AppDbContext"/>).
/// Concentra las reglas de negocio de los socios: fecha de alta automática,
/// unicidad de DNI y baja lógica. El DNI se normaliza (mayúsculas, sin espacios)
/// para que la unicidad no dependa del formato con que se teclee.
/// </summary>
public class SocioService : ISocioService
{
    private readonly AppDbContext _db;

    public SocioService(AppDbContext db) => _db = db;

    private static string NormalizarDni(string dni) => (dni ?? string.Empty).Trim().ToUpperInvariant();

    public async Task<IReadOnlyList<Socio>> ListarAsync(bool incluirBajas = false, string? busqueda = null)
    {
        IQueryable<Socio> query = _db.Socios.AsNoTracking();

        if (!incluirBajas)
            query = query.Where(s => s.FechaBaja == null);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.Trim();
            query = query.Where(s =>
                s.Nombre.Contains(b) ||
                s.Apellidos.Contains(b) ||
                s.Dni.Contains(b) ||
                s.Email.Contains(b));
        }

        return await query
            .OrderBy(s => s.Apellidos).ThenBy(s => s.Nombre)
            .ToListAsync();
    }

    public Task<Socio?> ObtenerAsync(int id) =>
        _db.Socios.FirstOrDefaultAsync(s => s.Id == id);

    public Task<Socio?> ObtenerPorDniAsync(string dni)
    {
        var norm = NormalizarDni(dni);
        return _db.Socios.FirstOrDefaultAsync(s => s.Dni == norm);
    }

    public async Task<ResultadoAlta> CrearAsync(Socio socio)
    {
        socio.Dni = NormalizarDni(socio.Dni);

        var existente = await _db.Socios.AsNoTracking().FirstOrDefaultAsync(s => s.Dni == socio.Dni);
        if (existente is not null)
            return existente.FechaBaja is null ? ResultadoAlta.DniDuplicadoActivo : ResultadoAlta.DniExisteDeBaja;

        socio.FechaAlta = DateTime.UtcNow;
        socio.FechaBaja = null;
        _db.Socios.Add(socio);
        await _db.SaveChangesAsync();
        return ResultadoAlta.Creado;
    }

    public async Task<ResultadoActualizacion> ActualizarAsync(Socio socio)
    {
        var actual = await _db.Socios.FirstOrDefaultAsync(s => s.Id == socio.Id);
        if (actual is null)
            return ResultadoActualizacion.NoEncontrado;

        var dni = NormalizarDni(socio.Dni);
        // ¿El DNI editado pisa a OTRO socio?
        var colision = await _db.Socios.AsNoTracking()
            .AnyAsync(s => s.Dni == dni && s.Id != socio.Id);
        if (colision)
            return ResultadoActualizacion.DniDuplicado;

        actual.Nombre = socio.Nombre;
        actual.Apellidos = socio.Apellidos;
        actual.Dni = dni;
        actual.Telefono = socio.Telefono;
        actual.Email = socio.Email;
        actual.Direccion = socio.Direccion;
        actual.CodigoPostal = socio.CodigoPostal;
        actual.Localidad = socio.Localidad;
        actual.Pais = socio.Pais;
        actual.AceptaPrivacidad = socio.AceptaPrivacidad;
        // FechaAlta y FechaBaja no se tocan aquí (la baja/reactivación tienen sus propias acciones).

        await _db.SaveChangesAsync();
        return ResultadoActualizacion.Actualizado;
    }

    public async Task DarDeBajaAsync(int id)
    {
        var socio = await _db.Socios.FirstOrDefaultAsync(s => s.Id == id);
        if (socio is null || socio.FechaBaja is not null) return;
        socio.FechaBaja = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task ReactivarAsync(int id)
    {
        var socio = await _db.Socios.FirstOrDefaultAsync(s => s.Id == id);
        if (socio is null || socio.FechaBaja is null) return;
        socio.FechaBaja = null;
        await _db.SaveChangesAsync();
    }
}
