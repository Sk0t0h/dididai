using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DididaiApp.Core.Models.Validation;

/// <summary>
/// Valida que el teléfono esté en formato internacional E.164 (prefijo <c>+</c> y
/// 8-15 dígitos). Implementa <see cref="IClientModelValidator"/> para emitir los
/// atributos <c>data-val-*</c> que jquery-validation-unobtrusive consume en cliente,
/// de modo que la MISMA regla se aplica en cliente y servidor sin duplicarla.
/// La validación real (servidor y cliente vía regex equivalente) delega en
/// <see cref="ValidacionIdentidad"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class TelefonoE164Attribute : ValidationAttribute, IClientModelValidator
{
    // Regex equivalente a ValidacionIdentidad.TelefonoValido, para el cliente.
    public const string PatronE164 = @"^\+[1-9]\d{7,14}$";

    public TelefonoE164Attribute()
        => ErrorMessage = "El teléfono debe estar en formato internacional, con prefijo de país (p. ej. +34612345678).";

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var telefono = value as string;
        // La obligatoriedad la cubre [Required] aparte; aquí, si viene vacío, no se
        // valida formato (para no duplicar el mensaje de "requerido").
        if (string.IsNullOrWhiteSpace(telefono))
            return ValidationResult.Success;

        return ValidacionIdentidad.TelefonoValido(telefono)
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage, [validationContext.MemberName!]);
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-telefonoe164", ErrorMessage!);
        // El adaptador de cliente (validacion-socio.js) lee el patrón de este atributo.
        MergeAttribute(context.Attributes, "data-val-telefonoe164-patron", PatronE164);
    }

    private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (!attributes.ContainsKey(key))
            attributes.Add(key, value);
    }
}
