using Microsoft.AspNetCore.Identity.UI.Services;

namespace DididaiApp.Services;

/// <summary>
/// Implementación provisional de <see cref="IEmailSender"/> que NO envía correos:
/// se limita a registrar el contenido en el log. Permite que el flujo de
/// recuperación de contraseña de Identity funcione de extremo a extremo en
/// desarrollo sin depender de un proveedor externo. Sustituir por un proveedor
/// real (SendGrid / SMTP) antes del despliegue.
/// </summary>
public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogWarning(
            "[EMAIL STUB] Para: {Email} | Asunto: {Subject}\n{Body}",
            email, subject, htmlMessage);
        return Task.CompletedTask;
    }
}
