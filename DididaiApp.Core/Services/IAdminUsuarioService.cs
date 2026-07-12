namespace DididaiApp.Core.Services;

/// <summary>
/// Gestión de los usuarios administradores del back (rol <c>Admin</c>). Sustituye al
/// registro público (deshabilitado): las altas de administradores se hacen desde dentro,
/// por un admin ya autenticado. La capa de presentación depende de esta abstracción y no
/// toca <c>UserManager</c>/<c>RoleManager</c> directamente.
///
/// <para>Los admins creados aquí nacen con el email confirmado (<c>EmailConfirmed=true</c>):
/// no hay confirmación por correo en el flujo interno, y sin ello la recuperación de
/// contraseña (<c>ForgotPassword</c>, que exige email confirmado) no les funcionaría.</para>
/// </summary>
public interface IAdminUsuarioService
{
    /// <summary>
    /// Crea un usuario administrador con <paramref name="email"/> y <paramref name="password"/>,
    /// email ya confirmado, y lo añade al rol <c>Admin</c>. La contraseña se valida contra la
    /// política de Identity; nunca se registra en logs ni se devuelve.
    /// </summary>
    Task<ResultadoCrearAdmin> CrearAdminAsync(string email, string password);

    /// <summary>
    /// Lista los administradores (usuarios en el rol <c>Admin</c>), ordenados por email.
    /// Devuelve solo datos no sensibles (nunca el hash de la contraseña).
    /// </summary>
    Task<IReadOnlyList<AdminUsuarioDto>> ListarAdminsAsync();

    /// <summary>
    /// Desactiva un administrador (bloquea su inicio de sesión sin borrar la fila, para no
    /// dejar huérfano el historial de auditoría). Baja lógica, reversible con
    /// <see cref="ReactivarAsync"/>. No borra físicamente.
    ///
    /// <para>Guardas: NUNCA se puede desactivar al superadmin (el del <c>Seed:AdminEmail</c>),
    /// ni un usuario puede desactivarse a sí mismo. <paramref name="emailSolicitante"/> es el
    /// admin autenticado que ejecuta la acción (lo fija el servidor).</para>
    /// </summary>
    Task<ResultadoBajaAdmin> DesactivarAsync(string id, string emailSolicitante);

    /// <summary>Reactiva un administrador previamente desactivado (quita el bloqueo).</summary>
    Task<ResultadoBajaAdmin> ReactivarAsync(string id);
}

/// <summary>Datos de un administrador para el listado del back (sin información sensible).</summary>
/// <param name="Id">Identificador del usuario de Identity.</param>
/// <param name="Email">Correo (= nombre de usuario) del administrador.</param>
/// <param name="Activo">
/// <c>true</c> si puede iniciar sesión; <c>false</c> si está desactivado (bloqueado por lockout).
/// </param>
/// <param name="EsSuperAdmin">
/// <c>true</c> si es el administrador primigenio (el del <c>Seed:AdminEmail</c>): intocable,
/// no se puede desactivar. La UI oculta su acción de baja.
/// </param>
public record AdminUsuarioDto(string Id, string Email, bool Activo, bool EsSuperAdmin);

/// <summary>Resultado de desactivar o reactivar un administrador.</summary>
public enum ResultadoBajaAdmin
{
    /// <summary>Operación aplicada.</summary>
    Ok,
    /// <summary>No existe un administrador con ese id.</summary>
    NoEncontrado,
    /// <summary>Es el superadmin: no se puede desactivar.</summary>
    EsSuperAdmin,
    /// <summary>Un usuario no puede desactivarse a sí mismo.</summary>
    NoUnoMismo,
}

/// <summary>Resultado del alta de un administrador.</summary>
public enum ResultadoCrearAdmin
{
    /// <summary>Administrador creado y añadido al rol Admin.</summary>
    Creado,
    /// <summary>Falta el email o la contraseña.</summary>
    DatosIncompletos,
    /// <summary>Ya existe un usuario con ese email.</summary>
    EmailDuplicado,
    /// <summary>La contraseña no cumple la política (longitud, complejidad…).</summary>
    PasswordInvalida,
}
