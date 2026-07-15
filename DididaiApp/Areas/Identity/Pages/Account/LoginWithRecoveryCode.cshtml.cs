// Override propio de la página de acceso con código de recuperación de Identity
// (alternativa al código del authenticator si se pierde el dispositivo). Vista en
// español, PageModel concreto tipado a IdentityUser.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginWithRecoveryCodeModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<LoginWithRecoveryCodeModel> _logger;

    public LoginWithRecoveryCodeModel(
        SignInManager<IdentityUser> signInManager, ILogger<LoginWithRecoveryCodeModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "El código de recuperación es obligatorio.")]
        [DataType(DataType.Text)]
        [Display(Name = "Código de recuperación")]
        public string RecoveryCode { get; set; } = default!;
    }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        // Fuera del flujo de 2FA (URL directa), volver al acceso en vez de error.
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return RedirectToPage("./Login");
        }

        ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException("No se puede cargar al usuario para la autenticación en dos pasos.");
        }

        var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);

        var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        var userId = await _signInManager.UserManager.GetUserIdAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation("El usuario con ID '{UserId}' ha accedido con un código de recuperación.", userId);
            return LocalRedirect(returnUrl ?? Url.Content("~/"));
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning("La cuenta del usuario con ID '{UserId}' está bloqueada.", userId);
            return RedirectToPage("./Lockout");
        }

        _logger.LogWarning("Código de recuperación no válido para el usuario con ID '{UserId}'.", userId);
        ModelState.AddModelError(string.Empty, "El código de recuperación no es válido.");
        return Page();
    }
}
