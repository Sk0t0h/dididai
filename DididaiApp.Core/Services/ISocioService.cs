using DididaiApp.Core.Models;

namespace DididaiApp.Core.Services;

/// <summary>
/// Operaciones de gestión de socios (la identidad de la persona colaboradora).
/// La capa de presentación (páginas Razor) depende de esta abstracción y nunca
/// accede al <c>AppDbContext</c> directamente.
/// </summary>
public interface ISocioService
{
    /// <summary>
    /// Lista socios ordenados por apellidos. Por defecto solo los activos; con
    /// <paramref name="incluirBajas"/> incluye también los dados de baja.
    /// <paramref name="busqueda"/> filtra por nombre, apellidos, DNI o email.
    /// </summary>
    Task<IReadOnlyList<Socio>> ListarAsync(bool incluirBajas = false, string? busqueda = null);

    /// <summary>Obtiene un socio por su id, o <c>null</c> si no existe.</summary>
    Task<Socio?> ObtenerAsync(int id);

    /// <summary>Busca un socio por DNI (normalizado), incluidos los de baja.</summary>
    Task<Socio?> ObtenerPorDniAsync(string dni);

    /// <summary>
    /// Socios ACTIVOS que coinciden por email O por teléfono (normalizados) con los datos
    /// dados. Es una SUGERENCIA para detectar si una solicitud pública es de alguien que ya
    /// colabora: como el formulario no pide DNI (la identidad fiable), la coincidencia por
    /// contacto no es concluyente (familias/gestores comparten email/teléfono) y la decide
    /// el admin. Devuelve lista vacía si no hay email ni teléfono con los que buscar.
    /// </summary>
    Task<IReadOnlyList<Socio>> BuscarPosiblesCoincidenciasAsync(string? email, string? telefono);

    /// <summary>
    /// Crea un socio nuevo. Fija <c>FechaAlta</c>. Falla si el DNI ya existe
    /// (ver <see cref="ResultadoAlta"/> para distinguir el caso "existe de baja").
    /// </summary>
    Task<ResultadoAlta> CrearAsync(Socio socio);

    /// <summary>
    /// Actualiza los datos de un socio existente. Falla si el DNI colisiona con otro socio.
    /// Devuelve, junto al resultado, el detalle de los campos que cambiaron (JSON) para la
    /// auditoría, o <c>null</c> si no cambió ninguno o la actualización no se aplicó.
    /// </summary>
    Task<ResultadoActualizacionSocio> ActualizarAsync(Socio socio);

    /// <summary>Da de baja (borrado lógico) un socio activo, fijando <c>FechaBaja</c>.</summary>
    Task DarDeBajaAsync(int id);

    /// <summary>Reactiva un socio dado de baja (limpia <c>FechaBaja</c>).</summary>
    Task ReactivarAsync(int id);
}

/// <summary>Resultado de un alta de socio.</summary>
public enum ResultadoAlta
{
    /// <summary>Alta correcta.</summary>
    Creado,
    /// <summary>Ya existe un socio ACTIVO con ese DNI: es un duplicado real.</summary>
    DniDuplicadoActivo,
    /// <summary>Existe un socio con ese DNI pero dado de BAJA: la UI puede ofrecer reactivarlo.</summary>
    DniExisteDeBaja,
    /// <summary>El país no es un código ISO válido del catálogo.</summary>
    PaisInvalido,
    /// <summary>El documento no es válido para el país indicado (p. ej. DNI/NIE español mal formado).</summary>
    DocumentoInvalido,
    /// <summary>El teléfono no está en formato internacional E.164.</summary>
    TelefonoInvalido,
}

/// <summary>
/// Resultado de actualizar un socio: el desenlace (<see cref="ResultadoActualizacion"/>) y,
/// cuando fue correcto, el detalle de los cambios en JSON (<c>null</c> si no cambió nada).
/// </summary>
public record ResultadoActualizacionSocio(ResultadoActualizacion Resultado, string? Cambios);

/// <summary>Resultado de una actualización de socio.</summary>
public enum ResultadoActualizacion
{
    Actualizado,
    /// <summary>El DNI editado colisiona con otro socio distinto.</summary>
    DniDuplicado,
    /// <summary>El socio a actualizar no existe.</summary>
    NoEncontrado,
    /// <summary>El país no es un código ISO válido del catálogo.</summary>
    PaisInvalido,
    /// <summary>El documento no es válido para el país indicado.</summary>
    DocumentoInvalido,
    /// <summary>El teléfono no está en formato internacional E.164.</summary>
    TelefonoInvalido,
}
