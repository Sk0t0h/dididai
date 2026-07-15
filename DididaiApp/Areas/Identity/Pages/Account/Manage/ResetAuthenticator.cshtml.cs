// Override propio de la página de restablecimiento de la clave del authenticator de
// Identity. Vista en español, PageModel concreto tipado a IdentityUser.
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Areas.Identity.Pages.Account.Manage;

public class ResetAuthenticatorModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<ResetAuthenticatorModel> _logger;

    public ResetAuthenticatorModel(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ILogger<ResetAuthenticatorModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
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

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
        }

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.ResetAuthenticatorKeyAsync(user);
        var userId = await _userManager.GetUserIdAsync(user);
        _logger.LogInformation("El usuario con ID '{UserId}' ha restablecido la clave de su aplicación de autenticación.", userId);

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Se ha restablecido la clave de tu aplicación de autenticación. Deberás configurar de nuevo tu aplicación con la clave nueva.";

        return RedirectToPage("./EnableAuthenticator");
    }
}
