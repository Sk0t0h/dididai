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

    /// <summary>
    /// Convierte el JSON de cambios (antes/después) de un registro en líneas legibles
    /// «Campo: antes → después». Devuelve lista vacía si no hay cambios o el JSON es inválido.
    /// </summary>
    public static IReadOnlyList<string> FormatearCambios(string? cambiosJson)
    {
        if (string.IsNullOrWhiteSpace(cambiosJson))
            return [];

        try
        {
            var dict = System.Text.Json.JsonSerializer
                .Deserialize<Dictionary<string, ConstructorCambios.ValorCambiado>>(cambiosJson);
            if (dict is null) return [];
            return dict
                .Select(kv => $"{kv.Key}: {Mostrar(kv.Value.antes)} → {Mostrar(kv.Value.despues)}")
                .ToList();
        }
        catch (System.Text.Json.JsonException)
        {
            return [];
        }
    }

    private static string Mostrar(string? v) => string.IsNullOrEmpty(v) ? "∅" : v;
}
