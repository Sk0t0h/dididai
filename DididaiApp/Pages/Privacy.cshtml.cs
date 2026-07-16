using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages;

/// <summary>
/// Ruta heredada de la plantilla. La política real vive en <c>/privacidad</c>
/// (página bilingüe conforme a RGPD). Redirige permanentemente para no dejar
/// enlaces rotos a la antigua <c>/Privacy</c>.
/// </summary>
public class PrivacyModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Legal/Privacidad");
}
