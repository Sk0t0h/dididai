using System.ComponentModel.DataAnnotations;
using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages.Admin.Economia;

/// <summary>
/// Módulo económico: métricas de ingresos (desde las colaboraciones), balance con los
/// gastos, gestión de gastos (alta/borrado) y vista global de colaboraciones. Los
/// cálculos se delegan en los servicios de Core; la página no toca el DbContext.
/// </summary>
[Authorize(Roles = DbSeeder.AdminRole)]
public class IndexModel : PageModel
{
    private readonly IResumenEconomicoService _resumen;
    private readonly IGastoService _gastos;
    private readonly IColaboracionService _colaboraciones;

    public IndexModel(IResumenEconomicoService resumen, IGastoService gastos, IColaboracionService colaboraciones)
    {
        _resumen = resumen;
        _gastos = gastos;
        _colaboraciones = colaboraciones;
    }

    public ResumenEconomico Resumen { get; private set; } = new();
    public IReadOnlyList<Gasto> Gastos { get; private set; } = [];
    public IReadOnlyList<Colaboracion> Colaboraciones { get; private set; } = [];

    [BindProperty]
    public NuevoGasto GastoNuevo { get; set; } = new();

    /// <summary>Datos del alta rápida de gasto (formulario inline en la página).</summary>
    public class NuevoGasto
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Concepto")]
        public string Concepto { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 10_000_000, ErrorMessage = "El importe debe ser mayor que cero.")]
        [Display(Name = "Importe (€)")]
        public decimal Importe { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow.Date;

        [Required]
        [Display(Name = "Categoría")]
        public CategoriaGasto Categoria { get; set; } = CategoriaGasto.AccionDirecta;
    }

    private async Task CargarAsync()
    {
        Resumen = await _resumen.ObtenerAsync();
        Gastos = await _gastos.ListarAsync();
        Colaboraciones = await _colaboraciones.ListarTodasAsync();
    }

    public async Task OnGetAsync() => await CargarAsync();

    public async Task<IActionResult> OnPostCrearGastoAsync()
    {
        if (!ModelState.IsValid)
        {
            await CargarAsync();
            return Page();
        }

        await _gastos.CrearAsync(new Gasto
        {
            Concepto = GastoNuevo.Concepto,
            Importe = GastoNuevo.Importe,
            Fecha = GastoNuevo.Fecha,
            Categoria = GastoNuevo.Categoria,
        });
        TempData["Mensaje"] = "Gasto registrado.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarGastoAsync(int id)
    {
        await _gastos.EliminarAsync(id);
        TempData["Mensaje"] = "Gasto eliminado.";
        return RedirectToPage();
    }
}
