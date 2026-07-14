using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages.Admin.Usuarios;

/// <summary>
/// Listado de usuarios administradores del back. Punto de entrada de la gestión de admins,
/// que sustituye al registro público (deshabilitado): desde aquí se dan de alta y se
/// activan/desactivan. Solo rol Admin.
/// </summary>
[Authorize(Roles = DbSeeder.AdminRole)]
public class IndexModel : PageModel
{
    private readonly IAdminUsuarioService _admins;
    private readonly IAuditoriaService _auditoria;

    public IndexModel(IAdminUsuarioService admins, IAuditoriaService auditoria)
    {
        _admins = admins;
        _auditoria = auditoria;
    }

    public IReadOnlyList<AdminUsuarioDto> Admins { get; private set; } = [];

    /// <summary>Email del admin autenticado (para no ofrecerle desactivarse a sí mismo).</summary>
    public string EmailActual => User.Identity?.Name ?? string.Empty;

    public async Task OnGetAsync()
    {
        Admins = await _admins.ListarAdminsAsync();
    }

    public async Task<IActionResult> OnPostDesactivarAsync(string id)
    {
        // El email se resuelve ANTES de desactivar, para que el detalle de auditoría sea
        // legible (el handler solo recibe el id/GUID).
        var email = await EmailDeAsync(id);
        var r = await _admins.DesactivarAsync(id, EmailActual);
        if (r == ResultadoBajaAdmin.Ok)
            await _auditoria.RegistrarAsync(TipoAccionAuditoria.AdminDesactivacion,
                "Administrador", id, $"Desactivación del administrador «{email}»", EmailActual);
        TempData["Mensaje"] = r switch
        {
            ResultadoBajaAdmin.Ok => "Administrador desactivado.",
            ResultadoBajaAdmin.EsSuperAdmin => "No se puede desactivar al administrador principal.",
            ResultadoBajaAdmin.NoUnoMismo => "No puedes desactivar tu propia cuenta.",
            _ => "No se encontró el administrador.",
        };
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReactivarAsync(string id)
    {
        var email = await EmailDeAsync(id);
        await _admins.ReactivarAsync(id);
        await _auditoria.RegistrarAsync(TipoAccionAuditoria.AdminReactivacion,
            "Administrador", id, $"Reactivación del administrador «{email}»", EmailActual);
        TempData["Mensaje"] = "Administrador reactivado.";
        return RedirectToPage();
    }

    /// <summary>Email del admin con ese id (para el detalle de auditoría); respalda al id si no aparece.</summary>
    private async Task<string> EmailDeAsync(string id)
    {
        var admin = (await _admins.ListarAdminsAsync()).FirstOrDefault(a => a.Id == id);
        return admin?.Email ?? $"#{id}";
    }
}
