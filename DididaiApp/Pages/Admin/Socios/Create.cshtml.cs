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

    /// <summary>
    /// Alta en blanco, o precargada desde una solicitud pública aprobada: si llegan los
    /// parámetros opcionales por querystring (enlace "Dar de alta" de la ficha de
    /// solicitud), se rellenan los campos coincidentes. Todo lo demás (documento, IBAN…)
    /// lo completa el admin, que sigue siendo quien controla el alta real.
    /// </summary>
    public void OnGet(string? nombre, string? apellidos, string? email, string? telefono)
    {
        if (!string.IsNullOrWhiteSpace(nombre)) Socio.Nombre = nombre;
        if (!string.IsNullOrWhiteSpace(apellidos)) Socio.Apellidos = apellidos;
        if (!string.IsNullOrWhiteSpace(email)) Socio.Email = email;
        if (!string.IsNullOrWhiteSpace(telefono)) Socio.Telefono = telefono;
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

            case ResultadoAlta.PaisInvalido:
                ModelState.AddModelError("Socio.PaisResidencia", "Selecciona un país de residencia válido de la lista.");
                return Page();

            case ResultadoAlta.DocumentoInvalido:
                ModelState.AddModelError("Socio.Dni",
                    "El documento no es válido para el tipo indicado (DNI/NIE deben llevar la letra de control correcta).");
                return Page();

            case ResultadoAlta.TelefonoInvalido:
                ModelState.AddModelError("Socio.Telefono",
                    "El teléfono debe estar en formato internacional, con prefijo de país (p. ej. +34612345678).");
                return Page();

            default:
                return Page();
        }
    }
}
