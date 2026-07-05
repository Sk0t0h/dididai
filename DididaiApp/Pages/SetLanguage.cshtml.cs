using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages;

/// <summary>
/// Fija el idioma elegido por el visitante en el selector de la cabecera.
/// Guarda la cultura en la cookie estándar de localización de ASP.NET Core
/// (<see cref="CookieRequestCultureProvider"/> la lee en la siguiente petición)
/// y redirige a la página desde la que se cambió. Solo acepta culturas del
/// catálogo soportado, para no aceptar valores arbitrarios desde el formulario.
/// </summary>
public class SetLanguageModel : PageModel
{
    // Debe coincidir con las culturas registradas en Program.cs.
    private static readonly HashSet<string> CulturasPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "es", "en"
    };

    public IActionResult OnPost(string culture, string? returnUrl = null)
    {
        if (!string.IsNullOrWhiteSpace(culture) && CulturasPermitidas.Contains(culture))
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    HttpOnly = false,
                    SameSite = SameSiteMode.Lax
                });
        }

        // Solo redirigir a rutas locales (evita open redirect).
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToPage("/Index");
    }
}
