using System.ComponentModel.DataAnnotations;
using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DididaiApp.Pages.Admin.Economia;

/// <summary>
/// Módulo económico: métricas de ingresos (desde las colaboraciones), balance con los
/// gastos, gestión de gastos (alta/borrado) y vista global de colaboraciones. Los
/// cálculos se delegan en los servicios de Core; la página no toca el DbContext.
/// </summary>
[Authorize(Roles = DbSeeder.AdminRole)]
public class IndexModel : PageModel
{
    private readonly IResumenEconomicoService _resumen;
    private readonly IGastoService _gastos;
    private readonly IColaboracionService _colaboraciones;
    private readonly IAuditoriaService _auditoria;

    public IndexModel(IResumenEconomicoService resumen, IGastoService gastos,
        IColaboracionService colaboraciones, IAuditoriaService auditoria)
    {
        _resumen = resumen;
        _gastos = gastos;
        _colaboraciones = colaboraciones;
        _auditoria = auditoria;
    }

    private string Actor => User.Identity?.Name ?? "desconocido";

    public ResumenEconomico Resumen { get; private set; } = new();
    public IReadOnlyList<Gasto> Gastos { get; private set; } = [];
    public IReadOnlyList<Colaboracion> Colaboraciones { get; private set; } = [];

    // Datos de las gráficas serializados a JSON para los atributos data-chart de los
    // canvas (los pinta dashboard.js con Chart.js). Se construyen en servidor para no
    // meter lógica en la vista y mantener el JS del cliente agnóstico de los datos.
    public string ChartIngresosPorTipo { get; private set; } = "null";
    public string ChartGastosPorCategoria { get; private set; } = "null";
    public string ChartIngresosVsGastos { get; private set; } = "null";
    public string ChartAltasPorMes { get; private set; } = "null";
    public string ChartProyeccion { get; private set; } = "null";

    [BindProperty]
    public NuevoGasto GastoNuevo { get; set; } = new();

    /// <summary>Datos del alta rápida de gasto (formulario inline en la página).</summary>
    public class NuevoGasto
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Concepto")]
        public string Concepto { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 10_000_000, ErrorMessage = "El importe debe ser mayor que cero.")]
        [Display(Name = "Importe (€)")]
        public decimal Importe { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow.Date;

        [Required]
        [Display(Name = "Categoría")]
        public CategoriaGasto Categoria { get; set; } = CategoriaGasto.AccionDirecta;
    }

    private async Task CargarAsync()
    {
        Resumen = await _resumen.ObtenerAsync();
        Gastos = await _gastos.ListarAsync();
        Colaboraciones = await _colaboraciones.ListarTodasAsync();
        // Proyección de los próximos 6 meses desde el mes actual ("si todo sigue igual").
        var hoy = DateTime.UtcNow;
        var proyeccion = await _resumen.ProyectarAsync(new DateTime(hoy.Year, hoy.Month, 1), 6);
        ConstruirGraficas(proyeccion);
    }

    private void ConstruirGraficas(IReadOnlyList<ProyeccionMes> proyeccion)
    {
        // Proyección: gráfica de líneas con DOS series (ingresos y gastos) sobre los
        // meses proyectados. Es extrapolación "si todo sigue igual", no una predicción.
        ChartProyeccion = System.Text.Json.JsonSerializer.Serialize(new
        {
            tipo = "line",
            labels = proyeccion.Select(p => p.Mes).ToArray(),
            series = new[]
            {
                new { etiqueta = "Ingresos previstos", valores = proyeccion.Select(p => p.IngresosProyectados).ToArray() },
                new { etiqueta = "Gastos previstos", valores = proyeccion.Select(p => p.GastosProyectados).ToArray() },
            },
        });

        // Ingresos por tipo (donut).
        ChartIngresosPorTipo = Chart("doughnut", "Ingresos por tipo",
            ["Cuota domiciliada", "Aportación única", "Teaming"],
            [Resumen.IngresosPorTipo[TipoColaboracion.CuotaDomiciliada],
             Resumen.IngresosPorTipo[TipoColaboracion.AportacionUnica],
             Resumen.IngresosPorTipo[TipoColaboracion.Teaming]]);

        // Gastos por categoría (barras); solo las categorías con gasto, con su nombre legible.
        var cats = Resumen.GastosPorCategoria.Keys.ToList();
        ChartGastosPorCategoria = Chart("bar", "Gastos por categoría",
            cats.Select(NombreCategoria).ToArray(),
            cats.Select(c => Resumen.GastosPorCategoria[c]).ToArray());

        // Ingresos vs gastos vs balance (barras).
        ChartIngresosVsGastos = Chart("bar", "Ingresos vs gastos",
            ["Ingresos", "Gastos", "Balance"],
            [Resumen.TotalIngresos, Resumen.TotalGastos, Resumen.Balance]);

        // Altas de colaboraciones por mes (líneas).
        ChartAltasPorMes = Chart("line", "Altas por mes",
            Resumen.AltasPorMes.Select(a => a.Mes).ToArray(),
            Resumen.AltasPorMes.Select(a => (decimal)a.Cantidad).ToArray());
    }

    private static string Chart(string tipo, string etiqueta, string[] labels, decimal[] valores)
        => System.Text.Json.JsonSerializer.Serialize(new { tipo, etiqueta, labels, valores });

    private static string NombreCategoria(CategoriaGasto c)
    {
        var campo = c.GetType().GetField(c.ToString());
        var attr = campo?.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
        return attr?.Name ?? c.ToString();
    }

    public async Task OnGetAsync() => await CargarAsync();

    public async Task<IActionResult> OnPostCrearGastoAsync()
    {
        if (!ModelState.IsValid)
        {
            await CargarAsync();
            return Page();
        }

        var gasto = new Gasto
        {
            Concepto = GastoNuevo.Concepto,
            Importe = GastoNuevo.Importe,
            Fecha = GastoNuevo.Fecha,
            Categoria = GastoNuevo.Categoria,
        };
        var creado = await _gastos.CrearAsync(gasto);
        if (creado)
            await _auditoria.RegistrarAsync(TipoAccionAuditoria.GastoAlta, "Gasto", gasto.Id.ToString(),
                $"Alta de gasto «{gasto.Concepto}» ({gasto.Importe:0.00} €, {NombreCategoria(gasto.Categoria)})", Actor);
        TempData["Mensaje"] = "Gasto registrado.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarGastoAsync(int id)
    {
        // Se lee el gasto ANTES de borrarlo (borrado físico) para dejar el detalle en el log.
        var gasto = await _gastos.ObtenerAsync(id);
        await _gastos.EliminarAsync(id);
        if (gasto is not null)
            await _auditoria.RegistrarAsync(TipoAccionAuditoria.GastoBaja, "Gasto", id.ToString(),
                $"Eliminación de gasto «{gasto.Concepto}» ({gasto.Importe:0.00} €, {NombreCategoria(gasto.Categoria)})", Actor);
        TempData["Mensaje"] = "Gasto eliminado.";
        return RedirectToPage();
    }
}
