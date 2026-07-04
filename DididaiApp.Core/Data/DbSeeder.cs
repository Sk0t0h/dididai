using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DididaiApp.Core.Data;

/// <summary>
/// Siembra de datos inicial de Identity: crea el rol <c>Admin</c> y un usuario
/// administrador si aún no existen. El correo y la contraseña del admin se leen
/// de configuración (User Secrets en desarrollo, variables de entorno en
/// producción); NUNCA se codifican en el repo, que es público.
/// </summary>
public static class DbSeeder
{
    public const string AdminRole = "Admin";

    public static async Task SeedAdminAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var config = services.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        // Rol Admin.
        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
        }

        // Usuario admin inicial (solo si hay credenciales configuradas y no existe ya).
        var adminEmail = config["Seed:AdminEmail"];
        var adminPassword = config["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning(
                "Seed del admin omitido: falta Seed:AdminEmail o Seed:AdminPassword en la configuración " +
                "(usar User Secrets en desarrollo / variables de entorno en producción).");
            return;
        }

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
        {
            return;
        }

        var admin = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, AdminRole);
            logger.LogInformation("Usuario admin inicial creado: {Email}", adminEmail);
        }
        else
        {
            logger.LogError(
                "No se pudo crear el usuario admin: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }
}
