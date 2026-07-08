using System.Globalization;
using System.Threading.RateLimiting;
using DididaiApp.Core.Data;
using DididaiApp.Core.Services;
using DididaiApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Idiomas del front público. Ampliar el catálogo es añadir una cultura a esta
// lista y su .resx correspondiente: el resto de la infra (middleware, selector)
// no cambia. El primero es el idioma por defecto.
string[] culturasSoportadas = ["es", "en"];

// Add services to the container.
// Localización de vistas: los textos viven en Resources/**/*.resx y se resuelven
// con IStringLocalizer / IViewLocalizer. El back de gestión (/Admin) NO se localiza.
builder.Services.AddRazorPages()
    .AddViewLocalization();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var culturas = culturasSoportadas.Select(c => new CultureInfo(c)).ToList();
    options.DefaultRequestCulture = new RequestCulture(culturasSoportadas[0]);
    options.SupportedCultures = culturas;
    options.SupportedUICultures = culturas;
    // El idioma lo decide el visitante mediante el selector, que persiste su
    // elección en una cookie. La cookie tiene prioridad sobre Accept-Language.
    options.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
});

// Persistencia: EF Core sobre SQLite. La cadena de conexión (solo la ruta del
// fichero .db, sin secretos) vive en appsettings.json.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Autenticación: ASP.NET Core Identity con roles, sobre el AppDbContext.
// Back de gestión cerrado; el registro público está deshabilitado (las altas de
// usuarios se hacen desde dentro). Sin confirmación de email (altas manuales).
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// Política usada para restringir zonas al rol Admin (incluye el registro de
// usuarios, que solo un admin autenticado puede realizar).
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(DbSeeder.AdminRole));
});

// Rutas de login/logout/denegado: apuntan a las páginas de la Default UI de
// Identity, que viven en el área "Identity".
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Envío de email PROVISIONAL: registra en el log en vez de enviar (permite el
// flujo de recuperación de contraseña sin proveedor externo). Sustituir por
// SendGrid/SMTP antes del despliegue.
builder.Services.AddTransient<IEmailSender, LoggingEmailSender>();

// Servicios de dominio (Core). Las páginas los inyectan; nunca el DbContext directo.
builder.Services.AddScoped<ISocioService, SocioService>();
builder.Services.AddScoped<IColaboracionService, ColaboracionService>();
builder.Services.AddScoped<IResumenEconomicoService, ResumenEconomicoService>();
builder.Services.AddScoped<IGastoService, GastoService>();
builder.Services.AddScoped<ISolicitudColaboracionService, SolicitudColaboracionService>();

// Rate limiting del formulario público de colaboración: frena el spam de bots
// limitando los ENVÍOS (POST) por IP. Solo afecta a la política "colaborar" (la
// landing); el resto de la app no se toca. Importante: solo se cuentan los POST —
// las visitas a la página (GET) NO se limitan, para no bloquear a quien solo navega.
// Ventana fija: pocos envíos por IP y ventana. Al superarse, 429 (sin cola).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("colaborar", httpContext =>
    {
        // Los GET (y demás verbos no mutantes) pasan sin límite.
        if (!HttpMethods.IsPost(httpContext.Request.Method))
            return RateLimitPartition.GetNoLimiter("_sin_limite");

        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(5),
            QueueLimit = 0,
        });
    });
});

var app = builder.Build();

// Arranque de datos: aplicar migraciones pendientes (crea el esquema —incluido
// Identity— si la BD no existe, p. ej. en un despliegue nuevo con /home vacío) y
// luego sembrar el rol y usuario admin. MigrateAsync es idempotente: si el esquema
// ya está al día, no hace nada. Se usa Migrate (no EnsureCreated) para respetar el
// historial de migraciones del proyecto.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAdminAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Cabeceras de seguridad (CSP estricta + endurecimiento) en cada respuesta. Va
// temprano para cubrir también los estáticos y las páginas de Identity.
app.UseMiddleware<DididaiApp.Services.SecurityHeadersMiddleware>();

// Aplica la cultura de la petición (cookie del selector → Accept-Language →
// idioma por defecto) antes del enrutado, para que las vistas se localicen.
app.UseRequestLocalization();

app.UseRouting();

// Rate limiter tras el enrutado (necesita el endpoint resuelto para aplicar su
// política) y antes de la autorización.
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Registro público deshabilitado: las altas de usuarios se hacen desde dentro del
// back (por un Admin), no por auto-registro. Las páginas de registro de la Default
// UI de Identity vienen compiladas en su ensamblado y las convenciones de página
// no las alcanzan de forma fiable, así que se bloquean aquí en el pipeline: toda
// ruta de registro devuelve 404 salvo que la solicite un usuario con rol Admin.
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    if (path.StartsWithSegments("/Identity/Account/Register", StringComparison.OrdinalIgnoreCase)
        && !context.User.IsInRole(DbSeeder.AdminRole))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await next();
});

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
