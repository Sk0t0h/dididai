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
    private readonly ISolicitudColaboracionService _solicitudes;
    private readonly IAuditoriaService _auditoria;

    public DetailsModel(ISocioService socios, IColaboracionService colaboraciones,
        ISolicitudColaboracionService solicitudes, IAuditoriaService auditoria)
    {
        _socios = socios;
        _colaboraciones = colaboraciones;
        _solicitudes = solicitudes;
        _auditoria = auditoria;
    }

    private string Actor => User.Identity?.Name ?? "desconocido";

    public Socio Socio { get; private set; } = new();

    /// <summary>Colaboraciones del socio (activas e históricas), más recientes primero.</summary>
    public IReadOnlyList<Colaboracion> Colaboraciones { get; private set; } = [];

    /// <summary>Solicitudes públicas vinculadas a este socio (trazabilidad).</summary>
    public IReadOnlyList<SolicitudColaboracion> Solicitudes { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var socio = await _socios.ObtenerAsync(id);
        if (socio is null)
            return NotFound();

        Socio = socio;
        Colaboraciones = await _colaboraciones.ListarPorSocioAsync(id);
        Solicitudes = await _solicitudes.ListarPorSocioAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostBajaColaboracionAsync(int id, int socioId)
    {
        await _colaboraciones.DarDeBajaAsync(id);
        await _auditoria.RegistrarAsync(TipoAccionAuditoria.ColaboracionBaja,
            "Colaboración", id.ToString(), $"Baja de colaboración del socio #{socioId}", Actor);
        TempData["Mensaje"] = "Colaboración finalizada.";
        return RedirectToPage("Details", new { id = socioId });
    }

    public async Task<IActionResult> OnPostBajaAsync(int id)
    {
        await _socios.DarDeBajaAsync(id);
        await _auditoria.RegistrarAsync(TipoAccionAuditoria.SocioBaja,
            "Socio", id.ToString(), DescribirSocio(await _socios.ObtenerAsync(id), id), Actor);
        TempData["Mensaje"] = "Socio dado de baja.";
        return RedirectToPage("Details", new { id });
    }

    public async Task<IActionResult> OnPostReactivarAsync(int id)
    {
        await _socios.ReactivarAsync(id);
        await _auditoria.RegistrarAsync(TipoAccionAuditoria.SocioReactivacion,
            "Socio", id.ToString(), DescribirSocio(await _socios.ObtenerAsync(id), id), Actor);
        TempData["Mensaje"] = "Socio reactivado.";
        return RedirectToPage("Details", new { id });
    }

    /// <summary>Detalle legible de un socio para el log (nombre + DNI), con respaldo por id.</summary>
    private static string DescribirSocio(Socio? socio, int id) =>
        socio is null ? $"Socio #{id}" : $"Socio {socio.Nombre} {socio.Apellidos} (DNI {socio.Dni})";
}
