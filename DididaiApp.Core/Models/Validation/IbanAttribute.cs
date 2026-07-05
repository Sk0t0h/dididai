using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DididaiApp.Core.Models.Validation;

/// <summary>
/// Valida que el valor sea un IBAN correcto (mod-97, ISO 13616) delegando en
/// <see cref="ValidacionIban"/>. Implementa <see cref="IClientModelValidator"/> para
/// emitir los <c>data-val-*</c> y aplicar la misma regla en cliente (adaptador en
/// validacion-socio.js / el JS de colaboraciones). La obligatoriedad la cubre
/// <c>[Required]</c> aparte; aquí un valor vacío no se considera error de formato.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class IbanAttribute : ValidationAttribute, IClientModelValidator
{
    public IbanAttribute()
        => ErrorMessage = "El IBAN no es válido (revisa el país, la longitud y los dígitos de control).";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var iban = value as string;
        if (string.IsNullOrWhiteSpace(iban))
            return ValidationResult.Success;

        return ValidacionIban.EsValido(iban)
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage, [validationContext.MemberName!]);
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-iban", ErrorMessage!);
    }

    private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (!attributes.ContainsKey(key))
            attributes.Add(key, value);
    }
}
