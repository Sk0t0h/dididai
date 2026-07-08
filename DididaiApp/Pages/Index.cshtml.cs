using System.ComponentModel.DataAnnotations;
using DididaiApp.Core.Models;
using DididaiApp.Core.Models.Validation;
using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace DididaiApp.Pages;

/// <summary>
/// Página pública de inicio (landing de la ONG) y formulario de colaboración. El
/// formulario es anónimo: NO da de alta un socio, crea una <see cref="SolicitudColaboracion"/>
/// que el admin revisa. Defensas OWASP del alta pública:
/// <list type="bullet">
///   <item>Antiforgery (token en el form + validación automática de Razor Pages).</item>
///   <item>Honeypot: un campo oculto que un humano no rellena; si viene con valor, se
///   descarta silenciosamente (respuesta neutra, sin pistas al bot).</item>
///   <item>Rate limit por IP (política "colaborar" en Program.cs) para frenar el spam.</item>
///   <item>Validación en servidor (además de cliente), reusando los validadores de dominio
///   vía el servicio; el IBAN NUNCA se pide aquí.</item>
///   <item>Mensaje de resultado neutro: no revela si el email ya existía ni detalles internos.</item>
/// </list>
/// </summary>
[EnableRateLimiting("colaborar")]
public class IndexModel : PageModel
{
    private readonly ISolicitudColaboracionService _solicitudes;
    private readonly IViewLocalizer _localizer;

    public IndexModel(ISolicitudColaboracionService solicitudes, IViewLocalizer localizer)
    {
        _solicitudes = solicitudes;
        _localizer = localizer;
    }

    [BindProperty]
    public FormularioColaboracion Colaborar { get; set; } = new();

    /// <summary>
    /// Honeypot antispam. Es un campo de texto oculto por CSS (no por <c>type=hidden</c>,
    /// para que los bots lo rellenen). Una persona nunca lo ve ni lo rellena; si llega con
    /// contenido, la petición es de un bot y se ignora. No se persiste.
    /// </summary>
    [BindProperty]
    public string? Web { get; set; }

    /// <summary>true tras un envío correcto: la vista muestra el mensaje de agradecimiento.</summary>
    public bool Enviado { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Honeypot: si el campo trampa viene relleno, es un bot. Respuesta neutra
        // (fingimos éxito) para no darle señal de que ha sido detectado.
        if (!string.IsNullOrEmpty(Web))
        {
            Enviado = true;
            return Page();
        }

        // El consentimiento RGPD es obligatorio: un bool [Required] valida presencia, no
        // que sea true; se comprueba explícitamente.
        if (!Colaborar.AceptaPrivacidad)
            ModelState.AddModelError("Colaborar.AceptaPrivacidad",
                _localizer["Form_Error_Consentimiento"].Value);

        if (!ModelState.IsValid)
            return Page();

        var solicitud = new SolicitudColaboracion
        {
            Nombre = Colaborar.Nombre!.Trim(),
            Apellidos = Colaborar.Apellidos!.Trim(),
            Email = Colaborar.Email!.Trim(),
            Telefono = Colaborar.Telefono!.Trim(),
            Tipo = Colaborar.Tipo,
            Importe = Colaborar.Importe,
            Periodicidad = Colaborar.Tipo == TipoColaboracionSolicitada.Socio ? Colaborar.Periodicidad : null,
            AceptaPrivacidad = Colaborar.AceptaPrivacidad,
        };

        var resultado = await _solicitudes.CrearAsync(solicitud);
        switch (resultado)
        {
            case ResultadoSolicitud.Registrada:
                Enviado = true;
                // Se limpia el formulario para no reenviar por recarga.
                Colaborar = new FormularioColaboracion();
                ModelState.Clear();
                return Page();

            case ResultadoSolicitud.TelefonoInvalido:
                ModelState.AddModelError("Colaborar.Telefono", _localizer["Form_Error_Telefono"].Value);
                return Page();

            case ResultadoSolicitud.FaltaConsentimiento:
                ModelState.AddModelError("Colaborar.AceptaPrivacidad", _localizer["Form_Error_Consentimiento"].Value);
                return Page();

            default:
                return Page();
        }
    }

    /// <summary>
    /// Datos que recoge el formulario público. Modelo de entrada aparte de la entidad:
    /// solo los campos que el visitante puede rellenar (sin estado, fechas ni IBAN).
    /// </summary>
    public class FormularioColaboracion
    {
        [Required(ErrorMessage = "Indica tu nombre.")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "Indica tus apellidos.")]
        [StringLength(150)]
        [Display(Name = "Apellidos")]
        public string? Apellidos { get; set; }

        [Required(ErrorMessage = "Indica tu email.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Indica tu teléfono.")]
        [StringLength(30)]
        [TelefonoE164]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Display(Name = "Tipo de colaboración")]
        public TipoColaboracionSolicitada Tipo { get; set; } = TipoColaboracionSolicitada.Socio;

        [Range(1, 1_000_000, ErrorMessage = "Introduce un importe válido.")]
        [Display(Name = "Importe (€)")]
        public decimal? Importe { get; set; }

        [Display(Name = "Periodicidad")]
        public ModalidadCuota? Periodicidad { get; set; }

        // El consentimiento RGPD es obligatorio. Un bool [Required] no basta (valida
        // presencia, no que sea true); [Range(true,true)] fuerza el valor true y, además,
        // emite el data-val que jquery-validation consume para bloquearlo en CLIENTE
        // (evita el submit "gastado" cuando el usuario no marca la casilla).
        [Range(typeof(bool), "true", "true", ErrorMessage = "Necesitamos tu consentimiento para tratar tus datos y gestionar la colaboración.")]
        [Display(Name = "Acepto la política de privacidad")]
        public bool AceptaPrivacidad { get; set; }
    }
}
