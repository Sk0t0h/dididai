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
    private readonly ISolicitudColaboracionService _solicitudes;

    public CreateModel(ISocioService socios, ISolicitudColaboracionService solicitudes)
    {
        _socios = socios;
        _solicitudes = solicitudes;
    }

    [BindProperty]
    public Socio Socio { get; set; } = new();

    /// <summary>
    /// Id de la solicitud pública desde la que se está dando de alta (si aplica). Se
    /// mantiene entre GET y POST para, al crear el socio, vincular la solicitud a él. La
    /// colaboración NO se crea aquí: se hace después desde la ficha de la solicitud (que ya
    /// tendrá socio vinculado), en un único sitio y sin mezclar la lógica económica.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public int? SolicitudId { get; set; }

    /// <summary>Id del socio de baja que coincide en DNI (para ofrecer reactivarlo), si lo hay.</summary>
    public int? IdReactivable { get; private set; }

    /// <summary>
    /// Alta en blanco, o precargada desde una solicitud pública: con <see cref="SolicitudId"/>
    /// se leen los datos de la solicitud de la BD (fuente de verdad, no la URL) y se marca el
    /// consentimiento (ya se dio en el formulario público). El resto (documento, dirección…)
    /// lo completa el admin, que controla el alta real.
    /// </summary>
    public async Task OnGetAsync()
    {
        if (SolicitudId is int solId && await _solicitudes.ObtenerAsync(solId) is SolicitudColaboracion sol)
        {
            Socio.Nombre = sol.Nombre;
            Socio.Apellidos = sol.Apellidos;
            Socio.Email = sol.Email;
            Socio.Telefono = sol.Telefono;
            // La persona ya consintió la privacidad en el formulario público.
            Socio.AceptaPrivacidad = true;
        }
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
                // Si el alta viene de una solicitud, vincularla al socio recién creado (la
                // colaboración se crea luego desde la ficha de la solicitud). Se vuelve a
                // esa ficha para continuar el flujo.
                if (SolicitudId is int solId)
                {
                    await _solicitudes.VincularSocioAsync(solId, Socio.Id);
                    TempData["Mensaje"] = $"Socio «{Socio.Nombre} {Socio.Apellidos}» dado de alta y vinculado. Ahora puedes crear su colaboración.";
                    return RedirectToPage("/Admin/Solicitudes/Details", new { id = solId });
                }
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
