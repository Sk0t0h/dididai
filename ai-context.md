# AI Context — Estado actual de trabajo

> Memoria de trabajo **volátil**: el "ahora" del proyecto (foco, próximos pasos inmediatos). Se
> **sobreescribe** en cada cierre de bloque, no crece. Para la crónica histórica → `logs/`. Para el tablero
> estratégico estable → `ORACULO.md`. Para las acciones detalladas → `context/next-steps.md`.
> Actualizado: 2026-07-04.

## Estado actual

Finde 1 avanzando bien. Hecho hoy (04-07): (1) arquitectura multi-proyecto (`DididaiApp.sln` = web
`DididaiApp` + `DididaiApp.Core`); (2) capa de datos (EF Core 10 + SQLite, modelo `Socio` 1:N `Colaboracion`
TPH, migración `InitialCreate`); (3) **autenticación** con ASP.NET Core Identity (Default UI + roles sobre
`AppDbContext`, migración `AddIdentity`): login, logout, recuperación de contraseña (email **stub** que
loguea), **registro público bloqueado** (404 salvo Admin), zona `/Admin` protegida por rol, seed de admin con
credenciales en User Secrets. **El front público sigue abierto sin login.** Todo verificado end-to-end por
HTTP. Racional en `context/decisions.md` (4 decisiones del 04-07).

Ritmo real: se trabaja sobre todo en **findes** y en **ratos cortos entre semana**. Trabajo pesado a findes;
ratos de diario solo tareas pequeñas y sin riesgo.

## Foco inmediato (Finde 1, 4-5 jul — "Esqueleto vivo y desplegado")

Estado de las tareas del finde:
1. ~~Commit memoria~~ ✓ (commit 51dc831).
2. ~~EF Core + SQLite + entidad~~ ✓ (04-07).
3. ~~Auth (Identity + rol Admin, seed admin) con login y zona protegida~~ ✓ (04-07, verificado).
4. **Desplegar "hola mundo" en Azure App Service F1 (validar pipeline)** ← **SIGUIENTE**.
5. README con secciones vacías.

## Pendientes abiertos

- **Deploy Azure (siguiente tarea):** publicar en App Service F1, obtener URL pública, validar el pipeline.
  Ojo al llevar la config sensible: credenciales del seed admin como **variables de entorno** en Azure (hoy
  en User Secrets local); el `IEmailSender` sigue siendo stub.
- **Email real:** sustituir `LoggingEmailSender` por SendGrid/SMTP antes de que la recuperación de contraseña
  sea útil en producción.
- **Servicios en Core:** aún no hay capa `Services/` escrita (las páginas deberán inyectar servicios, no el
  `DbContext`). Crear al construir el CRUD de socios (Finde 2).
- **UI mobile-first:** principio acordado 04-07; aplicar cuando se toque el front público.
- **NU1903 (CVE-2025-6965):** vulnerabilidad transitiva de SQLite aceptada y documentada; sin parche a día de
  hoy. Vigilar y actualizar antes del deploy. Aviso sigue visible en el build a propósito.
- **2FA:** opcional, si sobra tiempo (andamiaje de Identity disponible).
- Extraer recursos de marca de la web actual (logo, colores, textos) para el front.
- Elegir librería de gráficas para el dashboard compatible con CSP.
- Decidir si el formulario de contacto envía email o solo persiste.

## Credenciales de desarrollo (no versionadas)

- Admin seed en User Secrets del proyecto Web: `Seed:AdminEmail` = `admin@dididai.org`,
  `Seed:AdminPassword` = (puesta en User Secrets). Se re-siembra al arrancar si no existe.

## Contexto ONG (de www.dididai.org)

ONG de derechos infantiles y educación especial; orfanato en Katmandú (Nepal) para menores con discapacidad.
Secciones web actuales: Inicio · Colaborar (form) · Actividad · Filosofía · Objetivos · Contacto. Contacto:
info@dididai.org. El formulario `/form` (Hacerme soci@ / Donación / Microdonación) fue la base del modelo de
datos `Socio`/`Colaboracion`. Detalle en `ORACULO.md`.

## Caveats de rama/entorno

- Branch de trabajo: `main`. (Sin flujo de ramas/PR definido aún.) Cambios de hoy **sin commitear todavía**.
- Comandos EF: `dotnet ef ... --project DididaiApp.Core --startup-project DididaiApp`.
- Remoto solo desde **PowerShell / terminal de VS Code** (no Git Bash); clave SSH con passphrase en el
  ssh-agent de Windows.
- Sesiones cortas y espaciadas: dejar este fichero actualizado al cerrar cada rato para poder retomar en frío.