using DididaiApp.Core.Data;
using DididaiApp.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DididaiApp.Tests;

/// <summary>
/// Pruebas de integración de <see cref="AdminUsuarioService"/> sobre un
/// <see cref="UserManager{TUser}"/> real respaldado por SQLite en memoria. Cubren el alta
/// de administradores (email confirmado + rol Admin, duplicado, contraseña débil) y el
/// listado. La instrumentación de Identity se monta vía DI para usar los stores reales.
/// </summary>
public class AdminUsuarioServiceTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AdminUsuarioService _sut;

    /// <summary>Email del superadmin usado en los tests (= Seed:AdminEmail de la config de prueba).</summary>
    private const string SuperAdminEmail = "super@dididai.org";

    public AdminUsuarioServiceTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(_conn));
        services.AddIdentityCore<IdentityUser>(o => o.Password.RequiredLength = 8)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
        var sp = _scope.ServiceProvider;
        sp.GetRequiredService<AppDbContext>().Database.EnsureCreated();

        _userManager = sp.GetRequiredService<UserManager<IdentityUser>>();
        _roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        _roleManager.CreateAsync(new IdentityRole(DbSeeder.AdminRole)).GetAwaiter().GetResult();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Seed:AdminEmail"] = SuperAdminEmail })
            .Build();
        _sut = new AdminUsuarioService(_userManager, config);
    }

    /// <summary>Crea el superadmin (el del seed) como admin, tal y como haría el DbSeeder.</summary>
    private async Task<string> CrearSuperAdminAsync()
    {
        await _sut.CrearAdminAsync(SuperAdminEmail, "Password1!");
        var u = await _userManager.FindByEmailAsync(SuperAdminEmail);
        return u!.Id;
    }

    [Fact]
    public async Task Crear_Valido_CreaAdminConfirmadoYEnRolAdmin()
    {
        var r = await _sut.CrearAdminAsync("nuevo@dididai.org", "Password1!");

        Assert.Equal(ResultadoCrearAdmin.Creado, r);

        var usuario = await _userManager.FindByEmailAsync("nuevo@dididai.org");
        Assert.NotNull(usuario);
        Assert.True(usuario!.EmailConfirmed);
        Assert.True(await _userManager.IsInRoleAsync(usuario, DbSeeder.AdminRole));
    }

    [Fact]
    public async Task Crear_EmailDuplicado_NoCreaSegundo()
    {
        await _sut.CrearAdminAsync("dup@dididai.org", "Password1!");

        var r = await _sut.CrearAdminAsync("dup@dididai.org", "OtraPass9!");

        Assert.Equal(ResultadoCrearAdmin.EmailDuplicado, r);
        var admins = await _sut.ListarAdminsAsync();
        Assert.Single(admins, a => a.Email == "dup@dididai.org");
    }

    [Fact]
    public async Task Crear_PasswordCorta_DevuelvePasswordInvalidaYNoCrea()
    {
        var r = await _sut.CrearAdminAsync("corta@dididai.org", "abc");

        Assert.Equal(ResultadoCrearAdmin.PasswordInvalida, r);
        Assert.Null(await _userManager.FindByEmailAsync("corta@dididai.org"));
    }

    [Fact]
    public async Task Crear_DatosIncompletos_DevuelveDatosIncompletos()
    {
        Assert.Equal(ResultadoCrearAdmin.DatosIncompletos, await _sut.CrearAdminAsync("", "Password1!"));
        Assert.Equal(ResultadoCrearAdmin.DatosIncompletos, await _sut.CrearAdminAsync("x@dididai.org", "  "));
    }

    [Fact]
    public async Task Listar_DevuelveSoloAdmins_OrdenadosPorEmail_YActivos()
    {
        await _sut.CrearAdminAsync("zeta@dididai.org", "Password1!");
        await _sut.CrearAdminAsync("alfa@dididai.org", "Password1!");
        // Un usuario que NO es admin no debe aparecer.
        await _userManager.CreateAsync(new IdentityUser { UserName = "no@dididai.org", Email = "no@dididai.org" }, "Password1!");

        var admins = await _sut.ListarAdminsAsync();

        Assert.Equal(2, admins.Count);
        Assert.Equal("alfa@dididai.org", admins[0].Email);
        Assert.Equal("zeta@dididai.org", admins[1].Email);
        Assert.All(admins, a => Assert.True(a.Activo));
    }

    [Fact]
    public async Task Listar_MarcaSuperAdmin()
    {
        await CrearSuperAdminAsync();
        await _sut.CrearAdminAsync("normal@dididai.org", "Password1!");

        var admins = await _sut.ListarAdminsAsync();

        Assert.True(admins.Single(a => a.Email == SuperAdminEmail).EsSuperAdmin);
        Assert.False(admins.Single(a => a.Email == "normal@dididai.org").EsSuperAdmin);
    }

    [Fact]
    public async Task Desactivar_SuperAdmin_NoPermitido_YSigueActivo()
    {
        var superId = await CrearSuperAdminAsync();

        // Lo intenta otro admin distinto (no es "uno mismo").
        var r = await _sut.DesactivarAsync(superId, "otro@dididai.org");

        Assert.Equal(ResultadoBajaAdmin.EsSuperAdmin, r);
        var admins = await _sut.ListarAdminsAsync();
        Assert.True(admins.Single(a => a.Email == SuperAdminEmail).Activo);
    }

    [Fact]
    public async Task Desactivar_AUnoMismo_NoPermitido()
    {
        await _sut.CrearAdminAsync("yo@dididai.org", "Password1!");
        var yo = await _userManager.FindByEmailAsync("yo@dididai.org");

        var r = await _sut.DesactivarAsync(yo!.Id, "yo@dididai.org");

        Assert.Equal(ResultadoBajaAdmin.NoUnoMismo, r);
        var admins = await _sut.ListarAdminsAsync();
        Assert.True(admins.Single(a => a.Email == "yo@dididai.org").Activo);
    }

    [Fact]
    public async Task Desactivar_AdminNormal_PorOtro_LoBloquea_YReactivarLoRestaura()
    {
        await _sut.CrearAdminAsync("victima@dididai.org", "Password1!");
        var victima = await _userManager.FindByEmailAsync("victima@dididai.org");

        var baja = await _sut.DesactivarAsync(victima!.Id, "jefe@dididai.org");
        Assert.Equal(ResultadoBajaAdmin.Ok, baja);
        Assert.True(await _userManager.IsLockedOutAsync((await _userManager.FindByIdAsync(victima.Id))!));
        var trasBaja = await _sut.ListarAdminsAsync();
        Assert.False(trasBaja.Single(a => a.Email == "victima@dididai.org").Activo);

        var alta = await _sut.ReactivarAsync(victima.Id);
        Assert.Equal(ResultadoBajaAdmin.Ok, alta);
        var trasAlta = await _sut.ListarAdminsAsync();
        Assert.True(trasAlta.Single(a => a.Email == "victima@dididai.org").Activo);
    }

    [Fact]
    public async Task Desactivar_IdInexistente_DevuelveNoEncontrado()
    {
        Assert.Equal(ResultadoBajaAdmin.NoEncontrado, await _sut.DesactivarAsync("no-existe", "x@dididai.org"));
    }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
        _conn.Dispose();
    }
}
