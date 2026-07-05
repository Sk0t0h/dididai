using System.Text.RegularExpressions;

namespace DididaiApp.Core.Models;

/// <summary>
/// Validaciones de datos de identidad de un socio. El criterio es deliberado: la
/// base de socios es internacional (decisión previa), así que el formato estricto
/// solo se aplica cuando el propio socio declara un documento con formato conocido
/// —el <see cref="TipoDocumento.DniEspanol"/> o el <see cref="TipoDocumento.Nie"/>—;
/// pasaporte y "otro" se exigen pero no se validan de formato. La validación la
/// dispara el TIPO de documento declarado, no el país de residencia (así un español
/// residente fuera declara "DNI español" y se le valida la letra igualmente). El
/// teléfono se valida en formato internacional (E.164) para todos por igual.
/// </summary>
public static partial class ValidacionIdentidad
{
    private const string LetrasDni = "TRWAGMYFPDXBNJZSQVHLCKE";

    /// <summary>
    /// Valida el documento de identidad según el tipo declarado: DNI español o NIE
    /// exigen la letra de control correcta; pasaporte u "otro" solo que no esté vacío.
    /// </summary>
    public static bool DocumentoValido(string? documento, TipoDocumento tipo)
    {
        var doc = (documento ?? string.Empty).Trim().ToUpperInvariant();
        if (doc.Length == 0) return false;

        return tipo switch
        {
            TipoDocumento.DniEspanol => DniEspanolValido(doc),
            TipoDocumento.Nie => NieEspanolValido(doc),
            _ => true, // Pasaporte / Otro: laxo (solo presencia).
        };
    }

    /// <summary>DNI español: 8 dígitos + letra de control (algoritmo módulo 23).</summary>
    public static bool DniEspanolValido(string doc)
    {
        var m = DniRegex().Match(doc);
        if (!m.Success) return false;
        var numero = int.Parse(m.Groups["num"].Value);
        return LetrasDni[numero % 23] == m.Groups["letra"].Value[0];
    }

    /// <summary>NIE español: X/Y/Z + 7 dígitos + letra (X→0, Y→1, Z→2 y luego como el DNI).</summary>
    public static bool NieEspanolValido(string doc)
    {
        var m = NieRegex().Match(doc);
        if (!m.Success) return false;
        var prefijo = m.Groups["pre"].Value[0];
        var digitoPrefijo = prefijo switch { 'X' => "0", 'Y' => "1", 'Z' => "2", _ => null };
        if (digitoPrefijo is null) return false;
        var numero = int.Parse(digitoPrefijo + m.Groups["num"].Value);
        return LetrasDni[numero % 23] == m.Groups["letra"].Value[0];
    }

    /// <summary>
    /// Valida el teléfono en formato E.164: prefijo internacional <c>+</c> seguido de
    /// entre 8 y 15 dígitos (el primero, no cero). Universal, no atado a España.
    /// Se valida sobre el número ya normalizado (sin espacios ni separadores).
    /// </summary>
    public static bool TelefonoValido(string? telefono)
    {
        var t = NormalizarTelefono(telefono);
        return E164Regex().IsMatch(t);
    }

    /// <summary>Quita espacios y separadores habituales del teléfono, conservando el <c>+</c> inicial.</summary>
    public static string NormalizarTelefono(string? telefono)
        => SeparadoresTelefono().Replace((telefono ?? string.Empty).Trim(), string.Empty);

    [GeneratedRegex(@"^(?<num>\d{8})(?<letra>[A-Z])$")]
    private static partial Regex DniRegex();

    [GeneratedRegex(@"^(?<pre>[XYZ])(?<num>\d{7})(?<letra>[A-Z])$")]
    private static partial Regex NieRegex();

    [GeneratedRegex(@"^\+[1-9]\d{7,14}$")]
    private static partial Regex E164Regex();

    [GeneratedRegex(@"[\s\-\.\(\)]")]
    private static partial Regex SeparadoresTelefono();
}
