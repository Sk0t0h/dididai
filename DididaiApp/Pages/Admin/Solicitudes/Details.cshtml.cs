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
    private readonly ISocioService _socios;

    public DetailsModel(ISolicitudColaboracionService solicitudes, ISocioService socios)
    {
        _solicitudes = solicitudes;
        _socios = socios;
    }

    public SolicitudColaboracion Solicitud { get; private set; } = default!;

    /// <summary>Socio ya vinculado a la solicitud (si lo hay), para mostrarlo en la ficha.</summary>
    public Socio? SocioVinculado { get; private set; }

    /// <summary>Socios existentes que coinciden por email/teléfono (sugerencia de matching).</summary>
    public IReadOnlyList<Socio> PosiblesCoincidencias { get; private set; } = [];

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
        await CargarAsync(s);
        return Page();
    }

    /// <summary>
    /// Rellena la solicitud, el socio ya vinculado (si lo hay) y —solo si aún no está
    /// vinculada— las posibles coincidencias por email/teléfono (sugerencia de matching).
    /// </summary>
    private async Task CargarAsync(SolicitudColaboracion s)
    {
        Solicitud = s;
        if (s.SocioId is int socioId)
        {
            SocioVinculado = await _socios.ObtenerAsync(socioId);
        }
        else
        {
            PosiblesCoincidencias = await _socios.BuscarPosiblesCoincidenciasAsync(s.Email, s.Telefono);
        }
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

    /// <summary>
    /// Vincula la solicitud a un socio existente (el admin confirma que la persona ya
    /// colabora, a partir de las posibles coincidencias mostradas).
    /// </summary>
    public async Task<IActionResult> OnPostVincularAsync(int id, int socioId)
    {
        var ok = await _solicitudes.VincularSocioAsync(id, socioId);
        if (await _solicitudes.ObtenerAsync(id) is null) return NotFound();
        TempData["Mensaje"] = ok
            ? "Solicitud vinculada al socio."
            : "No se pudo vincular al socio.";
        return RedirectToPage("Details", new { id });
    }
}
