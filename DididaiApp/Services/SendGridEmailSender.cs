using Microsoft.AspNetCore.Identity.UI.Services;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace DididaiApp.Services;

/// <summary>
/// Envío de correo real vía SendGrid. Implementa la misma <see cref="IEmailSender"/>
/// que consume Identity (recuperación de contraseña), por lo que sustituir el stub por
/// esta clase no requiere tocar las páginas.
///
/// Configuración (secreta, NUNCA en el repo → User Secrets en dev, variables de entorno
/// en Azure):
///   SendGrid:ApiKey    — API key con permiso Mail Send.
///   SendGrid:FromEmail — remitente verificado en SendGrid (Single Sender / dominio).
///   SendGrid:FromName  — nombre visible del remitente (opcional; por defecto "DIDIDAI").
///
/// Si falta la API key (p. ej. una máquina de desarrollo sin el secreto), NO lanza:
/// registra el aviso en el log y no envía, para que la app siga arrancando y el resto
/// del flujo sea probable. El único efecto es que ese correo no sale.
/// </summary>
public class SendGridEmailSender : IEmailSender
{
    private readonly ILogger<SendGridEmailSender> _logger;
    private readonly string? _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SendGridEmailSender(IConfiguration config, ILogger<SendGridEmailSender> logger)
    {
        _logger = logger;
        _apiKey = config["SendGrid:ApiKey"];
        _fromEmail = config["SendGrid:FromEmail"] ?? "info@dididai.org";
        _fromName = config["SendGrid:FromName"] ?? "DIDIDAI";
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning(
                "SendGrid sin API key configurada: no se envía el correo a {Email} (asunto: {Subject}). " +
                "Configura SendGrid:ApiKey (User Secrets / variable de entorno).",
                email, subject);
            return;
        }

        var client = new SendGridClient(_apiKey);
        var msg = MailHelper.CreateSingleEmail(
            from: new EmailAddress(_fromEmail, _fromName),
            to: new EmailAddress(email),
            subject: subject,
            plainTextContent: null,
            htmlContent: htmlMessage);

        var response = await client.SendEmailAsync(msg);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Correo enviado a {Email} (asunto: {Subject}).", email, subject);
        }
        else
        {
            // No incluimos el cuerpo (puede llevar el enlace de reset) en el log de error.
            var body = await response.Body.ReadAsStringAsync();
            _logger.LogError(
                "SendGrid devolvió {Status} al enviar a {Email}. Detalle: {Detalle}",
                response.StatusCode, email, body);
        }
    }
}
