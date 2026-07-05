using System.ComponentModel.DataAnnotations;
using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Models.Validation;
using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages.Admin.Colaboraciones;

/// <summary>
/// Alta de una colaboración para un socio. Un solo formulario con selector de tipo:
/// los campos propios de la cuota domiciliada (IBAN, periodicidad) solo se muestran
/// —y se validan— cuando el tipo es CuotaDomiciliada. La página trabaja con un
/// ViewModel plano y construye el subtipo TPH correcto antes de delegar en el servicio.
/// </summary>
[Authorize(Roles = DbSeeder.AdminRole)]
public class CreateModel : PageModel
{
    private readonly IColaboracionService _colaboraciones;
    private readonly ISocioService _socios;

    public CreateModel(IColaboracionService colaboraciones, ISocioService socios)
    {
        _colaboraciones = colaboraciones;
        _socios = socios;
    }

    /// <summary>Tipo de colaboración (discriminador de la jerarquía).</summary>
    public enum TipoColaboracion
    {
        [Display(Name = "Cuota domiciliada")]
        CuotaDomiciliada,
        [Display(Name = "Aportación única")]
        AportacionUnica,
        [Display(Name = "Teaming")]
        Teaming,
    }

    [BindProperty]
    public Entrada Datos { get; set; } = new();

    public string NombreSocio { get; private set; } = string.Empty;

    /// <summary>Datos del formulario de alta (plano; se mapea al subtipo en el POST).</summary>
    public class Entrada
    {
        public int SocioId { get; set; }

        [Required]
        [Display(Name = "Tipo de colaboración")]
        public TipoColaboracion Tipo { get; set; } = TipoColaboracion.CuotaDomiciliada;

        [Required(ErrorMessage = "Indica el importe.")]
        [Range(0.01, 1_000_000, ErrorMessage = "El importe debe ser mayor que cero.")]
        [Display(Name = "Importe (€)")]
        public decimal? Importe { get; set; }

        // Solo para CuotaDomiciliada (se validan condicionalmente en OnPost).
        [Display(Name = "Periodicidad")]
        public ModalidadCuota Modalidad { get; set; } = ModalidadCuota.Mensual;

        [Display(Name = "IBAN")]
        [StringLength(34)]
        [Iban]
        public string? Iban { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int socioId)
    {
        var socio = await _socios.ObtenerAsync(socioId);
        if (socio is null) return NotFound();

        NombreSocio = $"{socio.Nombre} {socio.Apellidos}";
        Datos.SocioId = socioId;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var socio = await _socios.ObtenerAsync(Datos.SocioId);
        if (socio is null) return NotFound();
        NombreSocio = $"{socio.Nombre} {socio.Apellidos}";

        // Validación condicional del IBAN: solo obligatorio/validado para cuota domiciliada.
        if (Datos.Tipo == TipoColaboracion.CuotaDomiciliada)
        {
            if (string.IsNullOrWhiteSpace(Datos.Iban))
                ModelState.AddModelError("Datos.Iban", "El IBAN es obligatorio para una cuota domiciliada.");
            else if (!ValidacionIban.EsValido(Datos.Iban))
                ModelState.AddModelError("Datos.Iban", "El IBAN no es válido (revisa país, longitud y dígitos de control).");
        }

        if (!ModelState.IsValid)
            return Page();

        // ModelState.IsValid ya garantizó que Importe tiene valor ([Required]).
        var importe = Datos.Importe!.Value;
        Colaboracion colaboracion = Datos.Tipo switch
        {
            TipoColaboracion.CuotaDomiciliada => new CuotaDomiciliada
            {
                SocioId = Datos.SocioId,
                Importe = importe,
                Modalidad = Datos.Modalidad,
                Iban = Datos.Iban ?? string.Empty,
            },
            TipoColaboracion.AportacionUnica => new AportacionUnica
            {
                SocioId = Datos.SocioId,
                Importe = importe,
                Fecha = DateTime.UtcNow,
            },
            _ => new Teaming { SocioId = Datos.SocioId, Importe = importe },
        };

        var resultado = await _colaboraciones.CrearAsync(colaboracion);
        switch (resultado)
        {
            case ResultadoColaboracion.Creado:
                TempData["Mensaje"] = "Colaboración añadida.";
                return RedirectToPage("/Admin/Socios/Details", new { id = Datos.SocioId });

            case ResultadoColaboracion.ImporteInvalido:
                ModelState.AddModelError("Datos.Importe", "El importe debe ser mayor que cero.");
                return Page();

            case ResultadoColaboracion.IbanInvalido:
                ModelState.AddModelError("Datos.Iban", "El IBAN no es válido.");
                return Page();

            case ResultadoColaboracion.SocioNoEncontrado:
                return NotFound();

            default:
                return Page();
        }
    }
}
