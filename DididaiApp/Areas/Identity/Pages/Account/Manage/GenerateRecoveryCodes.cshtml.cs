// Override propio de la página de generación de códigos de recuperación de Identity.
// Vista en español, PageModel concreto tipado a IdentityUser.
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Areas.Identity.Pages.Account.Manage;

public class GenerateRecoveryCodesModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<GenerateRecoveryCodesModel> _logger;

    public GenerateRecoveryCodesModel(
        UserManager<IdentityUser> userManager, ILogger<GenerateRecoveryCodesModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [TempData]
    public string[]? RecoveryCodes { get; set; }

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

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
        {
            throw new InvalidOperationException("No se pueden generar códigos de recuperación porque la 2FA no está activada para este usuario.");
        }

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        RecoveryCodes = recoveryCodes!.ToArray();

        var userId = await _userManager.GetUserIdAsync(user);
        _logger.LogInformation("El usuario con ID '{UserId}' ha generado nuevos códigos de recuperación de 2FA.", userId);
        StatusMessage = "Has generado nuevos códigos de recuperación.";
        return RedirectToPage("./ShowRecoveryCodes");
    }
}
