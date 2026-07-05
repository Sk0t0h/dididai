# AI Context — Estado actual de trabajo

> Memoria de trabajo **volátil**: el "ahora" del proyecto (foco, próximos pasos inmediatos). Se
> **sobreescribe** en cada cierre de bloque, no crece. Para la crónica histórica → `logs/`. Para el tablero
> estratégico estable → `ORACULO.md`. Para las acciones detalladas → `context/next-steps.md`.
> Actualizado: 2026-07-05.

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

**Socios (i18n + Frente 1) y CRUD de Colaboraciones DESPLEGADOS y vivos en producción** (05-07, verificados
por HTTP: alta de cuota domiciliada + ficha muestra la colaboración con IBAN). Tests: `DididaiApp.Tests` (xUnit), **79 verdes**
(ValidacionIdentidad, Paises, PrefijosTelefonicos, ValidacionIban por TDD, ColaboracionService integración).

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

**05-07 — Fix del campo país** (bug reportado por el usuario: no se podía dar de alta, el select+buscador
suelto fallaba con "min/max length 2"). Sustituido por **combo `<input list>`+`<datalist>`** con buscador
nativo integrado; el usuario teclea el nombre y `site.js` (`data-pais-combo`) resuelve el código ISO al hidden
`PaisResidencia` (`data-pais-codigo`). Verificado por HTTP (alta con España → 302; país vacío → rechazo con
mensaje claro). 86 tests verdes.

Pendientes por orden sugerido:

1. **Validar en el navegador** (lo que el entorno impide verificar por Playwright): (a) que el **combo de
   país** filtra bien al teclear y actualiza el código; (b) que las **4 gráficas** del dashboard pintan.
2. **Desplegar** el acumulado sin desplegar (dashboards + fix del combo de país; sin migración nueva).
3. **Front público + look & feel** (todo junto, mobile-first, marca DIDIDAI) + **traducir EN** el contenido.

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

- Branch de trabajo: `main`. (Sin flujo de ramas/PR definido aún.) **Cambios de hoy SIN commitear todavía.**
- Comandos EF: `dotnet ef ... --project DididaiApp.Core --startup-project DididaiApp`.
- Remoto solo desde **PowerShell / terminal de VS Code** (no Git Bash); clave SSH con passphrase en el
  ssh-agent de Windows.
- **Azure:** cuenta `dididai@outlook.es`, `az` en `C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd`.
  Norton intercepta TLS → exclusiones ya añadidas. Deploy vigente: B1 / spaincentral / `dididai-ong`. Runbook
  completo en `context/deploy-azure.md`.
- Sesiones cortas y espaciadas: dejar este fichero actualizado al cerrar cada rato para poder retomar en frío.
