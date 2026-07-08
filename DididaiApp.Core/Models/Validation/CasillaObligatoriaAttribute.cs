using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DididaiApp.Core.Models.Validation;

/// <summary>
/// Exige que una casilla (bool) esté marcada (true). Pensado para consentimientos
/// obligatorios (RGPD). Valida en servidor y, como <see cref="IClientModelValidator"/>,
/// emite una regla de cliente propia (<c>casillaobligatoria</c>) cuyo adaptador
/// jquery-validation vive en <c>validacion-socio.js</c> y comprueba el estado
/// <c>checked</c> del input.
///
/// <para>NO se emite <c>data-val-required</c> ni se usa <c>[Range(bool)]</c>: para un
/// bool, ASP.NET renderiza el checkbox MÁS un input oculto <c>value="false"</c> con el
/// mismo name; jquery-validation ve ese hidden "con valor" y da <c>required</c> por
/// satisfecho aunque la casilla esté desmarcada. Validar por <c>checked</c> lo evita.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class CasillaObligatoriaAttribute : ValidationAttribute, IClientModelValidator
{
    public CasillaObligatoriaAttribute()
        => ErrorMessage = "Debes marcar esta casilla para continuar.";

    public override bool IsValid(object? value) => value is true;

    public void AddValidation(ClientModelValidationContext context)
    {
        // ASP.NET añade un 'required' implícito a los bool no-nullable. Sobre un checkbox
        // ese required es doblemente nocivo: trae mensaje en inglés y su hidden
        // value="false" lo da por satisfecho aunque la casilla esté desmarcada. Se
        // elimina para dejar SOLO la regla 'casillaobligatoria' (valida por 'checked').
        context.Attributes.Remove("data-val-required");

        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-casillaobligatoria", ErrorMessage!);
    }

    private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (!attributes.ContainsKey(key))
            attributes.Add(key, value);
    }
}
