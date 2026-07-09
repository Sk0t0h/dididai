// Override propio: recuperación de contraseña en español. Misma lógica que la
// Default UI (no revela si el usuario existe ni si tiene el email confirmado), pero
// usando el IEmailSender no genérico registrado en el proyecto (SendEmailAsync),
// por lo que el cuerpo del correo se compone aquí.
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace DididaiApp.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public class InputModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = default!;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // No revelar que el usuario no existe o no está confirmado.
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code },
                protocol: Request.Scheme)!;

            var enlace = HtmlEncoder.Default.Encode(callbackUrl);
            await _emailSender.SendEmailAsync(
                Input.Email,
                "Restablecer tu contraseña — DIDIDAI",
                $"Para restablecer tu contraseña, <a href='{enlace}'>haz clic aquí</a>. " +
                "Si no has solicitado este cambio, puedes ignorar este mensaje.");

            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        return Page();
    }
}
