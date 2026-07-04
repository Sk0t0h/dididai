using DididaiApp.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages.Admin;

/// <summary>
/// Página de inicio del back de gestión. Protegida: exige rol Admin. Punto de
/// entrada del que colgarán socios, económico y dashboards.
/// </summary>
[Authorize(Roles = DbSeeder.AdminRole)]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
