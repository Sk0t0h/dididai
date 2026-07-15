// Override propio de la página de acceso con 2FA (código del authenticator) de
// Identity. Vista en español, PageModel concreto tipado a IdentityUser. Aparece en
// el login cuando el usuario tiene la 2FA activada.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginWith2faModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<LoginWith2faModel> _logger;

    public LoginWith2faModel(SignInManager<IdentityUser> signInManager, ILogger<LoginWith2faModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "El código de autenticación es obligatorio.")]
        [StringLength(7, ErrorMessage = "El {0} debe tener entre {2} y {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Código de autenticación")]
        public string TwoFactorCode { get; set; } = default!;

        [Display(Name = "Recordar este dispositivo")]
        public bool RememberMachine { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(bool rememberMe, string? returnUrl = null)
    {
        // Verificar que el usuario ha pasado el paso de usuario y contraseña. Si se
        // llega aquí fuera de ese flujo (URL directa), volver al acceso en vez de error.
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return RedirectToPage("./Login");
        }

        ReturnUrl = returnUrl;
        RememberMe = rememberMe;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(bool rememberMe, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        returnUrl ??= Url.Content("~/");

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException("No se puede cargar al usuario para la autenticación en dos pasos.");
        }

        var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
            authenticatorCode, rememberMe, Input.RememberMachine);

        var userId = await _signInManager.UserManager.GetUserIdAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation("El usuario con ID '{UserId}' ha accedido con 2FA.", userId);
            return LocalRedirect(returnUrl);
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning("La cuenta del usuario con ID '{UserId}' está bloqueada.", userId);
            return RedirectToPage("./Lockout");
        }

        _logger.LogWarning("Código de autenticación no válido para el usuario con ID '{UserId}'.", userId);
        ModelState.AddModelError(string.Empty, "El código de autenticación no es válido.");
        return Page();
    }
}
