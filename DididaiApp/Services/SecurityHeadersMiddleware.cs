namespace DididaiApp.Services;

/// <summary>
/// Añade cabeceras de seguridad a cada respuesta, con una Content-Security-Policy
/// estricta como pieza central. La política es <c>default-src 'self'</c> sin
/// <c>'unsafe-inline'</c>: todo el JS, CSS, fuentes e imágenes se sirven desde el
/// propio origen (Bootstrap, jQuery, Chart.js y las fuentes están en wwwroot/lib;
/// no se usan CDNs). Por eso el proyecto prohíbe estilos y manejadores inline en el
/// HTML: con esta CSP activa, cualquier inline no solo es mala práctica, sino que el
/// navegador lo bloquea.
///
/// Se registra pronto en el pipeline para cubrir también los estáticos y las páginas
/// de Identity. Si en el futuro se incorpora un recurso de un tercero (p. ej. el
/// widget de Teaming en un iframe), habrá que ampliar la directiva correspondiente
/// (<c>frame-src</c>) de forma explícita, no relajar toda la política.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    // Directivas explícitas. 'self' en todo lo servible; 'data:' en imágenes porque
    // Bootstrap usa data-URIs en algunos iconos. object/base/frame-ancestors
    // endurecidos. form-action 'self': los formularios solo postean a esta app.
    private const string Csp =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self'; " +
        "img-src 'self' data:; " +
        "font-src 'self'; " +
        "connect-src 'self'; " +
        "object-src 'none'; " +
        "base-uri 'self'; " +
        "frame-ancestors 'self'; " +
        "form-action 'self'";

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["Content-Security-Policy"] = Csp;
        // Endurecimiento complementario, barato y sin efectos colaterales.
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "no-referrer";
        headers["X-Frame-Options"] = "SAMEORIGIN";
        await _next(context);
    }
}
