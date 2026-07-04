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

## 2026-07-04 · Despliegue en Azure App Service F1 (cuenta personal) — infra creada, entorno resuelto

- **Contexto:** validar el pipeline de despliegue pronto (tarea del Finde 1). Requisito: URL pública, coste
  cero. Se hizo por **Azure CLI** (reproducible, documentable) en vez del portal.
- **Cuenta Azure — decisión:** usar una cuenta **personal dedicada** `dididai@outlook.es` con Azure Free
  Account (200 USD de crédito), no la cuenta del trabajo. La cuenta corporativa (`...@ideidentidadsloutlook`)
  solo daba acceso a suscripciones de empresa (una con gasto real de ~449 €), inapropiadas para un TFM
  personal con repo público. Directorio/tenant propio: `dididaioutlook.onmicrosoft.com`, suscripción
  "Azure subscription 1" (`5c742941-32de-4787-b72b-cf092d13d81d`).
- **Recursos (Azure CLI, todo F1 gratis):** RG `rg-dididai`, plan F1 Linux `plan-dididai`, webapp
  `dididai-web` con runtime `DOTNETCORE:10.0`. **Región `francecentral`**: `westeurope` devolvió
  `RequestDisallowedByAzure - region not accepting new customers` (típico en cuentas nuevas); se probaron
  varias y francecentral aceptó. **App settings** (no en repo): `Seed__AdminEmail`, `Seed__AdminPassword`
  (doble guion bajo = anidamiento .NET) y `ConnectionStrings__DefaultConnection=Data Source=/home/dididai.db`
  (`/home` es almacenamiento **persistente** de App Service → la SQLite sobrevive a reinicios, gratis).
- **Escollo 1 — Norton intercepta TLS:** Norton re-firma los certificados HTTPS con su raíz "Norton Web/Mail
  Shield" → `az` falla con `CERTIFICATE_VERIFY_FAILED` (usa su propio bundle certifi, no el store de Windows).
  Añadir la raíz de Norton al bundle **no funcionó**: el cert de Norton no marca Basic Constraints como
  critical y OpenSSL 3.x lo rechaza. **Solución aplicada:** exclusiones en **Norton → Web segura →
  Exclusiones** para `login.microsoftonline.com`, `management.azure.com`, `graph.microsoft.com` y
  `*.azurewebsites.net` (http y https). Verificado inspeccionando el emisor del cert (pasó de "Norton..." a
  DigiCert/Microsoft). NO se desactivó Norton entero (opción descartada por reducir la protección general).
- **Escollo 2 — cuota F1:** el plan F1 da ~60 min de CPU/día; se agotó con los intentos de arranque/deploy y
  Azure puso la app en `QuotaExceeded` ("Web App stopped", 403), que **ni siquiera acepta el deploy**. Se
  resetea solo en ~24h. **Decisión del usuario: NO subir a plan de pago (B1)** para evitar escalada de coste;
  esperar el reset. F1 es suficiente para una demo de TFM (la app solo consume CPU cuando se usa).
- **Estado:** infraestructura creada y configurada; **deploy final pendiente** de que la cuota se resetee.
  Procedimiento de reanudación detallado en `ai-context.md` y `context/deploy-azure.md`. Herramienta: `az`
  2.87 en `C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd`.
- **F1 vs B1 (revisado 04-07):** aunque B1 (~13 $/mes) cabe de sobra en los 200 $ de crédito, **se mantiene
  F1**. Motivo: B1 NO es "siempre gratis" (tira del crédito, que caduca a los 30 días); si el crédito se
  agota y la cuenta pasa a pago por uso, B1 factura de verdad → riesgo de escalada de coste que F1 no tiene.
  Y la ganancia (sin cuota de CPU, sin dormir) es marginal para una demo de TFM. **Reconsiderar B1 SOLO si**
  F1 resulta inviable para una demo estable o para grabar el vídeo; y si se hace, la pauta segura es
  **subir a B1 → grabar/demostrar → volver a F1**, con alerta de presupuesto en Azure.

## 2026-07-05 · CRUD de socios: baja lógica, DNI único, Email NO único, validación internacional

- **Contexto:** primer módulo funcional del MVP (Finde 2). CRUD de la identidad del socio en `/Admin/Socios`.
  La ONG tiene vínculo internacional (el director trabajó años en Inglaterra) → **la base de socios no es
  solo española**. Eso condiciona validación y unicidad.
- **Baja del socio = borrado LÓGICO** (`Socio.FechaBaja` nullable; `Activo` derivado). El socio no se borra:
  se marca con fecha de baja, desaparece del listado por defecto (toggle "incluir bajas") y se puede
  **reactivar**. Distíngase de la baja de un **método de pago / colaboración** (dejar de pagar una cuota),
  que vive en `Colaboracion.FechaFin`/`Activa` y se gestionará en el bloque de colaboraciones. El usuario
  aclaró que lo habitual será dar de baja pagos, no al socio; la baja del socio cubre el caso "se va del todo"
  (roza el derecho de supresión RGPD, conservando trazabilidad). Sin borrado físico en la UI.
- **DNI único ABSOLUTO** (índice único en BD + validación en servidor; normalizado a mayúsculas/sin espacios
  para que la unicidad no dependa del formato tecleado). Si al dar de alta el DNI ya existe pero está de baja
  → la UI ofrece **reactivar** el existente en vez de crear un duplicado. Alternativa "único solo entre
  activos" (índice filtrado) descartada: permitiría varios registros de la misma persona y ensucia informes.
- **Email NO único** (a propósito): es habitual que varias personas compartan correo (familias, un gestor que
  administra a varios socios). Sigue siendo obligatorio y con formato válido, pero repetible.
- **Validación de formato: UNIVERSAL y laxa, NO nacional.** Decisión deliberada por el carácter internacional
  de la base de socios: **no** se añade regex de DNI/NIE, código postal ni teléfono español, porque
  rechazaría socios legítimos de UK u otros países (postcode `SW1A 1AA`, National Insurance, móvil `+44…`).
  Se mantiene solo lo universal: `[Required]`, longitudes máximas, `[EmailAddress]`, `[Phone]` (permisivo con
  `+`/espacios) y el consentimiento RGPD obligatorio en el alta. El **IBAN** (cuando llegue en colaboraciones)
  sí se podrá validar con el algoritmo **mod-97 internacional** (vale para todos los países, no ata a España).
  Alternativa "validación por país según `Socio.Pais`" descartada para el MVP: sobre-ingeniería a 15 días.
- **Idea abierta (post-MVP, sin decidir) — web bilingüe ES/EN + validación por idioma/país:** el usuario
  apunta que lo más funcional a futuro sería una web en **inglés y castellano** con validaciones acordes al
  idioma/país. **Tensión reconocida:** cualquier validación de formato nacional (aunque sea por idioma)
  **puede dejar fuera a alguien de un tercer país** que no encaje en ninguno de los dos moldes — justo lo que
  hay que evitar en una ONG de alcance internacional. Conclusión provisional: si se hace bilingüe, la
  validación de identificadores debe seguir siendo **permisiva por defecto** (aceptar formato libre) y, como
  mucho, *sugerir* formato según idioma, nunca **bloquear** por no cumplir un patrón nacional. Decisión real
  aplazada a después del MVP.
- **Arquitectura:** capa de servicios en Core (`ISocioService`/`SocioService`); las páginas Razor **no tocan
  el `DbContext`** (invariante del proyecto). Resultados de alta/actualización como enums (`ResultadoAlta`,
  `ResultadoActualizacion`) para que la UI distinga "duplicado activo" de "existe de baja" sin excepciones.
- **Seguridad (revisada con el usuario):** inyección SQL cubierta (EF parametriza todo, incl. el `.Contains`
  del buscador; sin SQL crudo); XSS cubierto por el escape automático de Razor (se guarda el dato tal cual y
  se escapa al mostrar — NO se sanea al entrar, que destruiría datos legítimos); CSRF cubierto (antiforgery
  de Razor Pages en todos los POST). Frontend sin inline (CSP): la confirmación de baja va por JS externo
  (`.js-confirm` + `data-confirm`), no por `onclick`.
- **Estado:** aplicado y **verificado end-to-end en local** (login, alta, listado, DNI normalizado, rechazo
  de DNI duplicado, rechazo sin consentimiento RGPD, edición, baja lógica, reactivación). Migración
  `AddSocioBajaAndDniIndex` aplicada en local. **Sin desplegar aún.** Pendiente/candidato: validación de
  formato por país si se decide más adelante (documentado como fuera de MVP).

## 2026-07-04 (tarde) · Despliegue rehecho: B1 en Spain Central (revierte "mantener F1")

- **Contexto:** al reanudar el deploy con la cuota F1 ya reseteada, el arranque real de la app volvió a
  agotar los 60 min de CPU → `QuotaExceeded` de nuevo. Se destaparon dos hechos que **invalidan la decisión
  previa "mantener F1"** (ver entrada anterior, punto "F1 vs B1"): (1) F1 **se cae al arrancar** la app .NET
  (cada cold start muerde la cuota; riesgo real de 403 en mitad de la defensa); (2) la **corrección del TFM
  puede llegar semanas después** del 20/07, así que la web debe estar en pie y estable **más de un mes** sin
  vigilancia — la pauta "subir a B1, demostrar, volver a F1" ya no sirve porque no se sabe qué día se evalúa.
- **Decisión — plan B1 (de pago) + convertir la suscripción a Pago por uso:** B1 no tiene cuota de CPU ni
  duerme la app. Financiado por los 200 $ (quedan 175 €) de crédito Free Trial mientras dure; el usuario
  **convierte a Pago por uso** (tarea suya en el portal) para que la web no se apague cuando el crédito
  caduque (~agosto 2026). Alerta de presupuesto creada (`presupuesto-dididai`, 30 €/mes, avisos 50%/90% por
  email a `dididai@outlook.es`).
- **Decisión — región Spain Central (`spaincentral`):** se recrea la infra en Spain Central en vez de
  francecentral. Doble motivo: (a) **RGPD/LOPD** — datos personales de socios en territorio nacional,
  minimización de transferencias (argumento defendible en el TFM); (b) francecentral **no tenía capacidad
  B1** en ese momento (`No available instances`), Spain sí. **Francia se deja intacta como respaldo** (no se
  borra); la webapp nueva usa nombre distinto **`dididai-ong`** (URL `https://dididai-ong.azurewebsites.net`)
  para no colisionar con `dididai-web`.
- **Recursos nuevos:** plan `plan-dididai-es` (B1 Linux, spaincentral) + webapp `dididai-ong`
  (`DOTNETCORE:10.0`) en el mismo RG `rg-dididai`. App settings replicados (seed admin + CS a
  `/home/dididai.db`).
- **Bug de arranque corregido (necesario para desplegar en limpio):** `Program.cs` sembraba el admin
  (`SeedAdminAsync`) **sin aplicar migraciones antes**. En local funcionaba porque `dididai.db` ya existía;
  en Azure con `/home` vacío, el seed petaba (sin tablas Identity) → worker no arrancaba → deploy failed.
  **Fix:** `await db.Database.MigrateAsync()` antes del seed (idempotente; usa Migrate, no EnsureCreated,
  para respetar el historial de migraciones). El primer arranque en frío (crear+migrar BD + pull de imagen)
  se pasa del timeout de deploy de 230s y el CLI reporta "failed", **pero Azure reintenta solo** y la app
  levanta en ~50s a la segunda; es esperado, no un fallo real.
- **Azure SQL — reconsiderado y descartado (de nuevo):** se evaluó migrar a una BD SQL servidor. Descartada:
  más coste (~+5 €/mes), y sobre todo más trabajo y riesgo a 16 días de entrega (cambiar proveedor EF,
  regenerar migraciones con tipos estrictos de SQL Server, firewall, más secretos). SQLite cubre el alcance
  del MVP (ONG pequeña, pocos usuarios concurrentes) y su único punto débil (persistencia) está resuelto en
  `/home`. Azure SQL queda en **roadmap**.
- **Alternativas:** esperar y reintentar B1 en francecentral — descartada (sin capacidad + sin ventaja RGPD);
  Azure Container Apps free tier — descartada (contenerizar = medio día de trabajo + SQLite sobre disco de
  contenedor es MÁS frágil, no menos); seguir en F1 — descartada (se cae al arrancar; inaceptable para una
  evaluación de duración desconocida).
- **Consecuencias:** web pública **estable** en `https://dididai-ong.azurewebsites.net` (Spain, B1),
  verificada end-to-end (home 200, `/Admin` anónimo 302, login admin 302, `/Admin` autenticado 200). Coste:
  cubierto por crédito ahora; ~13 €/mes de bolsillo solo si la corrección se alarga tras agotarse/caducar el
  crédito. La `dididai.db` de `/home` **no se borra** en cada deploy (persiste); si se corrompe, borrarla y
  reiniciar la recrea por la migración de arranque.
- **Estado:** aplicado y verificado (04-07 tarde). Pendiente del usuario: **convertir la suscripción a Pago
  por uso** en el portal antes de que caduque el crédito (~30 días).

## 2026-07-04 · Autenticación: ASP.NET Core Identity (Default UI) sobre AppDbContext

- **Contexto:** el back de gestión es cerrado, pero **el front público debe seguir abierto sin login**
  (fundamental). Se pidió: login al back, recuperación de contraseña sí, **registro público NO** (altas
  manuales o desde dentro del back), y 2FA "no estaría de más".
- **Decisión:**
  - **Identity compartiendo `AppDbContext`** (hereda de `IdentityDbContext<IdentityUser>`): una sola BD y
    migración (`AddIdentity`). Usuario `IdentityUser` estándar (email como login). `RequireConfirmedAccount
    = false` (altas manuales, sin flujo de confirmación de email). Password mínimo 8.
  - **Default UI de Identity** (`AddIdentity<,>() ... .AddDefaultUI()`): páginas embebidas (login, logout,
    forgot/reset password, manage) sin scaffolding. Se eligió `AddIdentity` (no `AddDefaultIdentity`) para
    tener **roles** (`AddRoles`/`IdentityRole`).
  - **Registro público deshabilitado por middleware:** toda ruta `/Identity/Account/Register*` devuelve 404
    salvo que la pida un usuario con rol `Admin`. Se descartó `AuthorizeAreaPage` por convención: **no
    alcanza de forma fiable las páginas compiladas en el ensamblado de la Default UI** (verificado: un
    anónimo llegaba a registrarse). El middleware va tras `UseAuthentication`.
  - **Rutas de cookie** (`ConfigureApplicationCookie`) apuntadas al área Identity
    (`/Identity/Account/Login|Logout|AccessDenied`), porque por defecto Identity apunta a `/Account/Login`
    (404 con Default UI).
  - **Seed** (`DbSeeder` en Core): crea rol `Admin` y un usuario admin si hay credenciales en configuración.
    **Credenciales NUNCA en el repo** (público) → `Seed:AdminEmail` / `Seed:AdminPassword` en **User Secrets**
    (dev) / variables de entorno (prod). Si faltan, se omite el seed con warning.
  - **Recuperación de contraseña:** funciona el flujo completo con un `IEmailSender` **stub**
    (`LoggingEmailSender`, en Web) que loguea el enlace en vez de enviarlo. Proveedor real (SendGrid/SMTP)
    pendiente para el deploy.
  - **2FA:** el andamiaje de Identity lo soporta; **no se fuerza ahora** (candidato a recortar por plazo).
  - **Zona protegida:** `/Admin` (Razor Page) con `[Authorize(Roles="Admin")]`; enlace de acceso/salir en el
    layout vía `_LoginPartial` (sin inline, respetando CSP).
- **Verificado end-to-end (HTTP):** front público 200 sin login · `/Admin` anónimo → 302 a login · registro
  anónimo (GET/POST) → 404 · login admin (seed) → 302 · admin → `/Admin` 200 · admin → registro 200.
- **Alternativas:** contexto Identity separado — descartada (ceremonia); `AddDefaultIdentity` — descartada
  (no da roles); bloquear registro por convención de página — descartada (no fiable con Default UI).
- **Estado:** aplicado y verificado. Pendiente: proveedor de email real, y scaffolding de 2FA si sobra tiempo.

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
