using System.ComponentModel.DataAnnotations;

namespace DididaiApp.Core.Models;

/// <summary>
/// Tipo de documento de identidad que aporta el socio. Es lo que decide cómo se
/// valida el documento —NO el país de residencia—, para no ser ambiguo con socios
/// internacionales (p. ej. un español residente en el extranjero declara "DNI
/// español" y se le valida la letra igualmente).
/// </summary>
public enum TipoDocumento
{
    [Display(Name = "DNI español")]
    DniEspanol = 0,

    [Display(Name = "NIE (España)")]
    Nie = 1,

    [Display(Name = "Pasaporte")]
    Pasaporte = 2,

    [Display(Name = "Otro documento")]
    Otro = 3,
}
