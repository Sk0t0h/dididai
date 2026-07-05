# AI Context — Estado actual de trabajo

> Memoria de trabajo **volátil**: el "ahora" del proyecto (foco, próximos pasos inmediatos). Se
> **sobreescribe** en cada cierre de bloque, no crece. Para la crónica histórica → `logs/`. Para el tablero
> estratégico estable → `ORACULO.md`. Para las acciones detalladas → `context/next-steps.md`.
> Actualizado: 2026-07-05.

## Estado actual

**Finde 1 CERRADO** (esqueleto vivo y desplegado) + **CRUD de socios hecho** + **infra i18n del front público
montada** (idioma ES/EN conmutable, extensible a N idiomas).

**05-07 (tarde): infra i18n del front público.** Localización estándar de ASP.NET Core (`AddViewLocalization`
+ `.resx` + `RequestLocalizationMiddleware`, cookie provider, `es` por defecto). Selector de idioma en la
cabecera (JS externo, CSP-safe). Página `SetLanguage` (POST con antiforgery, sin open-redirect) fija la cookie
de cultura. `_Layout` e `Index` localizados como prueba. **Solo front público**: el back `/Admin` queda en
español a propósito. **Ampliar idioma = una línea en `Program.cs` + su `.resx`**, sin tocar infra ni meter
`if (idioma=="en")`. **Verificado end-to-end** (default ES; cookie EN conmuta textos y `lang`; `/Admin` no
afectado; CSRF protegido; build OK). Commit pendiente de tu OK. **SIN DESPLEGAR.** Racional en
`context/decisions.md` (entrada 05-07, "Web multi-idioma"). **Decisión clave:** la validación de
DNI/teléfono/IBAN se condicionará al **país del socio**, NUNCA al idioma de la web.

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

Dos cosas hechas y verificadas en local, **ninguna desplegada**: CRUD de socios (commiteado) e infra i18n
(commit pendiente). Pendientes por orden sugerido:

0. **(Opcional) Commitear la infra i18n** si no se hizo en la sesión anterior.
1. **Frente 1 — País ISO + validación por país** (ANTES del CRUD de Colaboraciones, es zona sensible:
   entidad + migración). `Socio.Pais` (texto libre) → `PaisCodigo` ISO 3166-1 alpha-2 como única fuente de
   verdad; desplegable con buscador, España por defecto; validación de DNI (letra si ES) / teléfono (E.164)
   condicionada al país. Migración nueva. Ver `decisions.md` (entrada 05-07 "Web multi-idioma").
2. **Desplegar a Azure** lo acumulado (CRUD + i18n [+ país si ya está]) (B1/Spain, bajo riesgo; migraciones
   se aplican solas en arranque por el fix `MigrateAsync`). Verificar en producción. Runbook en
   `context/deploy-azure.md`.
3. **Limpiar la BD local**: quedó un socio de prueba (`id=1`, "Ana Maria Garcia Lopez", DNI 12345678Z) de la
   verificación. Está en `dididai.db` (ignorada por git); borrarlo o recrear la BD antes de grabar demo. En
   producción la BD está limpia.

Después, seguir el MVP:
- **CRUD de Colaboraciones** (métodos de pago: cuota domiciliada, aportación única, Teaming). Aquí es donde
  se "dan de baja los pagos" (el caso habitual). El **IBAN** con mod-97 internacional.
- Módulo económico simple (ingresos = suma de colaboraciones) y dashboards.
- **Traducir al inglés el contenido real del front público** cuando exista (la infra ya está lista).

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
- **Validación de formato por país (`Socio.Pais`):** fuera de MVP; hoy la validación es universal/laxa a
  propósito (base de socios internacional). Reconsiderar solo si se pide. IBAN → mod-97 internacional.
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
