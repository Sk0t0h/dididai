using DididaiApp.Core.Data;
using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages.Admin;

/// <summary>
/// Página de inicio del back de gestión. Protegida: exige rol Admin. Punto de
/// entrada del que cuelgan socios, económico, dashboards y las solicitudes públicas.
/// </summary>
[Authorize(Roles = DbSeeder.AdminRole)]
public class IndexModel : PageModel
{
    private readonly ISolicitudColaboracionService _solicitudes;

    public IndexModel(ISolicitudColaboracionService solicitudes) => _solicitudes = solicitudes;

    /// <summary>Solicitudes públicas pendientes de revisar (badge de aviso en el panel).</summary>
    public int SolicitudesPendientes { get; private set; }

    public async Task OnGetAsync()
    {
        SolicitudesPendientes = await _solicitudes.ContarPendientesAsync();
    }
}
