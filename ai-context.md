# AI Context — Estado actual de trabajo

> Memoria de trabajo **volátil**: el "ahora" del proyecto (foco, próximos pasos inmediatos). Se
> **sobreescribe** en cada cierre de bloque, no crece. Para la crónica histórica → `logs/`. Para el tablero
> estratégico estable → `ORACULO.md`. Para las acciones detalladas → `context/next-steps.md`.
> Actualizado: 2026-07-17 (cierre de sesión) — todo el MVP de código/contenido vivo en prod + README al día.
> Solo faltan slides y vídeo para completar la entrega del TFM.

## FOCO ACTUAL (17-07) — Solo faltan SLIDES y VÍDEO. Todo lo demás del TFM está listo.

`origin/main` = `HEAD` = `1647db4`, **working tree limpio.** Todo el código, contenido y el README están al día
y en su sitio. La app entera está viva y verificada en https://dididai-ong.azurewebsites.net.

**Entrega del TFM — formulario (9 campos, todos por URL):**

| Campo | Estado |
|---|---|
| Nombre / Apellidos / Email | ✅ (email contacto `eduardofragalopez@gmail.com`) |
| URL Repositorio GitHub | ✅ `https://github.com/Sk0t0h/dididai` |
| URL Despliegue | ✅ `https://dididai-ong.azurewebsites.net` |
| **URL Slides** | ⏳ **PENDIENTE** |
| **URL Vídeo** | ⏳ **PENDIENTE** |
| Usuario de prueba | ✅ `admin@dididai.org` |
| Contraseña de prueba | ✅ la de los app settings de Azure (va en el FORMULARIO, no en el README público) |

**Al tener las 2 URLs:** rellenarlas también en el README (hoy dicen "pendiente" en la sección "Enlaces del
proyecto").

**Próximo paso (ofrecido, no arrancado):** guion de slides + guion del vídeo de demo. El contenido lo prepara el
agente; el montaje visual lo hace el usuario. Deadline **20/07** (con colchón).

## Cerrado en la sesión del 17-07 (detalle en log W29)

1. **Deploy** de los 2 commits que quedaban en local (`124046a` cabecera responsive + `5cb77a5` periodicidad de
   gastos), verificado en prod.
2. **Unificación de azules → naranja de marca** (`c7078ae`, solo `site.css`): paginación, badge "Mensual", nav
   "Mi cuenta" de Identity, halo de foco. El usuario cerró los cambios estéticos.
3. **Limpieza:** borrado `dididai.db.bak-<epoch>` + `*.db.bak-*` al `.gitignore`. NU1903 revisado y aceptado.
4. **README reescrito** (`1647db4`) al estado real del MVP; roadmap con tienda virtual de merchan → pasarela de
   pago. Slides/vídeo quedan como "pendiente" en el README.

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

- **Branch:** `main`. Todo commiteado y **pusheado a `origin/main`** (HEAD = `1647db4`, 17-07). Sin flujo de PR.
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
