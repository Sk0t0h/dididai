// Override propio de la página índice de 2FA de Identity: estado de la autenticación
// en dos pasos y accesos a activar/desactivar/regenerar. Vista en español, PageModel
// concreto tipado a IdentityUser (mismo patrón que el resto de overrides).
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Areas.Identity.Pages.Account.Manage;

public class TwoFactorAuthenticationModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public TwoFactorAuthenticationModel(
        UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public bool HasAuthenticator { get; set; }
    public int RecoveryCodesLeft { get; set; }
    public bool Is2faEnabled { get; set; }
    public bool IsMachineRemembered { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
        }

        HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;
        Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
        RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
        }

        await _signInManager.ForgetTwoFactorClientAsync();
        StatusMessage = "Se ha olvidado el navegador actual. La próxima vez que accedas desde este navegador " +
                        "se te pedirá el código de autenticación en dos pasos.";
        return RedirectToPage();
    }
}
