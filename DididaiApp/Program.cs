using DididaiApp.Core.Data;
using DididaiApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

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

var app = builder.Build();

// Siembra inicial: rol Admin y usuario admin (credenciales desde configuración).
using (var scope = app.Services.CreateScope())
{
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

app.UseRouting();

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
