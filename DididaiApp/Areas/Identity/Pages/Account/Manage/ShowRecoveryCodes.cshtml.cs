// Override propio de la página que muestra los códigos de recuperación recién
// generados (llegan por TempData desde EnableAuthenticator/GenerateRecoveryCodes).
// Vista en español, PageModel concreto.
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Areas.Identity.Pages.Account.Manage;

public class ShowRecoveryCodesModel : PageModel
{
    [TempData]
    public string[]? RecoveryCodes { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public IActionResult OnGet()
    {
        if (RecoveryCodes == null || RecoveryCodes.Length == 0)
        {
            return RedirectToPage("./TwoFactorAuthentication");
        }

        return Page();
    }
}
