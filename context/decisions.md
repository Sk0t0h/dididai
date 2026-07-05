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

## 2026-07-05 · Web multi-idioma: i18n solo en front público, extensible a N idiomas; validación por país (no por idioma)

- **Contexto:** hace tiempo se pidió una versión en inglés de la web que quedó a medias. Se retoma ahora,
  antes del CRUD de Colaboraciones (que trae IBAN/teléfono), por si condiciona la estructura de datos y
  validación.
- **Decisión — separar tres ejes independientes:** (1) **cultura de la UI** (idioma que se ve, lo elige el
  visitante); (2) **país del socio** (`Socio.PaisCodigo`, dato de negocio); (3) **validación de datos**
  (DNI/teléfono/IBAN), que se condiciona **al país del socio, NUNCA al idioma de la web**. Un español puede
  navegar en inglés: la validación no debe cambiar por eso.
- **Decisión — alcance i18n:** solo **front público** bilingüe ES/EN; el back `/Admin` queda en español. La
  infra se monta **ahora** (antes de crear más páginas), la traducción de contenido real se hace cuando el
  MVP core esté cerrado.
- **Decisión — infra N-idiomas:** `AddViewLocalization` + `.resx` + `RequestLocalizationMiddleware` con
  **cookie provider** (selector manual en la cabecera, persiste en cookie), `es` por defecto. Ampliar idioma
  = añadir la cultura a la lista de `Program.cs` + su `.resx`; nada de `if (idioma == "en")` en el código.
- **Decisión — captura de país:** `Socio.Pais` (texto libre hoy) pasará a **código ISO 3166-1 alpha-2**
  (`ES`, `GB`…) como única fuente de verdad; desplegable con buscador, España por defecto. (Pendiente Frente 1.)
- **Alternativas descartadas:** validación por idioma de la web (frágil: descuadra a un español navegando en
  inglés); i18n también del back (más trabajo, sin valor para el TFM); idioma por URL `/es//en/` (obliga a
  reestructurar el routing, más de lo que conviene ahora); país como texto libre (impide validar por país de
  forma fiable).
- **Consecuencias:** mantiene la base de socios internacional sana (decisión previa "validación universal").
  El idioma es puramente cosmético. Selector CSP-safe (JS externo, sin inline). POST del selector con
  antiforgery y sin open-redirect.
- **Estado:** **Frente 2 (infra i18n) IMPLEMENTADO y verificado end-to-end (05-07)**: default ES, cookie EN
  conmuta textos y `lang`, `/Admin` no se ve afectado, CSRF protegido. SIN desplegar.

## 2026-07-05 · Frente 1: país=residencia (ISO), validación por TIPO de documento, teléfono E.164, cliente=servidor

- **Contexto:** al implementar la validación por país surgió el caso "español residente en UK (y viceversa)".
  Si `PaisCodigo` significaba a la vez nacionalidad y residencia, era ambiguo: a un español en UK no se le
  validaría la letra del DNI. Además se decidió cerrar la deuda de validación cliente/servidor divergente
  ("la UX debe ser central").
- **Decisión — tres datos separados, cada uno con su rol:**
  1. **`Socio.PaisResidencia`** (renombra `PaisCodigo`): ISO 3166-1 alpha-2; es el **domicilio**, NO decide
     la validación del documento.
  2. **`Socio.TipoDocumento`** (nuevo enum: `DniEspanol`/`Nie`/`Pasaporte`/`Otro`): es lo que **dispara** la
     validación del documento. Así un español en el extranjero declara "DNI español" y se le valida la letra.
  3. **`Socio.Dni`** (el valor): validado según `TipoDocumento` (DNI/NIE → letra de control; pasaporte/otro →
     laxo, solo presencia). Sigue siendo la clave única del socio.
- **Decisión — teléfono E.164 universal**, con UI de **prefijo (select) + número**: `Telefono` sigue siendo un
  único campo (E.164) en la entidad; la UI lo parte solo en pantalla y lo recompone (JS externo). El prefijo se
  preselecciona por residencia (comodidad), pero es independiente (móvil español viviendo fuera).
- **Decisión — validación cliente = servidor sin duplicar regla:** atributos custom `IClientModelValidator`
  (`[TelefonoE164]`, `[DocumentoPorTipo]`) que validan en servidor y emiten los `data-val-*`; adaptadores
  jquery-validation en `validacion-socio.js` (CSP-safe) que aplican la MISMA regla en vivo y **revalidan el
  documento al cambiar el tipo**. La lógica de servidor vive en `ValidacionIdentidad`; el cliente la reproduce
  (verificado con test de paridad en Node).
- **Decisión — catálogos en código, no en BD:** `Paises` (ISO, vía `RegionInfo`) y `PrefijosTelefonicos`
  (código de llamada, lista curada). Datos estándar y estables; la validez del código se garantiza con el
  desplegable + validación en servidor, no con FK. Promocionar a tabla si el negocio lo pidiera es no
  destructivo.
- **Decisión — layering:** los atributos `IClientModelValidator` obligan a `FrameworkReference
  Microsoft.AspNetCore.App` en Core. Aceptado: Core ya dependía de ASP.NET Core vía Identity, así que no
  introduce una dependencia de capa nueva.
- **Migración:** se descartó la intermedia `SocioPaisIso` (no desplegada) y se generó una única limpia
  `SocioResidenciaYTipoDocumento` (drop `Pais` + add `PaisResidencia` + `TipoDocumento`). Datos antiguos de
  texto libre no mapeables → drop+add deliberado; prod vacía, local desechable.
- **Alternativas descartadas:** un solo `PaisCodigo` con doble significado (ambiguo, el caso ES-en-UK lo
  rompía); validar el documento por país de residencia (mismo problema); tabla de países/prefijos en BD
  (sobre-ingeniería para dato estándar); teléfono en un solo input libre (peor UX y ambigüedad de prefijo);
  validación solo de servidor (deuda de UX, descartada por decisión explícita del usuario).
- **Estado:** **IMPLEMENTADO y verificado (05-07).** Servidor: 8 casos (incl. español-en-UK aceptado, DNI/NIE
  inválidos y residencia inexistente rechazados con mensaje). Cliente: paridad de lógica + glue del teléfono
  (compón/descompón/preselección) probados en Node contra el fichero real. Build OK. **SIN commitear/desplegar
  cuando se escribió esto** (commit inmediatamente después). Verificación en navegador con Playwright no fue
  posible por bloqueo de red del entorno (Norton/TLS), documentado.

## 2026-07-05 · CRUD de Colaboraciones (TDD en la lógica, IBAN mod-97, baja lógica)

- **Contexto:** segundo módulo del MVP. Un socio tiene N colaboraciones (jerarquía TPH ya existente:
  CuotaDomiciliada / AportacionUnica / Teaming). Es donde "se dan de baja los pagos".
- **Decisión — IBAN por TDD:** `ValidacionIban.EsValido` (mod-97 ISO 13616, internacional) escrito test-first
  (17 tests en rojo→verde). Atributo `[Iban]` (`IClientModelValidator`) que delega en él, mismo patrón que
  DNI/teléfono (cliente=servidor). Longitudes por país en un mapa (ampliable); país no registrado se rechaza.
- **Decisión — capa de servicios:** `IColaboracionService`/`ColaboracionService` (patrón de socios: la página
  no toca el DbContext). Reglas: socio debe existir, importe > 0, IBAN válido solo si es cuota domiciliada.
  **Baja lógica** (`Activa=false` + `FechaFin`), idempotente: es "dejar de pagar" conservando histórico, NO un
  borrado. Cubierto con **tests de integración** (SQLite en memoria, 7 tests) — según lo acordado: TDD/tests en
  lógica y servicios, pragmático en páginas.
- **Decisión — UI:** gestión **desde la ficha del socio** (Details lista sus colaboraciones + alta + baja por
  fila). Alta en **un solo formulario con selector de tipo**; los campos de cuota (IBAN, periodicidad) se
  muestran/ocultan por JS externo (CSP) y solo se validan si el tipo es cuota domiciliada. La página usa un
  ViewModel plano y construye el subtipo TPH en el POST (no se puede bindear al tipo base abstracto).
  **Vista global de colaboraciones pospuesta al módulo económico** (es donde aporta: agregación de ingresos);
  no duplicar ahora.
- **Alternativas descartadas:** IBAN solo español (contradice base internacional); bindeo directo a
  `Colaboracion` abstracta (no funciona con TPH); una página de alta por tipo (más páginas, peor UX); vista
  global ya (adelanta trabajo del módulo económico).
- **Estado:** **IMPLEMENTADO y verificado en local (05-07).** 79 tests verdes (incl. 7 de integración del
  servicio). E2E por HTTP: alta de los 3 tipos, IBAN inválido/ausente e importe cero rechazados con mensaje,
  baja lógica efectiva. Sin migración (solo atributos de validación; el esquema TPH ya existía). **SIN
  desplegar** al cerrar la nota.

## 2026-07-05 · Módulo económico: ingresos (desde colaboraciones) + gastos + balance, cálculo por TDD

- **Contexto:** tercer módulo del MVP ("económico simple, ingresos/gastos"). Los ingresos ya existían como
  datos (colaboraciones); los gastos no estaban modelados.
- **Decisión — entidad `Gasto`:** concepto, importe, fecha, `CategoriaGasto` (enum genérico de ONG: acción
  directa / administración / personal / suministros / otros — permite mostrar cuánto va a acción directa,
  refuerza el "99%"). Migración `AddGasto` (tabla nueva). **Borrado físico** (no baja lógica como socios/
  colaboraciones): un gasto no tiene ciclo de vida que trazar; mal metido se corrige quitándolo.
- **Decisión — cálculo por TDD:** `IResumenEconomicoService`/`ResumenEconomicoService` con 4 métricas +
  balance, todo test-first (6 tests, SQLite en memoria):
  1. **Ingreso recurrente mensual** = solo cuotas domiciliadas activas, anual/12 (Teaming y aportación única
     NO son recurrentes).
  2. **Ingresos por tipo** (solo activas).
  3. **Socios activos con colaboración activa** (socio de baja no cuenta aunque tenga colaboración activa).
  4. **Altas por mes** (serie temporal, orden cronológico).
  - `Balance = TotalIngresos − TotalGastos`. La agrupación por mes se hace en memoria (SQLite no la resuelve
    bien en el servidor; volumen de una ONG pequeña lo permite).
- **Decisión — UI `/Admin/Economia`:** métricas en cards + ingresos por tipo + gestión de gastos (alta inline
  + borrado) + **vista global de colaboraciones** (la que se pospuso al crear Colaboraciones). Card de acceso
  en el panel `/Admin`. Los servicios hacen el cálculo; la página no toca el DbContext. Mobile-first.
- **Alternativas descartadas:** gastos con baja lógica (innecesario, no hay que trazarlos); incluir Teaming en
  el recurrente (no tiene periodicidad modelada); agrupar por mes en SQL (frágil en SQLite).
- **Fuera de aquí:** los **dashboards con gráficas** (la página económica da los números; las gráficas se
  enganchan después, eligiendo librería CSP-compatible).
- **Estado:** **IMPLEMENTADO y verificado en local (05-07).** 85 tests verdes. E2E por HTTP: métricas
  correctas en la página real (recurrente 20€ con anual/12, ingresos 630€, balance 430€ con gasto 200€, 1
  socio con colaboración). **SIN desplegar** al cerrar la nota.

## 2026-07-05 · Dashboards con Chart.js servido en local (CSP-safe)

- **Contexto:** cuarto módulo del MVP (informes visuales). La página `/Admin/Economia` ya calculaba los
  números; faltaban las gráficas. Restricción dura: la disciplina CSP del proyecto (sin inline, sin CDN).
- **Decisión — librería:** **Chart.js 4.4.3 servido en local** (`wwwroot/lib/chartjs/chart.umd.min.js`, ~200
  KB), NO desde CDN (rompería CSP). El arranque de las gráficas va en **`dashboard.js` externo** que lee los
  datos de atributos `data-chart` de cada `<canvas>` (JSON serializado en servidor); no hay `<script>` inline
  con datos incrustados → CSP-safe. Nota: hoy el proyecto **no** emite aún la cabecera CSP (es disciplina de
  código para no romper cuando se añada); el enfoque es a prueba de futuro.
- **Decisión — gráficas (4):** ingresos por tipo (donut), ingresos vs gastos vs balance (barras), gastos por
  categoría (barras) y altas por mes (líneas). Los datos salen de `ResumenEconomicoService`.
- **Decisión — nueva agregación por TDD:** `GastosPorCategoria` añadido al resumen (test-first). El resto de
  datos ya existían.
- **Alternativas descartadas:** Chart.js/D3 desde CDN (rompe CSP); SVG propio (más código por tipo de gráfica,
  menos pulido, sin tooltips) — Chart.js local da más por menos.
- **Estado:** **IMPLEMENTADO y verificado en local (05-07)** salvo el render visual (Playwright bloqueado por
  el entorno). Verificado sin navegador: 4 canvas, Chart.js servido 200, `dashboard.js` sin errores de
  sintaxis, `data-chart` = JSON válido y parseable con los valores correctos. 86 tests verdes. **Pendiente:
  validación visual por el usuario en el navegador + desplegar.**

## 2026-07-05 · Fix del campo país: combo input+datalist (el select con buscador suelto fallaba)

- **Contexto:** el usuario reportó que no podía dar de alta un socio: el campo país mostraba "must be a string
  with min/max length of 2" con España seleccionada, y además el buscador suelto encima del select era mala
  UX. El patrón anterior (input `data-pais-buscador` que ocultaba `<option>` de un `<select>`) era frágil.
- **Decisión:** sustituir por un **combo `<input list>` + `<datalist>`** con buscador nativo integrado (sin
  caja separada). El usuario ve/teclea el **nombre**; un manejador externo (`site.js`, `data-pais-combo`)
  resuelve el **código ISO** y lo escribe en un **campo oculto** `PaisResidencia` (`data-pais-codigo`), que es
  el que se bindea y valida. En edición se precarga el nombre desde el código (`Paises.Nombre`). Si el texto
  no corresponde a ningún país, el código queda vacío → el servidor lo rechaza con mensaje claro
  ("Selecciona un país de residencia de la lista.", ya no el críptico de longitud).
- **Ajuste asociado:** el preselect de prefijo telefónico por país (en `validacion-socio.js`) pasa a leer el
  código del campo oculto y a escuchar el `input` del combo (antes escuchaba el `change` del select).
- **Alternativas descartadas:** select nativo simple sin buscador (funcionaría pero el usuario quería buscador
  integrado); arreglar el patrón input+select ocultando options (mantiene la fragilidad).
- **Estado:** **corregido y verificado por HTTP (05-07):** alta con España → 302 (antes fallaba); país vacío
  → rechazado con mensaje claro. 86 tests verdes. **Pendiente validación visual del usuario** (filtrado del
  combo al teclear) + desplegar.

## 2026-07-05 · Fix: los <select> de enum comparaban por nombre pero emiten valor numérico

- **Contexto:** el usuario reportó dos síntomas: (a) en el alta de colaboración **nunca aparecían** los campos
  de la cuota domiciliada (IBAN, periodicidad); (b) la validación de DNI **solo saltaba al enviar**, no en vivo.
- **Causa raíz común:** `Html.GetEnumSelectList<T>()` genera `<option>` con `value` = **valor numérico** del
  enum (`0`,`1`,`2`), pero el JS comparaba `select.value === "CuotaDomiciliada"` / `=== "DniEspanol"` (el
  NOMBRE). La comparación era siempre falsa → los campos de cuota nunca se mostraban y el validador de DNI
  nunca detectaba el tipo. El servidor no se veía afectado (bindea el enum correctamente).
- **Decisión:** no acoplar el JS al nombre ni al número mágico. Los `<select>` exponen el/los valor(es)
  relevante(s) en atributos `data-*` generados en Razor con el cast del enum:
  `data-colab-cuota-val="@((int)TipoColaboracion.CuotaDomiciliada)"` y
  `data-tipo-dni`/`data-tipo-nie` en el select de documento. El JS compara `select.value` contra esos
  atributos. Si mañana cambia el orden del enum o se pasa a value=nombre, sigue funcionando.
- **Extras del mismo fix:** `[Display]` en el enum `TipoColaboracion` para que el desplegable muestre "Cuota
  domiciliada" (antes salía pegado); `Entrada.Importe` pasa a `decimal?` (antes renderizaba `value="0"`, que
  fallaba el `[Range]` de entrada) con `[Required]`.
- **Estado:** corregido y verificado (05-07): lógica de mostrar/ocultar de cuota y de validación DNI probadas
  en Node contra los ficheros JS reales con valores numéricos; alta por HTTP OK; 86 tests verdes. **Pendiente
  validación visual del usuario** + desplegar.
