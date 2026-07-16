using System.ComponentModel.DataAnnotations;

namespace DididaiApp.Core.Models;

/// <summary>
/// Periodicidad de un <see cref="Gasto"/>. Determina cómo devenga en el tiempo:
/// un gasto puntual cuenta una sola vez en su fecha; uno recurrente (mensual/anual)
/// cuenta en cada período vivo dentro del rango consultado. Necesario para que el
/// balance y la previsión sean coherentes (un alquiler mensual no es un pago único).
/// </summary>
public enum PeriodicidadGasto
{
    [Display(Name = "Puntual")]
    Puntual = 0,

    [Display(Name = "Mensual")]
    Mensual = 1,

    [Display(Name = "Anual")]
    Anual = 2,
}
