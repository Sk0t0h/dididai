using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DididaiApp.Core.Models.Validation;

/// <summary>
/// Valida el documento de identidad según el <see cref="TipoDocumento"/> declarado en
/// otra propiedad del mismo modelo (validación dependiente de otro campo). DNI español
/// y NIE exigen letra de control correcta; pasaporte u "otro" solo presencia.
///
/// Implementa <see cref="IClientModelValidator"/>: emite <c>data-val-*</c> con el nombre
/// del campo del tipo y los patrones DNI/NIE, para que el adaptador de cliente
/// (validacion-socio.js) aplique la misma regla en vivo y REVALIDE al cambiar el tipo.
/// Cliente y servidor comparten criterio; la lógica de servidor delega en
/// <see cref="ValidacionIdentidad"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DocumentoPorTipoAttribute : ValidationAttribute, IClientModelValidator
{
    /// <summary>Nombre de la propiedad del modelo que contiene el <see cref="TipoDocumento"/>.</summary>
    public string TipoPropiedad { get; }

    // Patrones para el cliente (los algoritmos de letra los aplica el JS del adaptador).
    public const string PatronDni = @"^\d{8}[A-Za-z]$";
    public const string PatronNie = @"^[XYZxyz]\d{7}[A-Za-z]$";

    public DocumentoPorTipoAttribute(string tipoPropiedad)
    {
        TipoPropiedad = tipoPropiedad;
        ErrorMessage = "El documento no es válido para el tipo indicado (DNI/NIE deben llevar la letra de control correcta).";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var documento = value as string;
        if (string.IsNullOrWhiteSpace(documento))
            return ValidationResult.Success; // [Required] cubre la obligatoriedad.

        var propTipo = validationContext.ObjectType.GetProperty(TipoPropiedad);
        if (propTipo is null)
            return ValidationResult.Success; // Configuración incorrecta: no bloquear.

        var tipoValor = propTipo.GetValue(validationContext.ObjectInstance);
        var tipo = tipoValor is TipoDocumento t ? t : TipoDocumento.Otro;

        return ValidacionIdentidad.DocumentoValido(documento, tipo)
            ? ValidationResult.Success
            : new ValidationResult(ErrorMessage, [validationContext.MemberName!]);
    }

    public void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-documentoportipo", ErrorMessage!);
        // Nombre de la propiedad del tipo (el adaptador de cliente resuelve el <select>
        // hermano dentro del mismo formulario por sufijo). Y los patrones DNI/NIE.
        MergeAttribute(context.Attributes, "data-val-documentoportipo-tipocampo", TipoPropiedad);
        MergeAttribute(context.Attributes, "data-val-documentoportipo-patrondni", PatronDni);
        MergeAttribute(context.Attributes, "data-val-documentoportipo-patronnie", PatronNie);
    }

    private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (!attributes.ContainsKey(key))
            attributes.Add(key, value);
    }
}
