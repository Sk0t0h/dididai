using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages.Admin.Solicitudes;

/// <summary>
/// Listado de solicitudes de colaboración llegadas del formulario público, con filtro
/// por estado. Punto de entrada de la revisión: desde aquí el admin abre la ficha para
/// aprobar/rechazar. Solo rol Admin.
/// </summary>
[Authorize(Roles = DbSeeder.AdminRole)]
public class IndexModel : PageModel
{
    private readonly ISolicitudColaboracionService _solicitudes;

    public IndexModel(ISolicitudColaboracionService solicitudes) => _solicitudes = solicitudes;

    public IReadOnlyList<SolicitudColaboracion> Solicitudes { get; private set; } = [];

    /// <summary>Filtro de estado; null = todas. Llega por querystring.</summary>
    [BindProperty(SupportsGet = true)]
    public EstadoSolicitud? Estado { get; set; }

    public async Task OnGetAsync()
    {
        Solicitudes = await _solicitudes.ListarAsync(Estado);
    }
}
