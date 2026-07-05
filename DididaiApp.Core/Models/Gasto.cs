using System.ComponentModel.DataAnnotations;

namespace DididaiApp.Core.Models;

/// <summary>Categoría de un gasto de la ONG (destino del dinero).</summary>
public enum CategoriaGasto
{
    [Display(Name = "Proyectos / acción directa")]
    AccionDirecta = 0,

    [Display(Name = "Administración")]
    Administracion = 1,

    [Display(Name = "Personal")]
    Personal = 2,

    [Display(Name = "Suministros")]
    Suministros = 3,

    [Display(Name = "Otros")]
    Otros = 4,
}

/// <summary>
/// Gasto de la ONG. Contrapartida de las colaboraciones (ingresos) en el módulo
/// económico: el balance es la suma de ingresos menos la suma de gastos. La
/// categoría permite mostrar cuánto va a acción directa frente a administración
/// (refuerza el mensaje de marca "el 99% de los ingresos va a acción directa").
/// </summary>
public class Gasto
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Concepto")]
    public string Concepto { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 10_000_000, ErrorMessage = "El importe debe ser mayor que cero.")]
    [Display(Name = "Importe (€)")]
    public decimal Importe { get; set; }

    [Required]
    [Display(Name = "Fecha")]
    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; }

    [Required]
    [Display(Name = "Categoría")]
    public CategoriaGasto Categoria { get; set; } = CategoriaGasto.AccionDirecta;
}
