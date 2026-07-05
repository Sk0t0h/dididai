using DididaiApp.Core.Models;

namespace DididaiApp.Core.Services;

/// <summary>
/// Operaciones de gestión de las colaboraciones (formas de aportación) de un socio.
/// Igual que <see cref="ISocioService"/>, la capa de presentación depende de esta
/// abstracción y nunca toca el <c>AppDbContext</c> directamente. La baja de una
/// colaboración es lógica (se marca inactiva con fecha de fin), no un borrado: el
/// caso habitual es "dejar de pagar" una cuota conservando el histórico.
/// </summary>
public interface IColaboracionService
{
    /// <summary>Colaboraciones de un socio, más recientes primero. Incluye las finalizadas.</summary>
    Task<IReadOnlyList<Colaboracion>> ListarPorSocioAsync(int socioId);

    /// <summary>
    /// Todas las colaboraciones (de todos los socios), con el socio cargado, más
    /// recientes primero. Para la vista global del módulo económico.
    /// </summary>
    Task<IReadOnlyList<Colaboracion>> ListarTodasAsync();

    /// <summary>Obtiene una colaboración por id, o <c>null</c> si no existe.</summary>
    Task<Colaboracion?> ObtenerAsync(int id);

    /// <summary>
    /// Crea una colaboración para un socio. Fija <c>FechaInicio</c> y la marca activa.
    /// Falla si el socio no existe o si los datos no son válidos (ver <see cref="ResultadoColaboracion"/>).
    /// </summary>
    Task<ResultadoColaboracion> CrearAsync(Colaboracion colaboracion);

    /// <summary>
    /// Da de baja (finaliza) una colaboración activa: marca <c>Activa=false</c> y fija
    /// <c>FechaFin</c>. Es el caso de "dejar de pagar" una cuota. Idempotente.
    /// </summary>
    Task DarDeBajaAsync(int id);
}

/// <summary>Resultado de crear una colaboración.</summary>
public enum ResultadoColaboracion
{
    Creado,
    /// <summary>El socio indicado no existe.</summary>
    SocioNoEncontrado,
    /// <summary>El importe debe ser mayor que cero.</summary>
    ImporteInvalido,
    /// <summary>La cuota domiciliada requiere un IBAN válido (mod-97).</summary>
    IbanInvalido,
}
