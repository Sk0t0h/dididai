using DididaiApp.Core.Models;

namespace DididaiApp.Pages.Admin.Socios;

/// <summary>
/// Modelo del partial <c>_TablaColaboraciones</c>: la lista de colaboraciones de un
/// socio y el id del socio (para los enlaces de alta y el POST de baja). Lo comparten
/// la ficha (Details) y la edición (Edit) del socio.
/// </summary>
public sealed class TablaColaboracionesModel
{
    public required int SocioId { get; init; }
    public required IReadOnlyList<Colaboracion> Colaboraciones { get; init; }
}
