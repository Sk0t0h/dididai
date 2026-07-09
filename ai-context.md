# AI Context — Estado actual de trabajo

> Memoria de trabajo **volátil**: el "ahora" del proyecto (foco, próximos pasos inmediatos). Se
> **sobreescribe** en cada cierre de bloque, no crece. Para la crónica histórica → `logs/`. Para el tablero
> estratégico estable → `ORACULO.md`. Para las acciones detalladas → `context/next-steps.md`.
> Actualizado: 2026-07-09 (rediseño de solicitudes ya PUSHEADO; pendiente deploy + Identity).

## FOCO ACTUAL (09-07) — Rediseño PUSHEADO; falta DEPLOY + pulir Identity

**Rediseño del flujo de solicitudes TERMINADO** (4 bloques A-C2), validado visualmente por el usuario y
commiteado en local. Máquina de estados Pendiente(gris)→Gestionando(amarillo)→Aprobada(verde)/Cancelada(rojo);
log de acciones de gestión (`AccionSolicitud`, usuario del admin no editable, 1ª acción→Gestionando); matching
por email/teléfono como sugerencia (sin unicidad) + vincular a socio existente; alta de socio nuevo desde
solicitud (precarga + privacidad + vínculo); crear la colaboración desde la solicitud (`ColaboracionId`, no
duplica; microdonación→Teaming no genera); direcciones del socio opcionales. **Disociación clave:** solicitud
(intención revocable) ≠ socio (identidad) ≠ colaboración (aportación real, lo que cuenta en el económico);
aprobar ≠ crear colaboración (el IBAN solo entra al crear la colaboración). Además, **acceso a gestión desde el
front arreglado** (menú con sesión) y **cabecera del back rediseñada** para replicar el front (logo+crema).

**Estado de commits (rama `main`): TODO PUSHEADO a `origin/main`.** HEAD local = `origin/main` = `58ac972`
(verificado 09-07). Último commit: `58ac972` — acceso a gestión unificado front/back + cierre del día.

**DEPLOY HECHO Y VERIFICADO (09-07).** Desplegado `58ac972` a Azure (`dididai-ong`, B1/Spain):
`RuntimeSuccessful`, `state=Running`, home 200 (front nuevo servido: `front.*.css`/`front.*.js`, título propio),
/Admin sin login 302, CSP presente. Las 3 migraciones nuevas se aplicaron al arrancar (la app levantó). 125
tests verdes antes de publicar. **Todo el front público + solicitudes + rediseño ya está VIVO en producción.**
- El **riesgo del enum quedó DESCARTADO** (el módulo de solicitudes nunca se había desplegado → en prod no
  había filas con el valor viejo). Migraciones aditivas, sin data-fix ni reset. Confirmado por el arranque OK.

**RETOMAR (09-07):** pulir las páginas de **Identity** (login, gestión de cuenta): traducir a ES y quitar
lo que no aplica (registrarse, proveedores externos, confirmar email). Ya usan el `_Layout` del back vía
`Areas/Identity/Pages/_ViewStart`, pero el contenido interno es la Default UI → requiere **scaffold** (zona
sensible: auth). Ver `context/next-steps.md`.

**Verificado por HTTP** todo el rediseño (estados, acciones, matching, vinculación, alta desde solicitud, crear
colaboración por tipo, no duplicar). La validación VISUAL la hace el usuario (Playwright bloqueado por el entorno).

---


## Estado actual

**Finde 1 CERRADO** (esqueleto vivo y desplegado) + **CRUD de socios hecho** + **infra i18n del front público
montada** (idioma ES/EN conmutable, extensible a N idiomas).

**05-07 (tarde): infra i18n del front público (COMMITEADA, `a4dfa8d`).** Localización estándar de ASP.NET Core
(`AddViewLocalization` + `.resx` + `RequestLocalizationMiddleware`, cookie provider, `es` por defecto).
Selector de idioma en cabecera (JS externo, CSP-safe). Página `SetLanguage` (POST con antiforgery, sin
open-redirect). **Solo front público**; `/Admin` en español. Ampliar idioma = una línea en `Program.cs` + su
`.resx`. Verificado end-to-end.

**05-07 (tarde/noche): Frente 1 — país=residencia + validación por tipo de documento + teléfono E.164 +
paridad cliente/servidor (SIN commitear cuando se escribe esto).** Tres datos separados: `PaisResidencia`
(ISO, domicilio, NO valida), `TipoDocumento` (enum DNI/NIE/Pasaporte/Otro, **dispara** la validación del
documento) y `Dni` (valor, validado según tipo: DNI/NIE→letra, resto→laxo). **Caso resuelto: español
residente en UK** declara "DNI español" y se le valida la letra aunque resida fuera. Teléfono E.164 con UI
prefijo+número (un solo campo en la entidad, la UI lo parte/recompone por JS). **Validación cliente=servidor**
sin duplicar regla: atributos `IClientModelValidator` (`[TelefonoE164]`, `[DocumentoPorTipo]`) + adaptadores
jquery-validation (`validacion-socio.js`, CSP-safe) que revalidan al cambiar el tipo. Catálogos `Paises` y
`PrefijosTelefonicos` en código (no BD). Migración única `SocioResidenciaYTipoDocumento`. **Verificado**:
servidor 8 casos + cliente (paridad + glue teléfono) en Node. Racional en `context/decisions.md` (entrada
05-07 "Frente 1"). **SIN DESPLEGAR.**

Finde 1 (04-07): arquitectura multi-proyecto, capa de datos (EF Core 10 + SQLite, `Socio` 1:N `Colaboracion`
TPH), autenticación (Identity, roles, verificada), README, y **despliegue en producción**. La web está viva:
**https://dididai-ong.azurewebsites.net** (Azure App Service **B1**, Spain Central). Suscripción convertida a
Pago por uso (no caduca).

**05-07 (madrugada): CRUD de gestión de socios** en `/Admin/Socios` — capa de servicios en Core
(`ISocioService`/`SocioService`; las páginas no tocan el `DbContext`), páginas Razor (listado con búsqueda,
ficha, alta, edición), **baja lógica + reactivación**, **DNI único** (normalizado), **Email NO único**,
consentimiento RGPD obligatorio, **validación universal (no España-only)** por el carácter internacional de
la base de socios. Migración `AddSocioBajaAndDniIndex`. **Verificado end-to-end en local** (incl. rechazo de
DNI duplicado y de alta sin consentimiento). Commiteado. **AÚN SIN DESPLEGAR.** Racional en
`context/decisions.md` (entrada 05-07).

Ritmo real: se trabaja sobre todo en **findes** y en **ratos cortos entre semana**. Trabajo pesado a findes;
ratos de diario solo tareas pequeñas y sin riesgo.

## Cómo se cerró el deploy (resumen; detalle en `context/decisions.md` y `deploy-azure.md`)

- Al reanudar en F1, la app **volvió a caer en `QuotaExceeded` al arrancar**. Con eso + que la corrección del
  TFM puede tardar >1 mes, se **revirtió la decisión "mantener F1"**: se pasó a **B1** (sin cuota de CPU, no
  se duerme), financiado por el crédito Free Trial.
- Se **recreó la infra en `spaincentral`** (RGPD: datos en territorio nacional; y francecentral no tenía
  capacidad B1). **Francia (`dididai-web`/F1) se conserva intacta como respaldo**; la webapp nueva es
  `dididai-ong` con plan `plan-dididai-es`.
- **Bug de arranque corregido:** `Program.cs` sembraba el admin sin aplicar migraciones → en Azure (`/home`
  vacío) petaba. Fix: `await db.Database.MigrateAsync()` antes del seed.
- Alerta de presupuesto creada: `presupuesto-dididai`, 30 €/mes, avisos 50%/90% por email.

## RETOMAR AQUÍ

**El usuario validó el núcleo en producción (05-07): 5 gráficas + editar colaboración OK.** Reportó dos
mejoras, ya HECHAS y commiteadas (SIN desplegar aún):

1. **Fix doble-click al guardar IBAN.** El IBAN solo validaba en servidor → el error persistía hasta una
   interacción, "gastando" el primer submit. Corregido con validación de cliente real: atributo `[Iban]`
   (`IClientModelValidator`, ya existía) en los ViewModels de Create/Edit de colaboración + adaptador
   jquery-validation `iban` en `colaboracion-form.js` (mod-97 replicado 1:1 del servidor, verificado 10 casos
   en Node); script cargado también en Edit.
2. **Gestión de colaboraciones desde la edición del socio.** Tabla (ver/crear/editar/baja) extraída a partial
   `_TablaColaboraciones.cshtml` (+ modelo), usada en Details (sin duplicar) y en Edit. `Edit.cshtml.cs` carga
   colaboraciones y tiene handler de baja (redirige a Edit). Baja lógica, sin borrado físico.

Verificado por HTTP (sesión admin): `data-val-iban` en Create y Edit, tabla en Edit del socio con acciones,
POST de baja desde Edit OK (302→Finalizada). Doble-click confirmado visualmente por el usuario. **93 tests
verdes, build OK.** Playwright sigue bloqueado por el entorno (ERR_BLOCKED_BY_CLIENT en localhost, es el
navegador headless del entorno, NO Norton — descartado probando ambos puertos). **DESPLEGADO a producción**
(05-07, `RuntimeSuccessful`; verificado por HTTP: `data-val-iban` + `colaboracion-form.js` en el form de
colaboración, tabla de colaboraciones en Edit del socio, home 200, /Admin 302). Commit `b95e146`.

**05-07 (noche) — CSP estricta activada.** Se cerró el hueco: había disciplina anti-inline pero NO cabecera
CSP. Middleware `SecurityHeadersMiddleware` emite `default-src 'self'` sin `unsafe-inline` (+ X-Content-Type,
Referrer-Policy, X-Frame-Options) en cada respuesta; registrado tras `UseHttpsRedirection`. Quitado el
`<script type="importmap">` vacío del layout. Auditado: 0 inline / 0 CDN en las vistas propias. Verificado en
local (cabecera en home/login/back; login+Socios+Economia 200; escaneo de 5 páginas = 0 inline). Esto fija las
reglas ANTES de escribir el front, que irá CSP-safe.

## FRONT PÚBLICO — IMPLEMENTADO (07-07), SIN COMMITEAR / SIN DESPLEGAR

**07-07: front público montado desde la exportación de Claude Design.** Último módulo del MVP. El prototipo
(formato `x-dc`, inline) se recreó como **Razor + CSS/JS externos, CSP-safe** (0 inline verificado por HTTP).
`Index.cshtml` reemplaza la home de plantilla; layout público propio `_PublicLayout.cshtml` (el back sigue con
Bootstrap `_Layout`). **103 tests verdes, build OK, verificado E2E con la app corriendo.**

Qué se hizo (detalle fino en el commit cuando se haga; racional en el log W27 07-07):
- **Landing** one-page: header sticky (menú móvil + ES/EN + CTA), hero (foto + 99% gigante + tesis), Actividad,
  Filosofía, franja Transparencia (stats con count-up), 7 Objetivos, formulario Colaborar, Contacto, footer.
  Paleta `#f7941d` naranja al mando + `#241a12` + crema. Fuentes **Fraunces+Poppins autoalojadas** en
  `wwwroot/fonts/` (el prototipo las cargaba de Google Fonts → violaba la CSP). Imágenes en `wwwroot/images/front/`.
- **Formulario con campos por tipo** (arreglada la queja de que estaba "cojo"): Socio→periodicidad;
  Donación→importe puntual; Microdonación→1 €/mes fijo. Mostrar/ocultar por `front.js` (CSP-safe).
- **Flujo pago = solicitud → revisa el admin** (Stripe/SEPA 0,8 % = roadmap, NO MVP). Entidad
  **`SolicitudColaboracion`** + migración aditiva `AddSolicitudColaboracion` + `ISolicitudColaboracionService`
  (10 tests). **IBAN NUNCA en el form público.** `Program.cs` registra servicio + rate limiter.
- **Admin `/Admin/Solicitudes`** (listado con filtro + ficha aprobar/rechazar con nota) + badge de pendientes en
  el panel. Aprobar → enlace al alta de Socio con datos precargados (`Socios/Create.OnGet` acepta query params).
- **Defensas OWASP:** antiforgery + honeypot + **rate-limit por IP solo en POST** (5/5 min; los GET no se
  limitan) + validación server + mensaje neutro.
- **i18n:** contenido **ES** en `Index.resx`; **EN pendiente** (cae a ES por fallback). Infra i18n ya existía.

**07-07 (noche) — 1ª revisión visual del usuario: 6 fixes aplicados** (build OK, 103 verdes, SIN
commitear/desplegar). Detalle en log W27. Resumen: (1) textos HTML salían como el nombre del tipo
`LocalizedHtmlString` por usar `@Html.Raw(Localizer[..])` → `@Localizer[..]` directo; **esto causaba el
overflow horizontal** (era el grueso del "responsive roto"). (2) placeholder email mangleado → a resx. (3)
validación cliente no enganchaba: `_PublicLayout` no cargaba jQuery → añadido. (4) tipo sin marcar: `asp-for`
emitía el nombre del enum, `front.js` compara el número → emitir número + marcar `.sel` server-side. (5)(6)
formulario y header ajustados en móvil. **Falta re-verificar visualmente (lo hace el usuario).**

**HANDICAP a resolver (prioridad próxima sesión):** el entorno bloquea el navegador headless (Playwright
`ERR_BLOCKED_BY_CLIENT` en localhost, ambos puertos/IPs) → no se puede verificar el render visual desde aquí,
solo por HTTP/estructura. El front visual se está desarrollando a ciegas. Investigar alternativa (config de
red del sandbox, otro runner, o verificación asistida por el usuario con screenshots).

**PENDIENTE inmediato:** re-verificar los fixes en navegador (usuario); commitear + desplegar; traducir EN;
borrar la solicitud de prueba de la BD local.
**AVISO seguridad (vigente):** el IBAN real de la ONG está en la web vieja; **NO copiarlo** al repo público.
Orfanato = "BalMandir", Katmandú. Contenido literal versionado en `context/contenido-front.md`.

---

**Socios (i18n + Frente 1) y CRUD de Colaboraciones DESPLEGADOS y vivos en producción** (05-07, verificados
por HTTP: alta de cuota domiciliada + ficha muestra la colaboración con IBAN). Tests: `DididaiApp.Tests` (xUnit), **93 verdes**
(ValidacionIdentidad, Paises, PrefijosTelefonicos, ValidacionIban por TDD, ColaboracionService/ResumenEconomico integración).

**05-07 — CRUD de Colaboraciones.** IBAN validado por **TDD** (`ValidacionIban`, mod-97 ISO 13616,
internacional) + atributo `[Iban]` (`IClientModelValidator`). `IColaboracionService`/`ColaboracionService`
(reglas: socio existe, importe>0, IBAN válido solo si cuota domiciliada; **baja lógica** idempotente = "dejar
de pagar" sin borrar). Gestión **desde la ficha del socio** (Details lista + alta + baja por fila); alta en
un formulario con selector de tipo y campos de cuota que se muestran/ocultan por JS externo (CSP). Sin
migración (solo atributos; el esquema TPH ya existía). E2E verificado en local y en producción. Racional en
`decisions.md`.

**05-07 — Módulo económico (local, sin desplegar).** Entidad `Gasto` (CRUD, categorías ONG, migración
`AddGasto`) + `ResumenEconomicoService` (cálculo por **TDD**: recurrente mensual [cuotas domiciliadas activas,
anual/12], ingresos por tipo, socios con colaboración, altas/mes, balance ingresos−gastos) + página
`/Admin/Economia` con vista global de colaboraciones. 85 tests verdes. E2E verificado.

**TODO el back de gestión desplegado y vivo en producción** (socios + colaboraciones + económico, 05-07).
`/Admin/Economia` verificada en prod.

**05-07 — Dashboards (local, sin desplegar).** 4 gráficas Chart.js **servido local** (CSP-safe) en
`/Admin/Economia`: donut ingresos por tipo, barras ingresos/gastos/balance, barras gastos por categoría,
líneas altas/mes. Datos por `data-chart` + `dashboard.js` externo. Nueva agregación `GastosPorCategoria` (TDD).
86 tests verdes. **Render visual NO verificado** (Playwright bloqueado por el entorno) → validar en navegador.

**05-07 — Fixes de UI reportados por el usuario:**
- **Campo país**: el select+buscador suelto fallaba ("min/max length 2"). Sustituido por **combo
  `<input list>`+`<datalist>`**; `site.js` (`data-pais-combo`) resuelve el código ISO al hidden
  `PaisResidencia`. OK.
- **Enum en `<select>`**: `Html.GetEnumSelectList` emite el **valor numérico**, pero el JS comparaba por
  nombre → (a) los campos de cuota (IBAN/periodicidad) **nunca se mostraban** y (b) la validación de DNI solo
  saltaba al enviar. Corregido exponiendo el valor en `data-*` (`data-colab-cuota-val`, `data-tipo-dni/nie`) y
  comparando contra eso. `[Display]` en `TipoColaboracion` (texto legible) e `Importe` → `decimal?` (evita
  `value="0"` inválido de entrada). Verificado en Node contra los JS reales + HTTP. 86 tests verdes.

**05-07 — Feedback del usuario atendido:** (a) **editar colaboración** (`Colaboraciones/Edit`: importe/
periodicidad/IBAN, no tipo ni socio; `ActualizarAsync` por TDD; enlace por fila en la ficha); (b) **previsión
ingresos vs gastos** (5ª gráfica, líneas 2 series, 6 meses; `ProyectarAsync` por TDD — extrapolación "si todo
sigue igual", no predicción). `dashboard.js` soporta multi-serie. 93 tests verdes.

**TODO EL NÚCLEO DEL MVP DESPLEGADO Y VIVO EN PRODUCCIÓN** (05-07): socios, colaboraciones (alta/editar/baja),
económico, **dashboards con 5 gráficas** (incl. previsión 6 meses). Verificado por HTTP tras deploy
(`/Admin/Economia` 200, 5 canvas, Chart.js servido). El usuario iba a probarlo online.

Pendientes por orden sugerido:

1. **Validación visual del usuario** en producción: 5 gráficas del dashboard + editar colaboración (lo que
   Playwright no pudo verificar aquí por bloqueo de red del entorno). El combo de país y el form de cuota ya
   los validó OK.
2. **ÚNICO MÓDULO QUE QUEDA: Front público + look & feel** (mobile-first, marca DIDIDAI: logo/colores/textos de
   www.dididai.org) + **traducir EN** el contenido (la infra i18n ya está lista esperando el contenido).
3. Entregables no-código del TFM (README: credenciales demo; slides; vídeo). Deadline 20/07.

Nota: en producción quedó un socio de prueba (`Prueba Produccion`); el usuario lo gestiona al preparar la demo.

Pendientes administrativos/contenido (rápidos, sin riesgo):
- Rellenar en el README las **credenciales de demo** (usuario/contraseña ficticios) y, cuando existan, las
  URLs de slides y vídeo.

## Facturación Azure — RESUELTO

- **Suscripción convertida a "Pago por uso"** (05-07, verificado: `quotaId = PayAsYouGo_2014-09-01`). Ya NO
  caduca → la web no se apagará al consumirse el crédito. Se sigue tirando primero de los ~175 € de crédito;
  después ~13 €/mes de B1. Doble control de coste: alerta de presupuesto (30 €/mes, avisos 15/27 €) + método
  de pago = **tarjeta virtual con límite** del usuario. Al aprobar el TFM, apagar o bajar a F1 para cortar el
  gasto.

## Pendientes abiertos

- **Capa de servicios en Core (`Services/`):** ✓ creada con el CRUD de socios (`ISocioService`). Los nuevos
  módulos (colaboraciones, económico) seguirán el mismo patrón: servicio en Core, páginas sin `DbContext`.
- **Validación de identidad:** ✓ HECHA (Frente 1, 05-07): por **tipo de documento** (no por país), país=
  residencia, teléfono E.164, cliente=servidor. El **IBAN** (mod-97 internacional) queda para Colaboraciones,
  siguiendo el mismo patrón de atributo `IClientModelValidator` en `ValidacionIdentidad`.
- **Email real:** sustituir `LoggingEmailSender` (stub) por SendGrid/SMTP antes de que la recuperación de
  contraseña sea útil en producción. Nueva API key → gestionar como secreto (User Secrets / app settings).
- **UI mobile-first:** principio acordado 04-07; aplicar al tocar el front público.
- **NU1903 (CVE-2025-6965):** vulnerabilidad transitiva de SQLite aceptada y documentada; sin parche a día de
  hoy. El aviso sigue visible en el build a propósito. Vigilar y actualizar cuando salga.
- **2FA:** opcional, si sobra tiempo (andamiaje de Identity disponible).
- Extraer recursos de marca de la web actual (logo, colores, textos) para el front público.
- Elegir librería de gráficas para el dashboard compatible con CSP.
- Decidir si el formulario de contacto envía email o solo persiste.
- **Azure SQL:** en roadmap (descartado para el MVP; SQLite cubre el alcance).

## Credenciales de desarrollo (no versionadas)

- Admin seed en User Secrets del proyecto Web: `Seed:AdminEmail` = `admin@dididai.org`,
  `Seed:AdminPassword` = (en User Secrets). Se re-siembra al arrancar si no existe.
- **Las mismas credenciales están en los app settings de `dididai-ong` en Azure** (no en el repo).

## Contexto ONG (de www.dididai.org)

ONG de derechos infantiles y educación especial; orfanato en Katmandú (Nepal) para menores con discapacidad.
Secciones web actuales: Inicio · Colaborar (form) · Actividad · Filosofía · Objetivos · Contacto. Contacto:
info@dididai.org. El formulario `/form` (Hacerme soci@ / Donación / Microdonación) fue la base del modelo de
datos `Socio`/`Colaboracion`. Detalle en `ORACULO.md`.

## Caveats de rama/entorno

- Branch de trabajo: `main`. (Sin flujo de ramas/PR definido aún.) **Todo commiteado y PUSHEADO** a `origin/main`.
- Comandos EF: `dotnet ef ... --project DididaiApp.Core --startup-project DididaiApp`.
- Remoto solo desde **PowerShell / terminal de VS Code** (no Git Bash); clave SSH con passphrase en el
  ssh-agent de Windows.
- **Azure:** cuenta `dididai@outlook.es`, `az` en `C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd`.
  Norton intercepta TLS → exclusiones ya añadidas. Deploy vigente: B1 / spaincentral / `dididai-ong`. Runbook
  completo en `context/deploy-azure.md`.
- Sesiones cortas y espaciadas: dejar este fichero actualizado al cerrar cada rato para poder retomar en frío.
