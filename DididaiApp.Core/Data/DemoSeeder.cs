using System.Numerics;
using System.Text;
using DididaiApp.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DididaiApp.Core.Data;

/// <summary>
/// Siembra datos de DEMO ficticios (RGPD-safe) para poblar el back y que la evaluación
/// del TFM —que se hace sobre producción— pueda probar todas las funcionalidades con
/// vida real: socios, colaboraciones de los 3 tipos, gastos por categoría, solicitudes
/// en todos los estados y un historial de auditoría.
///
/// <para>Se activa con el flag de configuración <c>Seed:DemoData=true</c> (User Secrets
/// en dev, app setting en Azure). Es <b>idempotente</b>: si ya hay socios, no hace nada
/// (no duplica en reinicios). Tras sembrar en prod, conviene apagar el flag.</para>
///
/// <para>Diseño: inserta directamente por <see cref="AppDbContext"/> —en vez de por los
/// services— para poder <b>fechar</b> altas/gastos repartidos en ~12 meses (los services
/// fijan la fecha a "ahora", lo que aplanaría los dashboards). A cambio, los valores se
/// generan cumpliendo las validaciones reales: DNI/NIE con letra de control correcta
/// (mód-23), IBAN con dígitos de control mod-97 (ISO 7064), teléfonos en E.164. La
/// auditoría se registra a mano imitando lo que hacen las páginas (la traza la disparan
/// las páginas, no los services).</para>
///
/// <para>Todos los datos son claramente ficticios: nombres inventados, emails
/// <c>@example.org</c>, DNIs válidos-en-formato pero no reales, IBAN de prueba.</para>
/// </summary>
public static class DemoSeeder
{
    public static async Task SeedDemoAsync(IServiceProvider services)
    {
        var config = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DemoSeeder");

        if (!config.GetValue<bool>("Seed:DemoData"))
        {
            return; // flag apagado: no sembrar
        }

        var db = services.GetRequiredService<AppDbContext>();

        // Idempotencia: si ya hay socios, se asume sembrado; no duplicar.
        if (await db.Socios.AnyAsync())
        {
            logger.LogInformation("DemoSeeder: la BD ya tiene socios; no se siembran datos de demo.");
            return;
        }

        logger.LogInformation("DemoSeeder: sembrando datos de demo…");

        var ahora = DateTime.UtcNow;
        const string usuarioDemo = "admin@dididai.org";
        var auditoria = new List<RegistroAuditoria>();

        // ---- Socios ----------------------------------------------------------------
        var socios = new List<Socio>();
        for (int i = 0; i < NombresPila.Length; i++)
        {
            var nombre = NombresPila[i];
            var apellidos = $"{Apellidos1[i % Apellidos1.Length]} {Apellidos2[(i * 3) % Apellidos2.Length]}";
            var pais = PaisesSocio[i % PaisesSocio.Length];
            // La mayoría españoles con DNI; algunos NIE; los extranjeros, pasaporte.
            TipoDocumento tipo;
            string doc;
            if (pais == "ES" && i % 5 != 0)
            {
                tipo = TipoDocumento.DniEspanol;
                doc = DniValido(10_000_000 + i * 137);
            }
            else if (pais == "ES")
            {
                tipo = TipoDocumento.Nie;
                doc = NieValido(i);
            }
            else
            {
                tipo = TipoDocumento.Pasaporte;
                doc = $"P{pais}{100000 + i}";
            }

            // Alta repartida en los últimos 12 meses (más recientes según i baja).
            var fechaAlta = ahora.AddMonths(-(11 - (i % 12))).AddDays(-(i % 27));
            // 2-3 socios de baja (los de índice 4 y 18).
            DateTime? fechaBaja = (i == 4 || i == 18) ? fechaAlta.AddMonths(3) : null;

            var socio = new Socio
            {
                Nombre = nombre,
                Apellidos = apellidos,
                TipoDocumento = tipo,
                Dni = doc,
                Telefono = TelefonoE164(pais, i),
                Email = $"{Quitar(nombre)}.{Quitar(apellidos.Split(' ')[0])}{i}@example.org".ToLowerInvariant(),
                PaisResidencia = pais,
                Localidad = Localidades[i % Localidades.Length],
                CodigoPostal = (28000 + i * 7).ToString(),
                Direccion = $"Calle Ficticia {i + 1}",
                AceptaPrivacidad = true,
                FechaAlta = fechaAlta,
                FechaBaja = fechaBaja,
            };
            socios.Add(socio);
        }
        db.Socios.AddRange(socios);
        await db.SaveChangesAsync(); // asigna Ids

        foreach (var s in socios)
        {
            auditoria.Add(Reg(TipoAccionAuditoria.SocioAlta, "Socio", s.Id.ToString(),
                $"Alta del socio «{s.Nombre} {s.Apellidos}»", usuarioDemo, s.FechaAlta));
            if (s.FechaBaja is not null)
                auditoria.Add(Reg(TipoAccionAuditoria.SocioBaja, "Socio", s.Id.ToString(),
                    $"Baja del socio «{s.Nombre} {s.Apellidos}»", usuarioDemo, s.FechaBaja.Value));
        }

        // ---- Colaboraciones (TPH: los 3 tipos) -------------------------------------
        var colaboraciones = new List<Colaboracion>();
        var activos = socios.Where(s => s.Activo).ToList();
        for (int i = 0; i < activos.Count; i++)
        {
            var s = activos[i];
            var inicio = s.FechaAlta.AddDays(2);
            // Repartir tipos: ~55% cuota, ~30% donación, ~15% teaming.
            int r = i % 20;
            if (r < 11)
            {
                var modalidad = (i % 3 == 0) ? ModalidadCuota.Anual : ModalidadCuota.Mensual;
                var importe = modalidad == ModalidadCuota.Anual ? 120m + (i % 4) * 60 : 10m + (i % 5) * 5;
                colaboraciones.Add(new CuotaDomiciliada
                {
                    SocioId = s.Id, Importe = importe, FechaInicio = inicio,
                    Modalidad = modalidad, Iban = IbanEspanolValido(i),
                    Activa = (i != 6), FechaFin = (i == 6) ? inicio.AddMonths(5) : null,
                });
            }
            else if (r < 17)
            {
                var fecha = inicio.AddDays(i % 60);
                colaboraciones.Add(new AportacionUnica
                {
                    SocioId = s.Id, Importe = 25m + (i % 8) * 15, FechaInicio = inicio,
                    Fecha = fecha, Activa = true,
                });
            }
            else
            {
                colaboraciones.Add(new Teaming
                {
                    SocioId = s.Id, Importe = 1m, FechaInicio = inicio, Activa = true,
                });
            }
        }
        db.Colaboraciones.AddRange(colaboraciones);
        await db.SaveChangesAsync();

        foreach (var c in colaboraciones)
        {
            var tipo = c switch
            {
                CuotaDomiciliada => "cuota domiciliada",
                AportacionUnica => "aportación única",
                Teaming => "Teaming",
                _ => "colaboración",
            };
            auditoria.Add(Reg(TipoAccionAuditoria.ColaboracionAlta, "Colaboración", c.Id.ToString(),
                $"Alta de {tipo} ({c.Importe:0.00} €) del socio #{c.SocioId}", usuarioDemo, c.FechaInicio));
            if (!c.Activa && c.FechaFin is not null)
                auditoria.Add(Reg(TipoAccionAuditoria.ColaboracionBaja, "Colaboración", c.Id.ToString(),
                    $"Baja de {tipo} del socio #{c.SocioId}", usuarioDemo, c.FechaFin.Value));
        }

        // ---- Gastos (5 categorías, repartidos 12 meses) ----------------------------
        var gastos = new List<Gasto>();
        for (int i = 0; i < GastosConceptos.Length; i++)
        {
            var (concepto, categoria, baseImporte) = GastosConceptos[i];
            var fecha = ahora.AddMonths(-(11 - (i % 12))).AddDays(-(i % 20));
            gastos.Add(new Gasto
            {
                Concepto = concepto,
                Categoria = categoria,
                Importe = baseImporte + (i % 6) * 12.50m,
                Fecha = fecha,
            });
        }
        db.Gastos.AddRange(gastos);
        await db.SaveChangesAsync();

        foreach (var g in gastos)
            auditoria.Add(Reg(TipoAccionAuditoria.GastoAlta, "Gasto", g.Id.ToString(),
                $"Alta de gasto «{g.Concepto}» ({g.Importe:0.00} €, {g.Categoria})", usuarioDemo, g.Fecha));

        // ---- Solicitudes (los 4 estados) + acciones --------------------------------
        var solicitudes = new List<SolicitudColaboracion>();
        for (int i = 0; i < SolicitudesDemo.Length; i++)
        {
            var (nombre, apellidos, tipo, estado) = SolicitudesDemo[i];
            var fecha = ahora.AddDays(-(i * 9 + 3));
            solicitudes.Add(new SolicitudColaboracion
            {
                Nombre = nombre, Apellidos = apellidos,
                Email = $"{Quitar(nombre)}.{Quitar(apellidos)}@example.org".ToLowerInvariant(),
                Telefono = TelefonoE164("ES", 500 + i),
                Tipo = tipo,
                Importe = tipo == TipoColaboracionSolicitada.Socio ? 15m
                        : tipo == TipoColaboracionSolicitada.Donacion ? 50m : (decimal?)null,
                Periodicidad = tipo == TipoColaboracionSolicitada.Socio ? ModalidadCuota.Mensual : null,
                AceptaPrivacidad = true,
                FechaSolicitud = fecha,
                Estado = estado,
                FechaRevision = (estado == EstadoSolicitud.Aprobada || estado == EstadoSolicitud.Cancelada)
                    ? fecha.AddDays(2) : null,
                NotaRevision = estado == EstadoSolicitud.Aprobada ? "Datos confirmados por teléfono."
                    : estado == EstadoSolicitud.Cancelada ? "No responde tras varios intentos." : null,
            });
        }
        db.SolicitudesColaboracion.AddRange(solicitudes);
        await db.SaveChangesAsync();

        // Acciones de gestión para las que no están Pendiente
        var acciones = new List<AccionSolicitud>();
        foreach (var sol in solicitudes.Where(x => x.Estado != EstadoSolicitud.Pendiente))
        {
            acciones.Add(new AccionSolicitud
            {
                SolicitudId = sol.Id, Tipo = TipoAccionSolicitud.Telefono,
                Nota = "Contacto telefónico para confirmar los datos.",
                Usuario = usuarioDemo, Fecha = sol.FechaSolicitud.AddDays(1),
            });
        }
        db.AccionesSolicitud.AddRange(acciones);
        await db.SaveChangesAsync();

        foreach (var sol in solicitudes)
        {
            if (sol.Estado == EstadoSolicitud.Aprobada)
                auditoria.Add(Reg(TipoAccionAuditoria.SolicitudAprobada, "Solicitud", sol.Id.ToString(),
                    $"Solicitud de «{sol.Nombre} {sol.Apellidos}» aprobada", usuarioDemo, sol.FechaRevision!.Value));
            else if (sol.Estado == EstadoSolicitud.Cancelada)
                auditoria.Add(Reg(TipoAccionAuditoria.SolicitudCancelada, "Solicitud", sol.Id.ToString(),
                    $"Solicitud de «{sol.Nombre} {sol.Apellidos}» cancelada", usuarioDemo, sol.FechaRevision!.Value));
        }

        // ---- Auditoría (ordenada por fecha, como se habría ido registrando) --------
        db.RegistrosAuditoria.AddRange(auditoria.OrderBy(a => a.Fecha));
        await db.SaveChangesAsync();

        logger.LogInformation(
            "DemoSeeder: sembrados {Socios} socios, {Colab} colaboraciones, {Gastos} gastos, {Sol} solicitudes, {Aud} registros de auditoría.",
            socios.Count, colaboraciones.Count, gastos.Count, solicitudes.Count, auditoria.Count);
    }

    // --- Helpers de auditoría ---------------------------------------------------------
    private static RegistroAuditoria Reg(
        TipoAccionAuditoria accion, string entidad, string entidadId, string detalle, string usuario, DateTime fecha)
        => new()
        {
            Accion = accion, Entidad = entidad, EntidadId = entidadId,
            Detalle = detalle.Length > 500 ? detalle[..500] : detalle,
            Usuario = usuario, Fecha = fecha,
        };

    // --- Generadores de valores VÁLIDOS ----------------------------------------------
    private const string LetrasDni = "TRWAGMYFPDXBNJZSQVHLCKE";

    // DNI español: 8 dígitos + letra de control (mód-23).
    private static string DniValido(int numero)
    {
        numero %= 100_000_000;
        return $"{numero:D8}{LetrasDni[numero % 23]}";
    }

    // NIE: [XYZ] + 7 dígitos + letra. X=0, Y=1, Z=2 para el cálculo de la letra.
    private static string NieValido(int i)
    {
        char[] pref = { 'X', 'Y', 'Z' };
        var p = pref[i % 3];
        int prefValor = i % 3; // X->0, Y->1, Z->2
        int siete = 1_000_000 + (i * 53) % 8_999_999;
        long numero = long.Parse($"{prefValor}{siete:D7}");
        char letra = LetrasDni[(int)(numero % 23)];
        return $"{p}{siete:D7}{letra}";
    }

    // IBAN español válido (24 caracteres) con dígitos de control mod-97 calculados.
    private static string IbanEspanolValido(int i)
    {
        // BBAN de 20 dígitos (banco 4 + oficina 4 + control 2 + cuenta 10), ficticio.
        // El nº de cuenta se construye con un offset alto para que no salga casi todo
        // ceros en índices bajos (más realista visualmente; sigue siendo válido mod-97).
        long cuenta = (1_234_500_000L + (long)i * 987_654_321L) % 10_000_000_000L;
        string bban = $"{2100 + (i % 800):D4}{1000 + (i % 900):D4}{i % 100:D2}{cuenta:D10}";
        // Dígitos de control: se calculan con país+00 al final y mod-97.
        string tmp = bban + "ES00";
        var sb = new StringBuilder();
        foreach (var c in tmp)
            sb.Append(char.IsDigit(c) ? c.ToString() : (c - 'A' + 10).ToString());
        int resto = (int)(BigInteger.Parse(sb.ToString()) % 97);
        int control = 98 - resto;
        return $"ES{control:D2}{bban}";
    }

    // Teléfono en formato E.164 con prefijo del país.
    private static string TelefonoE164(string pais, int i)
    {
        var prefijo = PrefijosTelefonicos.CodigoDePais(pais) ?? "+34";
        var numero = 600_000_000 + (i * 7919) % 99_999_999;
        return $"{prefijo}{numero}";
    }

    private static string Quitar(string s) => s
        .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
        .Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U")
        .Replace("ñ", "n").Replace("Ñ", "N").Replace(" ", "");

    // --- Catálogos ficticios ----------------------------------------------------------
    private static readonly string[] NombresPila =
    {
        "Lucía", "Mateo", "Sofía", "Hugo", "Martina", "Daniel", "Paula", "Alejandro",
        "Valeria", "Pablo", "Emma", "Adrián", "Julia", "Diego", "Carla", "Álvaro",
        "Noa", "Marcos", "Vega", "Iván", "Olivia", "Bruno", "Ana", "Leo",
        "Clara", "Gonzalo", "Sara", "Nicolás", "Elena", "Marc",
    };
    private static readonly string[] Apellidos1 =
    {
        "García", "Fernández", "Rodríguez", "López", "Martín", "Sánchez", "Pérez",
        "Gómez", "Ruiz", "Díaz", "Moreno", "Jiménez",
    };
    private static readonly string[] Apellidos2 =
    {
        "Serrano", "Blanco", "Molina", "Castro", "Ortega", "Rubio", "Marín",
        "Iglesias", "Vega", "Cortés", "Santos", "Herrera",
    };
    private static readonly string[] Localidades =
    {
        "Madrid", "Barcelona", "Valencia", "Sevilla", "Bilbao", "Zaragoza",
        "Málaga", "Granada", "Vigo", "Gijón",
    };
    // Mayoría ES; algunos internacionales (IBAN/tel soportados).
    private static readonly string[] PaisesSocio =
    {
        "ES", "ES", "ES", "ES", "ES", "GB", "ES", "ES", "ES", "FR",
        "ES", "ES", "DE", "ES", "ES", "PT", "ES", "ES", "ES", "ES",
    };

    private static readonly (string, CategoriaGasto, decimal)[] GastosConceptos =
    {
        ("Material educativo multisensorial", CategoriaGasto.AccionDirecta, 320m),
        ("Sesiones de fisioterapia", CategoriaGasto.AccionDirecta, 480m),
        ("Alimentación mensual del centro", CategoriaGasto.AccionDirecta, 650m),
        ("Logopedia y terapia ocupacional", CategoriaGasto.AccionDirecta, 400m),
        ("Comisiones bancarias", CategoriaGasto.Administracion, 18m),
        ("Gestoría y contabilidad", CategoriaGasto.Administracion, 90m),
        ("Salario educador local", CategoriaGasto.Personal, 540m),
        ("Salario cuidador/a", CategoriaGasto.Personal, 500m),
        ("Electricidad y agua", CategoriaGasto.Suministros, 130m),
        ("Internet y telefonía", CategoriaGasto.Suministros, 45m),
        ("Transporte y logística", CategoriaGasto.Otros, 110m),
        ("Mantenimiento de instalaciones", CategoriaGasto.Otros, 200m),
        ("Juegos y recursos creativos", CategoriaGasto.AccionDirecta, 150m),
        ("Revisión médica anual", CategoriaGasto.AccionDirecta, 380m),
        ("Suministros de limpieza", CategoriaGasto.Suministros, 60m),
    };

    private static readonly (string, string, TipoColaboracionSolicitada, EstadoSolicitud)[] SolicitudesDemo =
    {
        ("Marta", "Nogales", TipoColaboracionSolicitada.Socio, EstadoSolicitud.Pendiente),
        ("Jorge", "Ferrer", TipoColaboracionSolicitada.Donacion, EstadoSolicitud.Pendiente),
        ("Lucía", "Ballester", TipoColaboracionSolicitada.Socio, EstadoSolicitud.Gestionando),
        ("Óscar", "Prieto", TipoColaboracionSolicitada.Microdonacion, EstadoSolicitud.Gestionando),
        ("Beatriz", "Salas", TipoColaboracionSolicitada.Socio, EstadoSolicitud.Aprobada),
        ("Raúl", "Cano", TipoColaboracionSolicitada.Donacion, EstadoSolicitud.Aprobada),
        ("Nuria", "Vidal", TipoColaboracionSolicitada.Socio, EstadoSolicitud.Cancelada),
        ("Sergio", "Pastor", TipoColaboracionSolicitada.Donacion, EstadoSolicitud.Cancelada),
        ("Irene", "Gallego", TipoColaboracionSolicitada.Microdonacion, EstadoSolicitud.Pendiente),
        ("Andrés", "Roldán", TipoColaboracionSolicitada.Socio, EstadoSolicitud.Gestionando),
    };
}
