using DididaiApp.Core.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace DididaiApp.Core.Services;

/// <summary>
/// Implementación de <see cref="IAdminUsuarioService"/> sobre ASP.NET Core Identity.
/// Encapsula <see cref="UserManager{TUser}"/> para que las páginas no lo usen directamente,
/// aplicando las reglas del alta de administradores (email confirmado, rol Admin) y de la
/// baja lógica (protección del superadmin).
/// </summary>
public class AdminUsuarioService : IAdminUsuarioService
{
    private readonly UserManager<IdentityUser> _userManager;

    /// <summary>
    /// Email del administrador primigenio (superadmin), leído de <c>Seed:AdminEmail</c>. Es el
    /// mismo que siembra <see cref="DbSeeder"/>; a este usuario no se le puede desactivar.
    /// </summary>
    private readonly string? _emailSuperAdmin;

    public AdminUsuarioService(UserManager<IdentityUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _emailSuperAdmin = config["Seed:AdminEmail"]?.Trim();
    }

    private bool EsSuperAdmin(IdentityUser u) =>
        !string.IsNullOrWhiteSpace(_emailSuperAdmin) &&
        string.Equals(u.Email, _emailSuperAdmin, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<ResultadoCrearAdmin> CrearAdminAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return ResultadoCrearAdmin.DatosIncompletos;
        }

        email = email.Trim();

        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return ResultadoCrearAdmin.EmailDuplicado;
        }

        // EmailConfirmed = true: el flujo interno no manda correo de confirmación, y sin ello
        // ForgotPassword (que exige email confirmado) no funcionaría para este admin.
        var usuario = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };

        var creado = await _userManager.CreateAsync(usuario, password);
        if (!creado.Succeeded)
        {
            // Distinguimos duplicado (por si dos peticiones cruzan el FindByEmail) de password
            // débil usando los CÓDIGOS de error de Identity, no su texto (que se localiza).
            var codigos = creado.Errors.Select(e => e.Code).ToList();
            if (codigos.Any(c => c is "DuplicateUserName" or "DuplicateEmail"))
            {
                return ResultadoCrearAdmin.EmailDuplicado;
            }
            return ResultadoCrearAdmin.PasswordInvalida;
        }

        await _userManager.AddToRoleAsync(usuario, DbSeeder.AdminRole);
        return ResultadoCrearAdmin.Creado;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AdminUsuarioDto>> ListarAdminsAsync()
    {
        var admins = await _userManager.GetUsersInRoleAsync(DbSeeder.AdminRole);
        return admins
            .OrderBy(u => u.Email)
            .Select(u => new AdminUsuarioDto(
                u.Id,
                u.Email ?? u.UserName ?? string.Empty,
                Activo: !(u.LockoutEnd is { } fin && fin > DateTimeOffset.UtcNow),
                EsSuperAdmin: EsSuperAdmin(u)))
            .ToList();
    }

    /// <inheritdoc />
    public async Task<ResultadoBajaAdmin> DesactivarAsync(string id, string emailSolicitante)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario is null)
            return ResultadoBajaAdmin.NoEncontrado;

        // El superadmin (el del seed) es intocable, lo pida quien lo pida.
        if (EsSuperAdmin(usuario))
            return ResultadoBajaAdmin.EsSuperAdmin;

        // Nadie puede desactivarse a sí mismo (evita autobloqueo accidental).
        if (string.Equals(usuario.Email, emailSolicitante, StringComparison.OrdinalIgnoreCase))
            return ResultadoBajaAdmin.NoUnoMismo;

        // Baja lógica: bloqueo indefinido. No borra la fila (conserva la auditoría).
        await _userManager.SetLockoutEnabledAsync(usuario, true);
        await _userManager.SetLockoutEndDateAsync(usuario, DateTimeOffset.MaxValue);
        return ResultadoBajaAdmin.Ok;
    }

    /// <inheritdoc />
    public async Task<ResultadoBajaAdmin> ReactivarAsync(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario is null)
            return ResultadoBajaAdmin.NoEncontrado;

        await _userManager.SetLockoutEndDateAsync(usuario, null);
        return ResultadoBajaAdmin.Ok;
    }
}
