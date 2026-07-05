using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages.Admin.Socios;

/// <summary>Edición de los datos de un socio existente.</summary>
[Authorize(Roles = DbSeeder.AdminRole)]
public class EditModel : PageModel
{
    private readonly ISocioService _socios;

    public EditModel(ISocioService socios) => _socios = socios;

    [BindProperty]
    public Socio Socio { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var socio = await _socios.ObtenerAsync(id);
        if (socio is null)
            return NotFound();

        Socio = socio;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var resultado = await _socios.ActualizarAsync(Socio);
        switch (resultado)
        {
            case ResultadoActualizacion.Actualizado:
                TempData["Mensaje"] = $"Socio «{Socio.Nombre} {Socio.Apellidos}» actualizado.";
                return RedirectToPage("Index");

            case ResultadoActualizacion.DniDuplicado:
                ModelState.AddModelError("Socio.Dni", "Ya existe otro socio con ese DNI.");
                return Page();

            case ResultadoActualizacion.PaisInvalido:
                ModelState.AddModelError("Socio.PaisResidencia", "Selecciona un país de residencia válido de la lista.");
                return Page();

            case ResultadoActualizacion.DocumentoInvalido:
                ModelState.AddModelError("Socio.Dni",
                    "El documento no es válido para el tipo indicado (DNI/NIE deben llevar la letra de control correcta).");
                return Page();

            case ResultadoActualizacion.TelefonoInvalido:
                ModelState.AddModelError("Socio.Telefono",
                    "El teléfono debe estar en formato internacional, con prefijo de país (p. ej. +34612345678).");
                return Page();

            case ResultadoActualizacion.NoEncontrado:
                return NotFound();

            default:
                return Page();
        }
    }
}
