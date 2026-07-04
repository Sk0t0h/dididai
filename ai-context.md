# AI Context — Estado actual de trabajo

> Memoria de trabajo **volátil**: el "ahora" del proyecto (foco, próximos pasos inmediatos). Se
> **sobreescribe** en cada cierre de bloque, no crece. Para la crónica histórica → `logs/`. Para el tablero
> estratégico estable → `ORACULO.md`. Para las acciones detalladas → `context/next-steps.md`.
> Actualizado: 2026-07-04 (noche).

## Estado actual

**Finde 1 CERRADO: esqueleto vivo y desplegado.** Hecho el 04-07: (1) arquitectura multi-proyecto (Web
`DididaiApp` + Core `DididaiApp.Core`); (2) capa de datos (EF Core 10 + SQLite, `Socio` 1:N `Colaboracion`
TPH, migración `InitialCreate`); (3) autenticación con ASP.NET Core Identity (login, recuperación stub,
registro público bloqueado, `/Admin` por rol, seed admin); (4) README completo verificado contra el enunciado;
(5) **DESPLIEGUE en producción, estable y verificado end-to-end**.

**La web está viva:** https://dididai-ong.azurewebsites.net (Azure App Service **B1**, región **Spain
Central**). Verificado por HTTP: home 200, `/Admin` anónimo → 302, login admin → 302, `/Admin` autenticado →
200. El front público sigue abierto sin login.

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

## RETOMAR AQUÍ (próximo bloque)

Finde 1 cerrado. El **Finde 2** entra en el MVP funcional. Foco natural: **CRUD de gestión de socios**, que
arrastra crear la **capa de servicios en Core** (`Services/`; las páginas inyectan servicios, no el
`DbContext`). Ver `context/next-steps.md` para el detalle accionable.

Antes de tocar producto, un par de cierres administrativos y de contenido (rápidos, sin riesgo):
- **Commit de todo lo de hoy** (código del fix de migración + README + memoria). Aún sin commitear.
- Rellenar en el README las **credenciales de demo** (usuario/contraseña ficticios) y, cuando existan, las
  URLs de slides y vídeo.

## Facturación Azure — RESUELTO

- **Suscripción convertida a "Pago por uso"** (05-07, verificado: `quotaId = PayAsYouGo_2014-09-01`). Ya NO
  caduca → la web no se apagará al consumirse el crédito. Se sigue tirando primero de los ~175 € de crédito;
  después ~13 €/mes de B1. Doble control de coste: alerta de presupuesto (30 €/mes, avisos 15/27 €) + método
  de pago = **tarjeta virtual con límite** del usuario. Al aprobar el TFM, apagar o bajar a F1 para cortar el
  gasto.

## Pendientes abiertos

- **Capa de servicios en Core (`Services/`):** aún no existe; crear con el CRUD de socios (Finde 2).
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
