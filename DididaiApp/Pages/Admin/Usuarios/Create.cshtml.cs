using System.ComponentModel.DataAnnotations;
using DididaiApp.Core.Data;
using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages.Admin.Usuarios;

/// <summary>
/// Alta de un usuario administrador desde el back. Sustituye al registro público
/// (deshabilitado): solo un admin autenticado puede crear otro admin. El nuevo usuario
/// nace con el email confirmado y en el rol Admin (lo garantiza el servicio). Solo rol Admin.
/// </summary>
[Authorize(Roles = DbSeeder.AdminRole)]
public class CreateModel : PageModel
{
    private readonly IAdminUsuarioService _admins;

    public CreateModel(IAdminUsuarioService admins) => _admins = admins;

    [BindProperty]
    public Entrada Datos { get; set; } = new();

    /// <summary>Datos del formulario de alta de administrador.</summary>
    public class Entrada
    {
        [Required(ErrorMessage = "Indica el correo.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(256)]
        [Display(Name = "Correo")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Indica la contraseña.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Repite la contraseña.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var resultado = await _admins.CrearAdminAsync(Datos.Email, Datos.Password);
        switch (resultado)
        {
            case ResultadoCrearAdmin.Creado:
                TempData["Mensaje"] = $"Administrador «{Datos.Email}» creado.";
                return RedirectToPage("Index");

            case ResultadoCrearAdmin.EmailDuplicado:
                ModelState.AddModelError("Datos.Email", "Ya existe un usuario con ese correo.");
                return Page();

            case ResultadoCrearAdmin.PasswordInvalida:
                ModelState.AddModelError("Datos.Password", "La contraseña no cumple los requisitos de seguridad.");
                return Page();

            default: // DatosIncompletos: no debería llegar (lo cubre ModelState), pero por si acaso.
                ModelState.AddModelError(string.Empty, "Revisa los datos del formulario.");
                return Page();
        }
    }
}
