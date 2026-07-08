using DididaiApp.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DididaiApp.Core.Data;

/// <summary>
/// Contexto EF Core de la aplicación. Contiene los conjuntos de entidades del
/// dominio y la configuración del modelo, y además las tablas de ASP.NET Core
/// Identity (usuarios y roles del back de gestión), al heredar de
/// <see cref="IdentityDbContext{TUser}"/>. La jerarquía <see cref="Colaboracion"/>
/// se mapea con la estrategia Table-Per-Hierarchy por defecto de EF Core.
/// </summary>
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Socio> Socios => Set<Socio>();
    public DbSet<Colaboracion> Colaboraciones => Set<Colaboracion>();
    public DbSet<Gasto> Gastos => Set<Gasto>();
    public DbSet<SolicitudColaboracion> SolicitudesColaboracion => Set<SolicitudColaboracion>();
    public DbSet<AccionSolicitud> AccionesSolicitud => Set<AccionSolicitud>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Registro explícito de los subtipos concretos de la jerarquía TPH.
        // Necesario porque la única navegación (Socio.Colaboraciones) apunta al
        // tipo base abstracto, así que EF no los descubre por sí solo.
        modelBuilder.Entity<CuotaDomiciliada>();
        modelBuilder.Entity<AportacionUnica>();
        modelBuilder.Entity<Teaming>();

        // Precisión explícita para el importe monetario (común a toda la jerarquía;
        // evita el warning de EF sobre decimal sin precisión y fija el formato en la BD).
        modelBuilder.Entity<Colaboracion>().Property(c => c.Importe).HasPrecision(10, 2);
        modelBuilder.Entity<Gasto>().Property(g => g.Importe).HasPrecision(10, 2);
        modelBuilder.Entity<SolicitudColaboracion>().Property(s => s.Importe).HasPrecision(10, 2);

        // DNI único a nivel de BD: identifica inequívocamente a la persona; dos socios
        // con el mismo DNI es siempre un duplicado (aunque uno esté de baja → se reactiva
        // el existente, no se crea otro). El Email NO es único a propósito: es habitual
        // que varias personas compartan correo (familias, un gestor para varios socios).
        // El teléfono TAMPOCO es único, por el mismo motivo (contacto compartido); la
        // detección de "esta solicitud es de un socio que ya existe" es una sugerencia por
        // email/teléfono que decide el admin, no una restricción de unicidad.
        modelBuilder.Entity<Socio>().HasIndex(s => s.Dni).IsUnique();

        // Solicitud → Socio: vínculo opcional. Si se borrara el socio, la solicitud se
        // conserva con SocioId a null (no se arrastra el borrado). El borrado de socio es
        // lógico (FechaBaja), pero se deja explícito por robustez.
        modelBuilder.Entity<SolicitudColaboracion>()
            .HasOne(s => s.Socio)
            .WithMany()
            .HasForeignKey(s => s.SocioId)
            .OnDelete(DeleteBehavior.SetNull);

        // Solicitud → Acciones: el historial de gestión pertenece a la solicitud y se borra
        // con ella (cascade).
        modelBuilder.Entity<AccionSolicitud>()
            .HasOne(a => a.Solicitud)
            .WithMany(s => s.Acciones)
            .HasForeignKey(a => a.SolicitudId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
