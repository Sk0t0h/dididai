// Override propio de la página de desactivación de 2FA de Identity. Vista en español,
// PageModel concreto tipado a IdentityUser (mismo patrón que el resto de overrides).
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Areas.Identity.Pages.Account.Manage;

public class Disable2faModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<Disable2faModel> _logger;

    public Disable2faModel(UserManager<IdentityUser> userManager, ILogger<Disable2faModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
        }

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
        {
            // No aplica sin 2FA activa: en vez de un error, volver al índice de 2FA.
            return RedirectToPage("./TwoFactorAuthentication");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
        }

        var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!disable2faResult.Succeeded)
        {
            throw new InvalidOperationException("Error inesperado al desactivar la autenticación en dos pasos.");
        }

        var userId = await _userManager.GetUserIdAsync(user);
        _logger.LogInformation("El usuario con ID '{UserId}' ha desactivado la 2FA.", userId);
        StatusMessage = "La autenticación en dos pasos se ha desactivado. Puedes volver a activarla cuando configures una aplicación de autenticación.";
        return RedirectToPage("./TwoFactorAuthentication");
    }
}
