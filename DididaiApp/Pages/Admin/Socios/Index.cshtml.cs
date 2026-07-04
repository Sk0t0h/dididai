using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages.Admin.Socios;

/// <summary>Listado de socios con búsqueda y opción de incluir los dados de baja.</summary>
[Authorize(Roles = DbSeeder.AdminRole)]
public class IndexModel : PageModel
{
    private readonly ISocioService _socios;

    public IndexModel(ISocioService socios) => _socios = socios;

    public IReadOnlyList<Socio> Socios { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Busqueda { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool IncluirBajas { get; set; }

    public async Task OnGetAsync()
    {
        Socios = await _socios.ListarAsync(IncluirBajas, Busqueda);
    }
}
