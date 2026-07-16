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

    private const int TamPagina = 10;

    public ResumenEconomico Resumen { get; private set; } = new();
    public IReadOnlyList<Gasto> Gastos { get; private set; } = [];
    public IReadOnlyList<Colaboracion> Colaboraciones { get; private set; } = [];

    // Paginación de las tablas (en memoria; el volumen de una ONG pequeña lo permite).
    [BindProperty(SupportsGet = true)] public int PgGastos { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int PgColab { get; set; } = 1;
    public int TotalPagsGastos { get; private set; } = 1;
    public int TotalPagsColab { get; private set; } = 1;

    // Rango de fechas del análisis económico (meses completos, inclusive). Por defecto,
    // el año natural en curso. Se enlazan desde el formulario GET de la cabecera.
    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? Desde { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? Hasta { get; set; }

    // Datos de las gráficas serializados a JSON para los atributos data-chart de los
    // canvas (los pinta dashboard.js con Chart.js). Se construyen en servidor para no
    // meter lógica en la vista y mantener el JS del cliente agnóstico de los datos.
    public string ChartIngresosPorTipo { get; private set; } = "null";
    public string ChartGastosPorCategoria { get; private set; } = "null";
    public string ChartIngresosVsGastos { get; private set; } = "null";
    public string ChartAltasPorMes { get; private set; } = "null";
    public string ChartIngresosPorMes { get; private set; } = "null";
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

        [Required]
        [Display(Name = "Periodicidad")]
        public PeriodicidadGasto Periodicidad { get; set; } = PeriodicidadGasto.Puntual;

        [DataType(DataType.Date)]
        [Display(Name = "Fin (si es recurrente)")]
        public DateTime? FechaFin { get; set; }
    }

    private async Task CargarAsync()
    {
        // Rango por defecto: año natural en curso (1 ene – 31 dic del año actual).
        var hoy = DateTime.UtcNow;
        var desde = Desde ?? new DateTime(hoy.Year, 1, 1);
        var hasta = Hasta ?? new DateTime(hoy.Year, 12, 31);
        if (hasta < desde) (desde, hasta) = (hasta, desde);
        Desde = desde;
        Hasta = hasta;

        Resumen = await _resumen.ObtenerAsync(desde, hasta);

        // Gastos y colaboraciones paginados en memoria (10/pág.).
        var gastos = await _gastos.ListarAsync();
        TotalPagsGastos = Math.Max(1, (int)Math.Ceiling(gastos.Count / (double)TamPagina));
        PgGastos = Math.Clamp(PgGastos, 1, TotalPagsGastos);
        Gastos = gastos.Skip((PgGastos - 1) * TamPagina).Take(TamPagina).ToList();

        var colaboraciones = await _colaboraciones.ListarTodasAsync();
        TotalPagsColab = Math.Max(1, (int)Math.Ceiling(colaboraciones.Count / (double)TamPagina));
        PgColab = Math.Clamp(PgColab, 1, TotalPagsColab);
        Colaboraciones = colaboraciones.Skip((PgColab - 1) * TamPagina).Take(TamPagina).ToList();
        // Proyección de los próximos 6 meses desde el mes actual ("si todo sigue igual").
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

        // Altas de colaboraciones por mes (líneas) — captación.
        ChartAltasPorMes = Chart("line", "Altas por mes",
            Resumen.AltasPorMes.Select(a => a.Mes).ToArray(),
            Resumen.AltasPorMes.Select(a => (decimal)a.Cantidad).ToArray());

        // Evolución de ingresos devengados por mes (líneas) — cuánto se ingresa cada mes.
        ChartIngresosPorMes = Chart("line", "Ingresos por mes",
            Resumen.IngresosPorMes.Select(a => a.Mes).ToArray(),
            Resumen.IngresosPorMes.Select(a => a.Valor).ToArray());
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
            Periodicidad = GastoNuevo.Periodicidad,
            FechaFin = GastoNuevo.Periodicidad == PeriodicidadGasto.Puntual ? null : GastoNuevo.FechaFin,
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
