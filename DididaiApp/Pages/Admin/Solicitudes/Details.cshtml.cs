using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages.Admin.Solicitudes;

/// <summary>
/// Ficha de una solicitud de colaboración pública. Muestra los datos declarados y
/// permite al admin resolverla (aprobar/rechazar) con una nota opcional. Aprobar NO
/// crea el socio automáticamente: ofrece un enlace al alta de socio con los datos
/// precargados, para que el admin complete lo que falta (incl. el IBAN si procede) y
/// controle el alta real. Solo rol Admin.
/// </summary>
[Authorize(Roles = DbSeeder.AdminRole)]
public class DetailsModel : PageModel
{
    private readonly ISolicitudColaboracionService _solicitudes;

    public DetailsModel(ISolicitudColaboracionService solicitudes) => _solicitudes = solicitudes;

    public SolicitudColaboracion Solicitud { get; private set; } = default!;

    /// <summary>Nota interna opcional al resolver (motivo del rechazo, seguimiento…).</summary>
    [BindProperty]
    public string? Nota { get; set; }

    /// <summary>Nueva acción de gestión a registrar (formulario del historial).</summary>
    [BindProperty]
    public TipoAccionSolicitud AccionTipo { get; set; } = TipoAccionSolicitud.Nota;

    [BindProperty]
    public string? AccionNota { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var s = await _solicitudes.ObtenerAsync(id);
        if (s is null) return NotFound();
        Solicitud = s;
        return Page();
    }

    public async Task<IActionResult> OnPostResolverAsync(int id, EstadoSolicitud estado)
    {
        var ok = await _solicitudes.ResolverAsync(id, estado, Nota);
        if (!ok)
        {
            // Id inexistente o estado no resolutorio: recargar la ficha si aún existe.
            if (await _solicitudes.ObtenerAsync(id) is null) return NotFound();
            TempData["Mensaje"] = "No se pudo procesar la solicitud.";
            return RedirectToPage("Details", new { id });
        }

        TempData["Mensaje"] = estado == EstadoSolicitud.Aprobada
            ? "Solicitud aprobada. Puedes dar de alta al socio desde la sección de Socios."
            : "Solicitud cancelada.";
        return RedirectToPage("Index");
    }

    /// <summary>
    /// Registra una acción de gestión en el historial de la solicitud. El usuario que
    /// queda registrado es el admin autenticado (no editable); lo toma el servidor de la
    /// identidad de la petición, no del formulario. La primera acción mueve la solicitud a
    /// "Gestionando".
    /// </summary>
    public async Task<IActionResult> OnPostAccionAsync(int id)
    {
        if (string.IsNullOrWhiteSpace(AccionNota))
        {
            TempData["Mensaje"] = "Escribe una nota para registrar la acción.";
            return RedirectToPage("Details", new { id });
        }

        var usuario = User.Identity?.Name ?? "desconocido";
        var ok = await _solicitudes.RegistrarAccionAsync(id, AccionTipo, AccionNota, usuario);
        if (!ok)
        {
            if (await _solicitudes.ObtenerAsync(id) is null) return NotFound();
            TempData["Mensaje"] = "No se pudo registrar la acción.";
        }
        return RedirectToPage("Details", new { id });
    }
}
