namespace DididaiApp.Core.Models;

/// <summary>
/// Prefijos telefónicos internacionales (código de llamada E.164) por país ISO.
/// .NET no los provee (RegionInfo no incluye el calling code), así que se mantiene
/// aquí como dato estándar y estable —mismo criterio que <see cref="Paises"/>: no
/// hay tabla en BD—. Se usa para el select de prefijo del formulario de socios.
///
/// La lista cubre los prefijos de uso más frecuente para la base de socios de la
/// ONG; no pretende ser exhaustiva de los ~200 países. Si falta uno, el usuario
/// puede seguir tecleando el número completo en E.164 (la validación no depende de
/// que el prefijo esté en esta lista, solo del formato E.164 final).
/// </summary>
public static class PrefijosTelefonicos
{
    /// <summary>Un prefijo: código de país ISO, prefijo E.164 y nombre para mostrar.</summary>
    public readonly record struct Prefijo(string PaisCodigo, string Codigo, string Nombre);

    // País por defecto (España) primero; resto ordenado por nombre.
    private static readonly IReadOnlyList<Prefijo> _lista = Construir();

    private static IReadOnlyList<Prefijo> Construir()
    {
        // (PaisISO, prefijoE164). Selección amplia con foco en Europa/EEUU/LatAm.
        var datos = new (string Pais, string Codigo)[]
        {
            ("ES", "+34"),  ("GB", "+44"),  ("FR", "+33"),  ("DE", "+49"),  ("IT", "+39"),
            ("PT", "+351"), ("NL", "+31"),  ("BE", "+32"),  ("IE", "+353"), ("CH", "+41"),
            ("AT", "+43"),  ("SE", "+46"),  ("NO", "+47"),  ("DK", "+45"),  ("FI", "+358"),
            ("PL", "+48"),  ("RO", "+40"),  ("GR", "+30"),  ("US", "+1"),   ("CA", "+1"),
            ("MX", "+52"),  ("AR", "+54"),  ("BR", "+55"),  ("CL", "+56"),  ("CO", "+57"),
            ("PE", "+51"),  ("VE", "+58"),  ("EC", "+593"), ("UY", "+598"), ("BO", "+591"),
            ("MA", "+212"), ("SN", "+221"), ("NP", "+977"), ("IN", "+91"),  ("CN", "+86"),
            ("JP", "+81"),  ("AU", "+61"),  ("NZ", "+64"),  ("ZA", "+27"),
        };

        var lista = datos
            .Select(d => new Prefijo(d.Pais, d.Codigo, $"{Paises.Nombre(d.Pais)} ({d.Codigo})"))
            .OrderBy(p => p.Nombre, StringComparer.Create(System.Globalization.CultureInfo.GetCultureInfo("es-ES"), ignoreCase: true))
            .ToList();

        var espana = lista.FirstOrDefault(p => p.PaisCodigo == Paises.CodigoPorDefecto);
        if (espana.PaisCodigo == Paises.CodigoPorDefecto)
        {
            lista.Remove(espana);
            lista.Insert(0, espana);
        }
        return lista;
    }

    /// <summary>Lista de prefijos (España primero, resto por nombre).</summary>
    public static IReadOnlyList<Prefijo> Todos => _lista;

    /// <summary>Prefijo E.164 por defecto (España, <c>+34</c>).</summary>
    public static string CodigoPorDefecto => _lista.FirstOrDefault(p => p.PaisCodigo == Paises.CodigoPorDefecto).Codigo ?? "+34";

    /// <summary>Prefijo E.164 asociado a un país ISO, o cadena vacía si no está en la lista.</summary>
    public static string CodigoDePais(string? paisCodigo)
    {
        if (string.IsNullOrWhiteSpace(paisCodigo)) return string.Empty;
        var c = paisCodigo.Trim().ToUpperInvariant();
        return _lista.FirstOrDefault(p => p.PaisCodigo == c).Codigo ?? string.Empty;
    }
}
