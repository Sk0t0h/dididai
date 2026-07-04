# Decisions Log — DIDIDAI.ORG

> Registro de decisiones de arquitectura y producto. Conciso, sin ruido.
> Formato: fecha · título · contexto · decisión · alternativas · consecuencias · estado.

---

## 2026-07-03 · Alcance del proyecto: web ONG DIDIDAI (front público + back de gestión), MVP para TFM

- **Contexto:** TFM del Máster de Desarrollo con IA (BIG School), deadline **20/07/2026**. Cliente real: ONG
  local para la que el usuario ya montó una web básica. Se quiere una web nueva taylor-made (no
  WordPress/Joomla) con front público abierto y back de gestión cerrado.
- **Decisión — alcance del producto:** front público + back de gestión con login. Módulos deseados a largo
  plazo: gestión de socios, cuentas/contabilidad de la ONG, informes visuales, gestor de contenido (CMS).
- **Decisión — MVP para el TFM (recorte por plazo):** web pública · autenticación + roles · **gestión de
  socios** (CRUD, alta/baja, listado) · **módulo económico simple** (ingresos/gastos) · **dashboards
  visuales** sobre esos datos.
- **Fuera del MVP → roadmap documentado:** CMS completo y contabilidad avanzada (cuadres, importes
  delicados). Se dejan como evolución futura (bien vistos en slides como "visión de producto").
- **Alternativas:** construir los 4 módulos completos — descartada, inviable y arriesgado en 17 días con
  vídeo/slides/README/deploy incluidos; MVP aún más reducido (sin módulo económico) — descartada, el módulo
  económico simple aporta mucho valor demostrable a bajo coste.
- **Consecuencias:** entrega completa y pulida > ambiciosa e inacabada. El CMS/contabilidad avanzada no se
  empiezan en esta fase.
- **Estado:** acordado. Pendiente detallar entidades y páginas.

## 2026-07-03 · Persistencia (EF Core + SQLite) y despliegue (Azure App Service F1)

- **Contexto:** el TFM exige (recomienda) despliegue con URL pública, y debe ser **sin coste** para la ONG.
- **Decisión:** ORM **EF Core** con proveedor **SQLite** (BD en fichero, no es un servicio facturable →
  coste cero imposible de superar). Despliegue en **Azure App Service F1 (gratuito)** para obtener URL
  pública nativa con el stack .NET.
- **Alternativas:** Azure SQL free — descartada por fragilidad del tier gratuito y riesgo de coste;
  SQL Server en contenedor en F1 — inviable (F1 no soporta contenedores); Postgres + host no-Azure —
  descartada por ser menos nativa con .NET.
- **Consecuencias:** máximo desacople entre la entrega y el presupuesto de la ONG. F1 tiene límites (la app
  se duerme, sin always-on) aceptables para demo de TFM. **Roadmap:** migrar a Azure SQL (cambiando el
  proveedor de EF Core) cuando la ONG asuma el coste de producción.
- **Estado:** acordado.

## 2026-07-04 · Arquitectura: solución multi-proyecto (Web + Core)

- **Contexto:** al empezar la implementación había que decidir la estructura de la solución. Convención
  habitual del usuario en otros proyectos: separar proyecto web + proyecto core (+ console/tests si hacen
  falta). Se descartó de partida el monolito de un solo proyecto.
- **Decisión:** solución `DididaiApp.sln` con dos proyectos por ahora:
  - **`DididaiApp` (web):** solo presentación (Razor Pages + `Program.cs`). Referencia a Core. Las páginas
    inyectan servicios de Core, **nunca** el `DbContext` directo.
  - **`DididaiApp.Core` (class library net10):** dominio + persistencia + lógica. Carpetas `Models/`,
    `Data/` (`AppDbContext`), `Services/`, `Migrations/`. Un **único Core** (sin separar Infrastructure).
  - Futuro, si hacen falta: `tests/DididaiApp.Tests` y/o un console para cargas.
- **Ubicación en el repo:** proyectos como carpetas hermanas en la raíz (sin `src/`); el proyecto web
  **mantiene el nombre `DididaiApp`** (no se renombra a `.Web`) para mínima perturbación del deploy futuro.
- **Migraciones EF:** el `DbContext` vive en Core pero el startup es Web. Los comandos usan
  `--project DididaiApp.Core --startup-project DididaiApp`. Web referencia `EntityFrameworkCore.Design`
  (solo diseño) para que las tools funcionen.
- **Alternativas:** monolito por carpetas — descartada (no es la convención del usuario); Core+Infrastructure
  separados — descartada por ceremonia innecesaria para el plazo (partible después si molesta).
- **Consecuencias:** separación limpia dominio/presentación desde el inicio, defendible en el TFM. Coste: dos
  referencias EF.Design y el patrón `--startup-project` en los comandos ef.
- **Estado:** aplicado. Build OK, migración `InitialCreate` aplicada.

## 2026-07-04 · Modelo de datos: Socio (persona) + Colaboracion (aportación, TPH)

- **Contexto:** modelado sobre el formulario real de `www.dididai.org/form` (Hacerme soci@ / Donación /
  Microdonación). El usuario señaló que un socio puede aportar de varias formas a la vez y que eso crecerá
  (cuota domiciliada, aportación única, Teaming, futuras: Bizum, etc.).
- **Decisión:** **disociar persona de pago** en relación 1:N.
  - **`Socio`** = identidad estable: datos personales + contacto + domicilio + `AceptaPrivacidad` + `FechaAlta`.
  - **`Colaboracion`** (abstracta) = forma de aportar, jerarquía **TPH** (una tabla + discriminador). Base:
    `SocioId`, `Importe` (común, ver abajo), `FechaInicio`, `FechaFin?`, `Activa`. Subtipos:
    `CuotaDomiciliada` (`Modalidad`, `Iban`), `AportacionUnica` (`Fecha`), `Teaming` (sin campos propios).
  - `Importe` se subió a la **clase base**: al declararlo en cada subtipo, EF generaba 3 columnas
    (`Importe`, `CuotaDomiciliada_Importe`, `Teaming_Importe`) — feo, poco mantenible y complicaba el módulo
    económico (sumar ingresos exigiría coalescer 3 columnas). Con `Importe` en la base → 1 sola columna.
  - Los subtipos TPH se registran explícitamente en `OnModelCreating` (`modelBuilder.Entity<...>()`) porque
    la única navegación (`Socio.Colaboraciones`) apunta al tipo base abstracto y EF no los descubre solo.
- **Alternativas:** meter cuota+IBAN dentro de `Socio` — descartada (ata a "1 socio = 1 cuota"); tabla única
  con campos nullable (sin herencia) — descartada, el usuario prefirió TPH por limpieza en C#.
- **Consecuencias:** los **ingresos** del módulo económico saldrán de `Colaboracion`, no de `Socio`; el CRUD
  de socios queda limpio de dinero. RGPD: `Dni`/`Iban` son datos sensibles → nunca subir BD con datos reales
  (`*.db` en `.gitignore`), usar datos anonimizados en demo. No se cifra a nivel de columna (fuera de MVP).
- **Estado:** aplicado. Migración `InitialCreate` regenerada limpia y aplicada; `dididai.db` creada e
  ignorada por git.

## 2026-07-04 · Vulnerabilidad NU1903 (CVE-2025-6965) en SQLite transitivo: documentar y aceptar

- **Contexto:** `EntityFrameworkCore.Sqlite 10.0.9` arrastra transitivamente
  `SQLitePCLRaw.lib.e_sqlite3 2.1.11`, que trae SQLite < 3.50.2 con **CVE-2025-6965** (truncamiento numérico
  / corrupción de memoria, severidad alta). `dotnet restore`/`build` emiten `NU1903`.
- **Análisis:** la explotación **requiere ejecutar SQL arbitrario** (CVSS complejidad alta, condiciones
  previas). Nuestra superficie no ofrece SQL crudo: todo va por EF Core parametrizado. En la práctica, para
  esta app, es un **falso positivo funcional**. Además **no hay versión parcheada** del paquete NuGet a
  fecha 2026-07-04 (issue dotnet/dotnet#7294 cerrado como duplicado; se espera que el bundle suba a SQLite
  >= 3.50.2). No existe un "sube de versión" que lo cierre hoy.
- **Decisión:** **documentar y aceptar el riesgo**, SIN silenciar el aviso (no se añade a `NoWarn`): que
  siga visible en el build como recordatorio hasta que haya parche. Vigilar `SQLitePCLRaw`/EF y actualizar
  en cuanto salga la versión corregida.
- **Alternativas:** forzar un `SQLitePCLRaw` parcheado — inviable (no existe aún); `NoWarn NU1903` —
  descartada (enmascararía futuros NU1903 de otros paquetes); cambiar de motor de BD — desproporcionado,
  rompe stack y plazo.
- **Estado:** aceptado y vigilado. **Revisar antes del despliegue** por si ya hay parche.

## 2026-07-03 · Cuenta de GitHub dedicada por SSH para este repo

- **Contexto:** el usuario usa una cuenta de GitHub distinta para este proyecto y no quiere afectar a la que
  usa en el resto de proyectos.
- **Decisión:** clave SSH dedicada `~/.ssh/id_ed25519_dididai` (con passphrase) + alias de host
  `github-dididai` en `~/.ssh/config` (con `IdentitiesOnly yes`). Remote =
  `github-dididai:Sk0t0h/dididai.git`. Identidad git **local** del repo a
  `Eduardo Fraga <eduardo.fraga.lopez@gmail.com>`; la global del usuario no se toca. `core.sshCommand` local
  → `C:/Windows/System32/OpenSSH/ssh.exe` para que git use el ssh-agent de Windows.
- **Alternativas:** reutilizar `id_ed25519` (ya registrada en la cuenta) — descartada para tener clave
  dedicada al proyecto; HTTPS+PAT — descartada por el problema de credenciales compartidas del Credential
  Manager en Windows.
- **Consecuencias:** el aislamiento es real (otros repos siguen usando `github.com` con su clave). Operar el
  remoto desde PowerShell/VS Code, no Git Bash (usa otro `ssh` y pediría passphrase).
- **Estado:** aplicado y verificado (`ssh -T github-dididai` → "Hi Sk0t0h!"; push OK).

## 2026-07-03 · Licencia MIT a nombre de DIDIDAI

- **Contexto:** repo público (requisito del máster) para una ONG.
- **Decisión:** licencia **MIT**, con el copyright a nombre de **DIDIDAI** (no del desarrollador a título
  personal).
- **Alternativas:** sin licencia (todos los derechos reservados) — descartada por contradecir el "público"
  del máster; Apache 2.0 / GPL — innecesariamente formal / restrictiva para el caso.
- **Consecuencias:** permite reutilización manteniendo aviso de copyright; cláusula "sin garantía" limita
  responsabilidad.
- **Estado:** aplicado.
