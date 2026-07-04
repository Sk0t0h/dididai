using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages.Admin.Socios;

/// <summary>Alta de un socio nuevo.</summary>
[Authorize(Roles = DbSeeder.AdminRole)]
public class CreateModel : PageModel
{
    private readonly ISocioService _socios;

    public CreateModel(ISocioService socios) => _socios = socios;

    [BindProperty]
    public Socio Socio { get; set; } = new();

    /// <summary>Id del socio de baja que coincide en DNI (para ofrecer reactivarlo), si lo hay.</summary>
    public int? IdReactivable { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // En el alta el consentimiento RGPD es obligatorio (un bool [Required] no basta:
        // valida presencia, no que sea true). Se comprueba explícitamente aquí.
        if (!Socio.AceptaPrivacidad)
            ModelState.AddModelError("Socio.AceptaPrivacidad", "Es necesario el consentimiento de privacidad para dar de alta al socio.");

        if (!ModelState.IsValid)
            return Page();

        var resultado = await _socios.CrearAsync(Socio);
        switch (resultado)
        {
            case ResultadoAlta.Creado:
                TempData["Mensaje"] = $"Socio «{Socio.Nombre} {Socio.Apellidos}» dado de alta.";
                return RedirectToPage("Index");

            case ResultadoAlta.DniDuplicadoActivo:
                ModelState.AddModelError("Socio.Dni", "Ya existe un socio activo con ese DNI.");
                return Page();

            case ResultadoAlta.DniExisteDeBaja:
                var existente = await _socios.ObtenerPorDniAsync(Socio.Dni);
                IdReactivable = existente?.Id;
                ModelState.AddModelError("Socio.Dni",
                    "Ya existe un socio con ese DNI dado de baja. Puedes reactivarlo en lugar de crear uno nuevo.");
                return Page();

            default:
                return Page();
        }
    }
}
