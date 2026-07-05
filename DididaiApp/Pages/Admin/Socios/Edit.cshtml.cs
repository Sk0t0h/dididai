using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages.Admin.Socios;

/// <summary>Edición de los datos de un socio existente, con su tabla de colaboraciones.</summary>
[Authorize(Roles = DbSeeder.AdminRole)]
public class EditModel : PageModel
{
    private readonly ISocioService _socios;
    private readonly IColaboracionService _colaboraciones;

    public EditModel(ISocioService socios, IColaboracionService colaboraciones)
    {
        _socios = socios;
        _colaboraciones = colaboraciones;
    }

    [BindProperty]
    public Socio Socio { get; set; } = new();

    /// <summary>Colaboraciones del socio (activas e históricas), para gestionarlas desde aquí.</summary>
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            // Recargar la tabla para que el partial no salga vacío al re-renderizar.
            Colaboraciones = await _colaboraciones.ListarPorSocioAsync(Socio.Id);
            return Page();
        }

        var resultado = await _socios.ActualizarAsync(Socio);
        switch (resultado)
        {
            case ResultadoActualizacion.Actualizado:
                TempData["Mensaje"] = $"Socio «{Socio.Nombre} {Socio.Apellidos}» actualizado.";
                return RedirectToPage("Index");

            case ResultadoActualizacion.DniDuplicado:
                ModelState.AddModelError("Socio.Dni", "Ya existe otro socio con ese DNI.");
                break;

            case ResultadoActualizacion.PaisInvalido:
                ModelState.AddModelError("Socio.PaisResidencia", "Selecciona un país de residencia válido de la lista.");
                break;

            case ResultadoActualizacion.DocumentoInvalido:
                ModelState.AddModelError("Socio.Dni",
                    "El documento no es válido para el tipo indicado (DNI/NIE deben llevar la letra de control correcta).");
                break;

            case ResultadoActualizacion.TelefonoInvalido:
                ModelState.AddModelError("Socio.Telefono",
                    "El teléfono debe estar en formato internacional, con prefijo de país (p. ej. +34612345678).");
                break;

            case ResultadoActualizacion.NoEncontrado:
                return NotFound();
        }

        // Cualquier error de validación de servidor: recargar la tabla y re-renderizar.
        Colaboraciones = await _colaboraciones.ListarPorSocioAsync(Socio.Id);
        return Page();
    }

    /// <summary>Da de baja (finaliza) una colaboración desde la edición del socio.</summary>
    public async Task<IActionResult> OnPostBajaColaboracionAsync(int id, int socioId)
    {
        await _colaboraciones.DarDeBajaAsync(id);
        TempData["Mensaje"] = "Colaboración finalizada.";
        return RedirectToPage("Edit", new { id = socioId });
    }
}
