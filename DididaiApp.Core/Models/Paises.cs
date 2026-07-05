using System.Globalization;

namespace DididaiApp.Core.Models;

/// <summary>
/// Catálogo de países (ISO 3166-1 alpha-2) usado como fuente para el desplegable
/// del formulario de socios y para validar que el código guardado es real.
///
/// No hay tabla en BD a propósito: la lista es estándar y cuasi-estática, .NET la
/// provee vía <see cref="RegionInfo"/>, y la validez del código se garantiza con el
/// desplegable (solo ofrece códigos válidos) + la validación en servidor
/// (<see cref="EsCodigoValido"/>). Si el negocio pidiera gestionar la lista o datos
/// propios por país, se promocionaría a tabla + FK sin coste (los códigos ya son válidos).
///
/// España se ofrece primera/por defecto por ser el país mayoritario de la base de socios.
/// </summary>
public static class Paises
{
    /// <summary>Código ISO del país por defecto (España).</summary>
    public const string CodigoPorDefecto = "ES";

    /// <summary>Un país del catálogo: código ISO alpha-2 y nombre para mostrar (en español).</summary>
    public readonly record struct Pais(string Codigo, string Nombre);

    // Catálogo construido una sola vez a partir de las culturas específicas del
    // sistema: de cada una se extrae su RegionInfo (país), se deduplica por código
    // ISO y se ordena alfabéticamente por nombre en español. España va primera.
    private static readonly IReadOnlyList<Pais> _catalogo = Construir();

    private static IReadOnlyList<Pais> Construir()
    {
        var esES = CultureInfo.GetCultureInfo("es-ES");

        var paises = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(c =>
            {
                try { return new RegionInfo(c.Name); }
                catch { return null; }
            })
            .Where(r => r is not null && r!.TwoLetterISORegionName.Length == 2)
            .Select(r => r!.TwoLetterISORegionName.ToUpperInvariant())
            .Distinct()
            .Select(codigo => new Pais(codigo, NombreEnEspañol(codigo, esES)))
            .OrderBy(p => p.Nombre, StringComparer.Create(esES, ignoreCase: true))
            .ToList();

        // España primera; el resto en orden alfabético.
        var espana = paises.FirstOrDefault(p => p.Codigo == CodigoPorDefecto);
        if (espana.Codigo == CodigoPorDefecto)
        {
            paises.Remove(espana);
            paises.Insert(0, espana);
        }

        return paises;
    }

    private static string NombreEnEspañol(string codigo, CultureInfo esES)
    {
        try
        {
            // El nombre debe salir SIEMPRE en español, sin depender de la cultura del
            // proceso (en Azure/Linux el hilo podría no ser es-*). Se toma el DisplayName
            // de la RegionInfo instanciada bajo es-ES ejecutando en ese contexto de cultura.
            var previa = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = esES;
                var region = new RegionInfo(codigo);
                var nombre = region.DisplayName;
                return string.IsNullOrWhiteSpace(nombre) ? codigo : nombre;
            }
            finally
            {
                CultureInfo.CurrentCulture = previa;
            }
        }
        catch
        {
            return codigo;
        }
    }

    /// <summary>Catálogo completo (España primera, resto alfabético).</summary>
    public static IReadOnlyList<Pais> Todos => _catalogo;

    /// <summary>Indica si <paramref name="codigo"/> es un código ISO del catálogo (validación en servidor).</summary>
    public static bool EsCodigoValido(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return false;
        var c = codigo.Trim().ToUpperInvariant();
        return _catalogo.Any(p => p.Codigo == c);
    }

    /// <summary>Nombre del país para mostrar, o el propio código si no está en el catálogo.</summary>
    public static string Nombre(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return string.Empty;
        var c = codigo.Trim().ToUpperInvariant();
        var pais = _catalogo.FirstOrDefault(p => p.Codigo == c);
        return pais.Codigo == c ? pais.Nombre : c;
    }
}
