// Override propio de la página de configuración del authenticator (2FA/TOTP) de
// Identity: vista en español y con el QR generado EN SERVIDOR (data-URI), porque la
// Default UI lo genera con una librería JS que este proyecto no carga (CSP estricta
// sin scripts externos). Misma lógica que la Default UI, con PageModel concreto
// tipado a IdentityUser.
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QRCoder;

namespace DididaiApp.Areas.Identity.Pages.Account.Manage;

public class EnableAuthenticatorModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<EnableAuthenticatorModel> _logger;
    private readonly UrlEncoder _urlEncoder;

    // Nombre del emisor que aparece en la app de autenticación (Google/Microsoft
    // Authenticator, etc.). Se muestra junto a la cuenta al escanear el QR.
    private const string AuthenticatorUriFormat =
        "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
    private const string Issuer = "DIDIDAI";

    public EnableAuthenticatorModel(
        UserManager<IdentityUser> userManager,
        ILogger<EnableAuthenticatorModel> logger,
        UrlEncoder urlEncoder)
    {
        _userManager = userManager;
        _logger = logger;
        _urlEncoder = urlEncoder;
    }

    // Clave compartida, formateada en grupos de 4 para teclearla a mano si no se
    // puede escanear el QR.
    public string SharedKey { get; set; } = default!;

    // QR como imagen embebida (data-URI PNG en base64): CSP-safe, sin JS.
    public string QrCodeDataUri { get; set; } = default!;

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    [TempData]
    public string[]? RecoveryCodes { get; set; }

    public class InputModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El código de verificación es obligatorio.")]
        [System.ComponentModel.DataAnnotations.StringLength(7, ErrorMessage = "El {0} debe tener entre {2} y {1} caracteres.", MinimumLength = 6)]
        [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Text)]
        [System.ComponentModel.DataAnnotations.Display(Name = "Código de verificación")]
        public string Code { get; set; } = default!;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadSharedKeyAndQrCodeUriAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadSharedKeyAndQrCodeUriAsync(user);
            return Page();
        }

        // Normalizar: quitar espacios y guiones que el usuario pueda copiar del QR.
        var verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

        var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2faTokenValid)
        {
            ModelState.AddModelError("Input.Code", "El código de verificación no es válido.");
            await LoadSharedKeyAndQrCodeUriAsync(user);
            return Page();
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        var userId = await _userManager.GetUserIdAsync(user);
        _logger.LogInformation("El usuario con ID '{UserId}' ha activado la autenticación en dos pasos.", userId);

        StatusMessage = "Tu aplicación de autenticación se ha verificado.";

        // Si aún no hay códigos de recuperación, generarlos y mostrarlos una vez.
        if (await _userManager.CountRecoveryCodesAsync(user) == 0)
        {
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            RecoveryCodes = recoveryCodes!.ToArray();
            return RedirectToPage("./ShowRecoveryCodes");
        }

        return RedirectToPage("./TwoFactorAuthentication");
    }

    private async Task LoadSharedKeyAndQrCodeUriAsync(IdentityUser user)
    {
        // Obtener (o restablecer si no existe) la clave del authenticator.
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        SharedKey = FormatKey(unformattedKey!);

        var email = await _userManager.GetEmailAsync(user);
        var authenticatorUri = GenerateQrCodeUri(email!, unformattedKey!);
        QrCodeDataUri = GenerateQrCodePng(authenticatorUri);
    }

    // Agrupa la clave en bloques de 4 para leerla/teclearla con comodidad.
    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            AuthenticatorUriFormat,
            _urlEncoder.Encode(Issuer),
            _urlEncoder.Encode(email),
            unformattedKey);
    }

    // Genera el QR en servidor como PNG y lo devuelve como data-URI base64.
    // CSP-safe: no requiere JS ni recursos externos; img-src ya permite 'data:'.
    private static string GenerateQrCodePng(string payload)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var pngQr = new PngByteQRCode(data);
        var bytes = pngQr.GetGraphic(6);
        return "data:image/png;base64," + Convert.ToBase64String(bytes);
    }
}
