using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DididaiApp.Core.Services;

/// <summary>
/// Implementación de <see cref="IColaboracionService"/> sobre EF Core. Concentra las
/// reglas de negocio de las colaboraciones: fecha de inicio automática, validación de
/// importe e IBAN (para la cuota domiciliada) y baja lógica (finalizar sin borrar).
/// </summary>
public class ColaboracionService : IColaboracionService
{
    private readonly AppDbContext _db;

    public ColaboracionService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Colaboracion>> ListarPorSocioAsync(int socioId)
        => await _db.Colaboraciones.AsNoTracking()
            .Where(c => c.SocioId == socioId)
            .OrderByDescending(c => c.FechaInicio)
            .ToListAsync();

    public async Task<IReadOnlyList<Colaboracion>> ListarTodasAsync()
        => await _db.Colaboraciones.AsNoTracking()
            .Include(c => c.Socio)
            .OrderByDescending(c => c.FechaInicio)
            .ToListAsync();

    public Task<Colaboracion?> ObtenerAsync(int id)
        => _db.Colaboraciones.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<ResultadoColaboracion> CrearAsync(Colaboracion colaboracion)
    {
        var socioExiste = await _db.Socios.AnyAsync(s => s.Id == colaboracion.SocioId);
        if (!socioExiste)
            return ResultadoColaboracion.SocioNoEncontrado;

        if (colaboracion.Importe <= 0)
            return ResultadoColaboracion.ImporteInvalido;

        // La cuota domiciliada exige IBAN válido (mod-97). Se normaliza antes de guardar.
        if (colaboracion is CuotaDomiciliada cuota)
        {
            cuota.Iban = ValidacionIban.Normalizar(cuota.Iban);
            if (!ValidacionIban.EsValido(cuota.Iban))
                return ResultadoColaboracion.IbanInvalido;
        }

        colaboracion.FechaInicio = DateTime.UtcNow;
        colaboracion.FechaFin = null;
        colaboracion.Activa = true;
        _db.Colaboraciones.Add(colaboracion);
        await _db.SaveChangesAsync();
        return ResultadoColaboracion.Creado;
    }

    public async Task DarDeBajaAsync(int id)
    {
        var colaboracion = await _db.Colaboraciones.FirstOrDefaultAsync(c => c.Id == id);
        if (colaboracion is null || !colaboracion.Activa) return;
        colaboracion.Activa = false;
        colaboracion.FechaFin = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<ResultadoActualizacionColaboracion> ActualizarAsync(int id, decimal importe, ModalidadCuota modalidad, string? iban)
    {
        var colaboracion = await _db.Colaboraciones.FirstOrDefaultAsync(c => c.Id == id);
        if (colaboracion is null)
            return new(ResultadoColaboracion.NoEncontrada, null);

        if (importe <= 0)
            return new(ResultadoColaboracion.ImporteInvalido, null);

        var cambios = new ConstructorCambios()
            .Registrar("Importe", $"{colaboracion.Importe:0.00} €", $"{importe:0.00} €");

        // Periodicidad e IBAN solo aplican a la cuota domiciliada; en el resto se ignoran.
        if (colaboracion is CuotaDomiciliada cuota)
        {
            var norm = ValidacionIban.Normalizar(iban ?? string.Empty);
            if (!ValidacionIban.EsValido(norm))
                return new(ResultadoColaboracion.IbanInvalido, null);

            // El IBAN se registra ENMASCARADO (solo los últimos 4) para no volcar el dato
            // bancario en claro al log de auditoría.
            cambios.Registrar("Periodicidad", cuota.Modalidad, modalidad)
                   .Registrar("IBAN", Enmascarar(cuota.Iban), Enmascarar(norm));

            cuota.Iban = norm;
            cuota.Modalidad = modalidad;
        }

        colaboracion.Importe = importe;
        await _db.SaveChangesAsync();
        return new(ResultadoColaboracion.Creado, cambios.ToJson());
    }

    /// <summary>Deja visibles solo los últimos 4 caracteres de un IBAN (resto como ••••).</summary>
    private static string Enmascarar(string? iban)
    {
        var v = (iban ?? string.Empty).Trim();
        return v.Length <= 4 ? v : new string('•', v.Length - 4) + v[^4..];
    }
}
