# AI Context — Estado actual de trabajo

> Memoria de trabajo **volátil**: el "ahora" del proyecto (foco, próximos pasos inmediatos). Se
> **sobreescribe** en cada cierre de bloque, no crece. Para la crónica histórica → `logs/`. Para el tablero
> estratégico estable → `ORACULO.md`. Para las acciones detalladas → `context/next-steps.md`.
> Actualizado: 2026-07-17 — desplegado todo lo pendiente + unificados los azules al naranja. TODO EL CÓDIGO Y
> CONTENIDO DEL MVP ESTÁ VIVO EN PROD. Solo quedan entregables no-código.

## FOCO ACTUAL (17-07) — MVP de código/contenido CERRADO y en prod. Solo faltan entregables no-código.

`origin/main` = `HEAD` = `c7078ae`, **working tree limpio, nada pendiente de desplegar.** Todo el código y
contenido del MVP está vivo y verificado en https://dididai-ong.azurewebsites.net.

**Única tarea del MVP pendiente — entregables no-código (deadline 20/07, con colchón):**

- **README**: credenciales de demo (`admin@dididai.org` / la pass de los app settings de Azure), URL de prod,
  cómo probar cada módulo.
- **Slides** + **vídeo** de la demo.
- Ver `context/next-steps.md`.

## Cerrado hoy (17-07 — 2 deploys, todo verificado en prod; detalle en log W29)

1. **Deploy de los 2 commits que quedaban en local** — `124046a` cabecera de gestión responsive + `5cb77a5`
   periodicidad de gastos (mensual/anual con prorrateo). Runbook estándar, `RuntimeSuccessful`. Verificado en
   prod: badges Mensual/Anual + sufijos `/mes`·`/año` en Economía, ☰ de la cabecera responsive presente, 0
   inline, deploy de solo código → BD de demo intacta.

2. **Unificar los azules de Bootstrap al naranja de marca** (`c7078ae`, solo `site.css`, presentación pura):
   paginación (Economía y listados), badge "Mensual" (`text-bg-info` → naranja; "Anual" sigue gris para
   distinguir; cubre también la ficha del socio), nav "Mi cuenta" de Identity (`nav-pills`), y el halo de foco
   de botones/campos/checkboxes (`#258cfb` residual → naranja, afectaba a toda la app). Revisión general: sin
   más azules hardcodeados. Validado en local por el usuario + verificado en prod. **El usuario cerró la puerta
   a más cambios estéticos** ("se nos va de las manos").

3. **Limpieza de repo**: borrado el `dididai.db.bak-<epoch>` (backup que crea DB Browser for SQLite al guardar;
   estaba sin trackear en repo público) + añadido `*.db.bak-*` al `.gitignore` (`*.db` no cubría ese sufijo).

## ROADMAP (hablado con el usuario, NO implementado — mejoras "proyecto vivo")

- **Exportación de datos (CSV/Excel)**: gastos, cuotas/colaboraciones y **socios con todos sus datos** (la más
  valiosa: backup / gestoría). **Consideración RGPD:** solo admin, decidir **IBAN enmascarado vs completo**, y
  **registrar la exportación en auditoría**. Enunciado pendiente de afinar (formato objetivo, decisión del IBAN).
- Otros roadmap previos: pasarela de pago real (Stripe/SEPA), email real ya vivo (SendGrid), Azure SQL. Ver
  `ORACULO.md` / `context/next-steps.md`.

---

## Estado del MVP (resumen — todo VIVO en https://dididai-ong.azurewebsites.net)

- **Front público** one-page bilingüe ES/EN (hero 99%, actividad, filosofía, transparencia, 7 objetivos,
  formulario de colaboración, contacto, footer con enlaces legales). Mobile-first, CSP-safe.
- **Formulario de colaboración** → crea `SolicitudColaboracion` (NO da de alta socio; **IBAN nunca en el form
  público**). Defensas OWASP: antiforgery + honeypot + rate-limit por IP + validación servidor. 1ª capa RGPD.
- **Back `/Admin`** (español fijo por diseño): socios (CRUD + baja lógica), colaboraciones (alta/editar/baja,
  TPH, periodicidad), económico (gastos con periodicidad + balance + **5 gráficas** Chart.js), solicitudes
  (máquina de estados + acciones), administradores (alta/desactivar, superadmin protegido, forzar cambio de
  pass), **auditoría transversal** (log inmutable + diff antes/después, IBAN enmascarado), **2FA** (páginas ES +
  QR en servidor). Cabecera de gestión **responsive** (igualada al front). Toda la UI del back en naranja de
  marca (sin azules de plantilla).
- **Sesión OWASP** (idle 30 min + absolute 8 h, sin "Recordarme").
- **Páginas legales** aviso legal / privacidad / cookies (bilingües, borrador → revisión jurídica pendiente).
- **Datos de demo** poblados en prod (idempotente, flag off) para la evaluación del tribunal.

---

## Caveats de entorno (para retomar en frío)

- **Branch:** `main`. Todo commiteado y **pusheado a `origin/main`** (HEAD = `c7078ae`, 17-07). Sin flujo de PR.
- **Remoto solo desde PowerShell / terminal de VS Code** (no Git Bash); clave SSH con passphrase en el ssh-agent.
  Commits multilínea: heredoc POSIX en Bash o here-string en PowerShell (ojo al `@` basura, ver [[commits-heredoc-shell]]).
- **Azure:** cuenta `dididai@outlook.es`; `az` en `C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd`;
  Norton intercepta TLS → exclusiones ya puestas. Deploy = **B1 / spaincentral / `dididai-ong`**. Runbook
  completo en `context/deploy-azure.md`. Deploy de solo código NO toca `dididai.db` de `/home`.
- **Comandos EF:** `dotnet ef ... --project DididaiApp.Core --startup-project DididaiApp`.
- **PLAYWRIGHT bloqueado por el entorno** (`ERR_BLOCKED_BY_CLIENT` en cualquier destino; diagnóstico cerrado por
  eliminación, NO es Norton ni localhost — está en la capa del MCP). **Método de verificación: el agente valida
  por HTTP/estructura/status/0-inline; el usuario valida el render** en su navegador (o screenshots por
  OneDrive\Documentos\CLAUDE).
- **BD local SQLite:** gestionar con **DB Browser for SQLite**, no con SSMS ni la extensión de VS Code
  ([[bd-local-sqlite]]). Genera backups `dididai.db.bak-<epoch>` al guardar → ya gitignoreados (`*.db.bak-*`).
- **NU1903 (CVE SQLite):** vulnerabilidad transitiva de `SQLitePCLRaw 2.1.11` (la arrastra EF 10.0.9, la última;
  sin parche vía actualización normal). Aceptada y documentada: el vector (SQL/ficheros no confiables) no aplica.
  El aviso sale en el build a propósito — NO suprimirlo. Vigilar post-TFM.

## Credenciales de desarrollo (no versionadas)

- Admin seed en User Secrets (Web) y en app settings de Azure (no en el repo): `Seed:AdminEmail` =
  `admin@dididai.org`, `Seed:AdminPassword` = (en User Secrets / app settings). Se re-siembra al arrancar si no existe.

## Contexto ONG (de www.dididai.org)

ONG de derechos infantiles y educación especial; orfanato "BalMandir" en Katmandú (Nepal) para menores con
discapacidad. Contacto: info@dididai.org. El formulario "Hacerme soci@ / Donación / Microdonación" fue la base
del modelo `Socio`/`Colaboracion`. **AVISO seguridad:** el IBAN real de la ONG está en la web vieja; NO copiarlo
al repo público. Contenido literal del front en `context/contenido-front.md`. Detalle en `ORACULO.md`.
