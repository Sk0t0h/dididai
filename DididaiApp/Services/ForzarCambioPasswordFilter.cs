using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Services;

/// <summary>
/// Filtro de páginas que fuerza el cambio de contraseña en el primer inicio de sesión de un
/// administrador dado de alta por otro. Mientras el usuario autenticado tenga el claim
/// <see cref="IAdminUsuarioService.MustChangePasswordClaim"/>, cualquier página lo redirige a
/// la de cambio de contraseña. El claim se elimina al cambiarla (ver ChangePassword), y el
/// refresco de la cookie hace que el filtro deje de actuar sin re-login.
///
/// <para>Es una medida de higiene, no un control de seguridad fuerte: el claim viaja firmado en
/// la cookie de autenticación. Se excluyen las rutas imprescindibles (la propia página de
/// cambio y el logout) para no dejar al usuario atrapado sin poder cambiarla ni salir.</para>
/// </summary>
public class ForzarCambioPasswordFilter : IAsyncPageFilter
{
    // Rutas que NO se deben interceptar: la de cambio (destino) y el logout (salida segura).
    private const string RutaCambio = "/Account/Manage/ChangePassword";
    private const string RutaLogout = "/Account/Logout";

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        var debeCambiar = user.Identity?.IsAuthenticated == true &&
                          user.HasClaim(c => c.Type == IAdminUsuarioService.MustChangePasswordClaim);

        if (debeCambiar)
        {
            // La ruta de página actual (p. ej. "/Admin/Socios/Index" o, en un área,
            // "/Account/Manage/ChangePassword"). Se compara con las excluidas.
            var pagina = (context.ActionDescriptor as CompiledPageActionDescriptor)?.ViewEnginePath
                         ?? string.Empty;

            if (!pagina.Equals(RutaCambio, StringComparison.OrdinalIgnoreCase) &&
                !pagina.Equals(RutaLogout, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToPageResult("/Account/Manage/ChangePassword", new { area = "Identity" });
                return;
            }
        }

        await next();
    }
}
