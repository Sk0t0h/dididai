using System.ComponentModel.DataAnnotations;
using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages.Admin.Auditoria;

/// <summary>
/// Consulta (solo lectura) del log de auditoría transversal: las acciones de gestión
/// registradas automáticamente por el sistema (quién hizo qué y cuándo). No permite
/// crear, editar ni borrar: el log es inmutable. Solo rol Admin.
/// </summary>
[Authorize(Roles = DbSeeder.AdminRole)]
public class IndexModel : PageModel
{
    private readonly IAuditoriaService _auditoria;

    public IndexModel(IAuditoriaService auditoria) => _auditoria = auditoria;

    public PaginaAuditoria Resultado { get; private set; } = default!;

    // Filtros (se mantienen en la URL para poder compartir/paginar la vista filtrada).
    [BindProperty(SupportsGet = true)]
    public string? Usuario { get; set; }

    [BindProperty(SupportsGet = true)]
    public TipoAccionAuditoria? Accion { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? Desde { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? Hasta { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Pagina { get; set; } = 1;

    public const int TamanoPagina = 50;

    public async Task OnGetAsync()
    {
        Resultado = await _auditoria.ListarAsync(Usuario, Accion, Desde, Hasta, Pagina, TamanoPagina);
    }
}
