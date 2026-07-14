using System.Text.Json;

namespace DididaiApp.Core.Models;

/// <summary>
/// Acumula los cambios campo-a-campo de una edición (valor antes → valor después) para
/// registrarlos en la auditoría. Solo guarda los campos que realmente cambiaron; si no
/// cambió nada, <see cref="ToJson"/> devuelve <c>null</c>.
///
/// <para>El resultado es un JSON de la forma
/// <c>{"Apellidos":{"antes":"Pérez","despues":"García"}}</c>, pensado para leerse formateado
/// en la ficha de auditoría, no para consultarse por SQL.</para>
/// </summary>
public sealed class ConstructorCambios
{
    private readonly Dictionary<string, ValorCambiado> _cambios = new();

    /// <summary>Un valor que cambió: su estado anterior y el nuevo (como texto legible).</summary>
    public sealed record ValorCambiado(string? antes, string? despues);

    /// <summary>
    /// Registra el cambio de un campo si <paramref name="antes"/> y <paramref name="despues"/>
    /// difieren. La comparación es sobre el texto ya normalizado que se va a persistir.
    /// </summary>
    public ConstructorCambios Registrar(string campo, string? antes, string? despues)
    {
        var a = antes ?? string.Empty;
        var d = despues ?? string.Empty;
        if (!string.Equals(a, d, StringComparison.Ordinal))
            _cambios[campo] = new ValorCambiado(antes, despues);
        return this;
    }

    /// <summary>Sobrecarga para valores no-string (se comparan y guardan por su texto).</summary>
    public ConstructorCambios Registrar<T>(string campo, T antes, T despues)
    {
        var a = antes?.ToString();
        var d = despues?.ToString();
        return Registrar(campo, a, d);
    }

    /// <summary>¿Hubo al menos un cambio?</summary>
    public bool HayCambios => _cambios.Count > 0;

    /// <summary>
    /// Serializa los cambios a JSON, o devuelve <c>null</c> si no hubo ninguno (para no
    /// guardar ruido en el log cuando una edición se envía sin modificar nada).
    /// </summary>
    public string? ToJson() =>
        _cambios.Count == 0 ? null : JsonSerializer.Serialize(_cambios);
}
