using System.ComponentModel.DataAnnotations;

namespace DididaiApp.Core.Models;

/// <summary>
/// Forma de aportación de un <see cref="Socio"/> a la ONG. Clase base de una
/// jerarquía Table-Per-Hierarchy (TPH): EF Core mapea todos los subtipos a una
/// única tabla con una columna discriminadora. Un socio puede tener varias
/// colaboraciones (activas o históricas) de distinto tipo.
/// </summary>
public abstract class Colaboracion
{
    public int Id { get; set; }

    /// <summary>Socio al que pertenece esta colaboración.</summary>
    public int SocioId { get; set; }
    public Socio? Socio { get; set; }

    /// <summary>Importe de la aportación (común a todos los tipos de colaboración).</summary>
    public decimal Importe { get; set; }

    /// <summary>Inicio de la colaboración.</summary>
    public DateTime FechaInicio { get; set; }

    /// <summary>Fin de la colaboración; <c>null</c> mientras sigue vigente.</summary>
    public DateTime? FechaFin { get; set; }

    /// <summary>Indica si la colaboración está actualmente activa.</summary>
    public bool Activa { get; set; } = true;
}

/// <summary>
/// Cuota periódica domiciliada en una cuenta bancaria (el caso del formulario
/// "Hacerme soci@" de la web actual).
/// </summary>
public class CuotaDomiciliada : Colaboracion
{
    /// <summary>Periodicidad de la cuota.</summary>
    public ModalidadCuota Modalidad { get; set; }

    /// <summary>IBAN de la cuenta de cargo.</summary>
    [StringLength(34)]
    public string Iban { get; set; } = string.Empty;
}

/// <summary>Aportación puntual de un importe (donación única por transferencia, etc.).</summary>
public class AportacionUnica : Colaboracion
{
    /// <summary>Fecha en que se realizó la aportación.</summary>
    public DateTime Fecha { get; set; }
}

/// <summary>
/// Participación a través de la plataforma Teaming (microdonación recurrente,
/// típicamente de importe fijo pequeño). No añade campos propios sobre la base.
/// </summary>
public class Teaming : Colaboracion
{
}
