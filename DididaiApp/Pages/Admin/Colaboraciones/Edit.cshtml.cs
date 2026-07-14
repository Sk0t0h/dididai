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
/// Edición de los datos económicos de una colaboración (importe y, si es cuota
/// domiciliada, periodicidad e IBAN). El tipo y el socio NO se pueden cambiar.
/// </summary>
[Authorize(Roles = DbSeeder.AdminRole)]
public class EditModel : PageModel
{
    private readonly IColaboracionService _colaboraciones;
    private readonly IAuditoriaService _auditoria;

    public EditModel(IColaboracionService colaboraciones, IAuditoriaService auditoria)
    {
        _colaboraciones = colaboraciones;
        _auditoria = auditoria;
    }

    [BindProperty]
    public Entrada Datos { get; set; } = new();

    public string TipoNombre { get; private set; } = string.Empty;
    public bool EsCuota { get; private set; }
    public int SocioId { get; private set; }

    public class Entrada
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Indica el importe.")]
        [Range(0.01, 1_000_000, ErrorMessage = "El importe debe ser mayor que cero.")]
        [Display(Name = "Importe (€)")]
        public decimal? Importe { get; set; }

        [Display(Name = "Periodicidad")]
        public ModalidadCuota Modalidad { get; set; } = ModalidadCuota.Mensual;

        [Display(Name = "IBAN")]
        [StringLength(34)]
        [Iban]
        public string? Iban { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var col = await _colaboraciones.ObtenerAsync(id);
        if (col is null) return NotFound();

        SocioId = col.SocioId;
        Datos.Id = col.Id;
        Datos.Importe = col.Importe;
        (TipoNombre, EsCuota) = col switch
        {
            CuotaDomiciliada c => ("Cuota domiciliada", true),
            AportacionUnica => ("Aportación única", false),
            Teaming => ("Teaming", false),
            _ => ("Colaboración", false),
        };
        if (col is CuotaDomiciliada cuota)
        {
            Datos.Modalidad = cuota.Modalidad;
            Datos.Iban = cuota.Iban;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var col = await _colaboraciones.ObtenerAsync(Datos.Id);
        if (col is null) return NotFound();
        SocioId = col.SocioId;
        EsCuota = col is CuotaDomiciliada;
        TipoNombre = EsCuota ? "Cuota domiciliada" : (col is Teaming ? "Teaming" : "Aportación única");

        // Para cuota domiciliada, el IBAN es obligatorio y validado; en otros tipos se ignora.
        if (EsCuota)
        {
            if (string.IsNullOrWhiteSpace(Datos.Iban))
                ModelState.AddModelError("Datos.Iban", "El IBAN es obligatorio para una cuota domiciliada.");
            else if (!ValidacionIban.EsValido(Datos.Iban))
                ModelState.AddModelError("Datos.Iban", "El IBAN no es válido (revisa país, longitud y dígitos de control).");
        }

        if (!ModelState.IsValid)
            return Page();

        var r = await _colaboraciones.ActualizarAsync(Datos.Id, Datos.Importe!.Value, Datos.Modalidad, Datos.Iban);
        switch (r)
        {
            case ResultadoColaboracion.Creado:
                await _auditoria.RegistrarAsync(TipoAccionAuditoria.ColaboracionEdicion,
                    "Colaboración", Datos.Id.ToString(),
                    $"Edición de colaboración ({TipoNombre}) del socio #{SocioId}: {Datos.Importe!.Value:0.00} €",
                    User.Identity?.Name ?? "desconocido");
                TempData["Mensaje"] = "Colaboración actualizada.";
                return RedirectToPage("/Admin/Socios/Details", new { id = SocioId });
            case ResultadoColaboracion.ImporteInvalido:
                ModelState.AddModelError("Datos.Importe", "El importe debe ser mayor que cero.");
                return Page();
            case ResultadoColaboracion.IbanInvalido:
                ModelState.AddModelError("Datos.Iban", "El IBAN no es válido.");
                return Page();
            case ResultadoColaboracion.NoEncontrada:
                return NotFound();
            default:
                return Page();
        }
    }
}
