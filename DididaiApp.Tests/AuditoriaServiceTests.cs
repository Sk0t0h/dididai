using DididaiApp.Core.Data;
using DididaiApp.Core.Models;
using DididaiApp.Core.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DididaiApp.Tests;

/// <summary>
/// Pruebas de integración de <see cref="AuditoriaService"/> sobre SQLite en memoria.
/// Cubren el registro (fecha UTC fijada por el servicio, respaldo de usuario, truncado del
/// detalle) y la consulta (filtros por usuario/acción/fecha, orden descendente, paginación).
/// </summary>
public class AuditoriaServiceTests : IDisposable
{
    private readonly SqliteConnection _conn;
    private readonly AppDbContext _db;
    private readonly AuditoriaService _sut;

    public AuditoriaServiceTests()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conn).Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _sut = new AuditoriaService(_db);
    }

    [Fact]
    public async Task Registrar_InsertaConFechaUtcYCampos()
    {
        var antes = DateTime.UtcNow.AddSeconds(-1);

        await _sut.RegistrarAsync(TipoAccionAuditoria.SocioAlta, "Socio", "7", "Alta de socio Ada", "admin@dididai.org");

        var reg = Assert.Single(await _db.RegistrosAuditoria.ToListAsync());
        Assert.Equal(TipoAccionAuditoria.SocioAlta, reg.Accion);
        Assert.Equal("Socio", reg.Entidad);
        Assert.Equal("7", reg.EntidadId);
        Assert.Equal("Alta de socio Ada", reg.Detalle);
        Assert.Equal("admin@dididai.org", reg.Usuario);
        Assert.InRange(reg.Fecha, antes, DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task Registrar_SinUsuario_UsaDesconocido()
    {
        await _sut.RegistrarAsync(TipoAccionAuditoria.SocioBaja, "Socio", "1", "x", "  ");

        var reg = Assert.Single(await _db.RegistrosAuditoria.ToListAsync());
        Assert.Equal("desconocido", reg.Usuario);
    }

    [Fact]
    public async Task Registrar_DetalleMuyLargo_SeTruncaA500()
    {
        var largo = new string('x', 600);

        await _sut.RegistrarAsync(TipoAccionAuditoria.SocioEdicion, "Socio", "1", largo, "admin");

        var reg = Assert.Single(await _db.RegistrosAuditoria.ToListAsync());
        Assert.Equal(500, reg.Detalle.Length);
    }

    [Fact]
    public async Task Listar_OrdenaPorFechaDescendente()
    {
        // Insertados en orden creciente de fecha; se esperan en orden inverso.
        _db.RegistrosAuditoria.AddRange(
            new RegistroAuditoria { Fecha = new DateTime(2026, 1, 1), Usuario = "a", Accion = TipoAccionAuditoria.SocioAlta, Entidad = "Socio", EntidadId = "1", Detalle = "1" },
            new RegistroAuditoria { Fecha = new DateTime(2026, 3, 1), Usuario = "a", Accion = TipoAccionAuditoria.SocioAlta, Entidad = "Socio", EntidadId = "2", Detalle = "3" },
            new RegistroAuditoria { Fecha = new DateTime(2026, 2, 1), Usuario = "a", Accion = TipoAccionAuditoria.SocioAlta, Entidad = "Socio", EntidadId = "3", Detalle = "2" });
        await _db.SaveChangesAsync();

        var pagina = await _sut.ListarAsync();

        Assert.Equal(new[] { "3", "2", "1" }, pagina.Registros.Select(r => r.Detalle));
    }

    [Fact]
    public async Task Listar_FiltraPorUsuarioParcial()
    {
        await SembrarAsync(
            (TipoAccionAuditoria.SocioAlta, "ana@dididai.org"),
            (TipoAccionAuditoria.SocioAlta, "beto@dididai.org"));

        var pagina = await _sut.ListarAsync(usuario: "ana");

        Assert.Equal(1, pagina.Total);
        Assert.Equal("ana@dididai.org", pagina.Registros[0].Usuario);
    }

    [Fact]
    public async Task Listar_FiltraPorAccion()
    {
        await SembrarAsync(
            (TipoAccionAuditoria.SocioAlta, "a"),
            (TipoAccionAuditoria.AdminAlta, "a"),
            (TipoAccionAuditoria.SocioBaja, "a"));

        var pagina = await _sut.ListarAsync(accion: TipoAccionAuditoria.AdminAlta);

        Assert.Equal(1, pagina.Total);
        Assert.Equal(TipoAccionAuditoria.AdminAlta, pagina.Registros[0].Accion);
    }

    [Fact]
    public async Task Listar_FiltraPorRangoDeFechas_HastaInclusivoElDia()
    {
        _db.RegistrosAuditoria.AddRange(
            new RegistroAuditoria { Fecha = new DateTime(2026, 1, 10, 8, 0, 0), Usuario = "a", Entidad = "Socio", EntidadId = "1", Detalle = "dentro" },
            new RegistroAuditoria { Fecha = new DateTime(2026, 1, 15, 23, 30, 0), Usuario = "a", Entidad = "Socio", EntidadId = "2", Detalle = "borde-final" },
            new RegistroAuditoria { Fecha = new DateTime(2026, 1, 20, 0, 0, 0), Usuario = "a", Entidad = "Socio", EntidadId = "3", Detalle = "fuera" });
        await _db.SaveChangesAsync();

        var pagina = await _sut.ListarAsync(desde: new DateTime(2026, 1, 10), hasta: new DateTime(2026, 1, 15));

        Assert.Equal(2, pagina.Total);
        Assert.DoesNotContain(pagina.Registros, r => r.Detalle == "fuera");
        // "hasta" incluye todo el día 15 aunque la hora sea 23:30.
        Assert.Contains(pagina.Registros, r => r.Detalle == "borde-final");
    }

    [Fact]
    public async Task Listar_Pagina_DevuelveTamanoYTotalCorrectos()
    {
        for (var i = 0; i < 5; i++)
            _db.RegistrosAuditoria.Add(new RegistroAuditoria
            {
                Fecha = new DateTime(2026, 1, 1).AddMinutes(i),
                Usuario = "a", Entidad = "Socio", EntidadId = i.ToString(), Detalle = i.ToString(),
            });
        await _db.SaveChangesAsync();

        var pagina1 = await _sut.ListarAsync(pagina: 1, tamanoPagina: 2);
        var pagina3 = await _sut.ListarAsync(pagina: 3, tamanoPagina: 2);

        Assert.Equal(5, pagina1.Total);
        Assert.Equal(3, pagina1.TotalPaginas);
        Assert.Equal(2, pagina1.Registros.Count);
        Assert.Single(pagina3.Registros); // la última página tiene 1
    }

    private async Task SembrarAsync(params (TipoAccionAuditoria accion, string usuario)[] items)
    {
        var t = new DateTime(2026, 1, 1);
        foreach (var (accion, usuario) in items)
        {
            _db.RegistrosAuditoria.Add(new RegistroAuditoria
            {
                Fecha = t, Usuario = usuario, Accion = accion,
                Entidad = "X", EntidadId = "1", Detalle = "d",
            });
            t = t.AddMinutes(1);
        }
        await _db.SaveChangesAsync();
    }

    public void Dispose()
    {
        _db.Dispose();
        _conn.Dispose();
    }
}
