using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages.Admin.Socios;

/// <summary>Ficha de un socio, con sus acciones de baja/reactivación.</summary>
[Authorize(Roles = DbSeeder.AdminRole)]
public class DetailsModel : PageModel
{
    private readonly ISocioService _socios;
    private readonly IColaboracionService _colaboraciones;

    public DetailsModel(ISocioService socios, IColaboracionService colaboraciones)
    {
        _socios = socios;
        _colaboraciones = colaboraciones;
    }

    public Socio Socio { get; private set; } = new();

    /// <summary>Colaboraciones del socio (activas e históricas), más recientes primero.</summary>
    public IReadOnlyList<Colaboracion> Colaboraciones { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var socio = await _socios.ObtenerAsync(id);
        if (socio is null)
            return NotFound();

        Socio = socio;
        Colaboraciones = await _colaboraciones.ListarPorSocioAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostBajaColaboracionAsync(int id, int socioId)
    {
        await _colaboraciones.DarDeBajaAsync(id);
        TempData["Mensaje"] = "Colaboración finalizada.";
        return RedirectToPage("Details", new { id = socioId });
    }

    public async Task<IActionResult> OnPostBajaAsync(int id)
    {
        await _socios.DarDeBajaAsync(id);
        TempData["Mensaje"] = "Socio dado de baja.";
        return RedirectToPage("Details", new { id });
    }

    public async Task<IActionResult> OnPostReactivarAsync(int id)
    {
        await _socios.ReactivarAsync(id);
        TempData["Mensaje"] = "Socio reactivado.";
        return RedirectToPage("Details", new { id });
    }
}
