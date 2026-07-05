using System.Numerics;
using System.Text;

namespace DididaiApp.Core.Models;

/// <summary>
/// Validación de IBAN según ISO 13616 (algoritmo mod-97 de la norma ISO 7064).
/// Internacional, no atada a España: se comprueba el prefijo de país, la longitud
/// registrada para ese país y los dígitos de control. La base de socios es
/// internacional (decisión previa), de ahí que no se restrinja a IBAN español.
/// </summary>
public static class ValidacionIban
{
    // Longitud total del IBAN por país (ISO 13616). Lista de los países de uso más
    // probable para la ONG; si un país no está registrado aquí, se rechaza (no se
    // puede comprobar la longitud con fiabilidad). Ampliable añadiendo entradas.
    private static readonly Dictionary<string, int> LongitudPorPais = new()
    {
        ["ES"] = 24, ["GB"] = 22, ["DE"] = 22, ["FR"] = 27, ["IT"] = 27, ["PT"] = 25,
        ["NL"] = 18, ["BE"] = 16, ["IE"] = 22, ["CH"] = 21, ["AT"] = 20, ["SE"] = 24,
        ["NO"] = 15, ["DK"] = 18, ["FI"] = 18, ["PL"] = 28, ["GR"] = 27, ["LU"] = 20,
    };

    /// <summary>Quita espacios y pasa a mayúsculas (formato canónico del IBAN).</summary>
    public static string Normalizar(string? iban)
        => (iban ?? string.Empty).Replace(" ", string.Empty).Trim().ToUpperInvariant();

    /// <summary>
    /// Indica si el IBAN es válido: formato (2 letras país + 2 dígitos control + BBAN
    /// alfanumérico), longitud registrada para el país y dígitos de control (mod-97).
    /// </summary>
    public static bool EsValido(string? iban)
    {
        var s = Normalizar(iban);

        // Estructura mínima: 2 letras (país) + 2 dígitos (control) + resto alfanumérico.
        if (s.Length < 4 || s.Length > 34) return false;
        if (!char.IsLetter(s[0]) || !char.IsLetter(s[1])) return false;
        if (!char.IsDigit(s[2]) || !char.IsDigit(s[3])) return false;

        var pais = s[..2];
        if (!LongitudPorPais.TryGetValue(pais, out var longitud) || s.Length != longitud)
            return false;

        for (int i = 4; i < s.Length; i++)
            if (!char.IsLetterOrDigit(s[i])) return false;

        return Mod97(s) == 1;
    }

    // mod-97 (ISO 7064): mueve los 4 primeros caracteres al final, convierte letras a
    // números (A=10 … Z=35) y calcula el módulo 97 del entero resultante. Válido si es 1.
    private static int Mod97(string iban)
    {
        var reordenado = iban[4..] + iban[..4];
        var sb = new StringBuilder(reordenado.Length * 2);
        foreach (var c in reordenado)
        {
            if (char.IsDigit(c)) sb.Append(c);
            else sb.Append((c - 'A' + 10).ToString());
        }
        var numero = BigInteger.Parse(sb.ToString());
        return (int)(numero % 97);
    }
}
