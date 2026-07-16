# AI Context — Estado actual de trabajo

> Memoria de trabajo **volátil**: el "ahora" del proyecto (foco, próximos pasos inmediatos). Se
> **sobreescribe** en cada cierre de bloque, no crece. Para la crónica histórica → `logs/`. Para el tablero
> estratégico estable → `ORACULO.md`. Para las acciones detalladas → `context/next-steps.md`.
> Actualizado: 2026-07-16 (paquete legal + distinción cuota mensual/anual DESPLEGADOS y verificados en prod).

## FOCO ACTUAL (16-07) — Paquete legal + fix de Economía DESPLEGADOS. Solo quedan entregables no-código.

**Todo el código y contenido del MVP está VIVO en producción.** Ya no queda nada de desarrollo pendiente salvo
los **entregables no-código** (README con credenciales de demo, slides, vídeo). Deadline **20/07**, con colchón.

Cerrado hoy (2 commits, desplegados y verificados en prod — detalle en log W29 16-07):

1. **Economía: distinguir cuota mensual/anual** (`3dbce1a`). Defecto visual del usuario: una cuota domiciliada
   se mostraba igual fuese mensual o anual y el importe era ambiguo. Ahora badge `Mensual`/`Anual` + sufijo
   `/mes` `/año` en el importe, en `Economia/Index` y en la ficha del socio (`_TablaColaboraciones`). **No había
   bug de cálculo** — el recurrente ya prorratea las anuales. Solo presentación.

2. **Paquete legal bilingüe** (`a2457bb`). 3 páginas nuevas `/aviso-legal`, `/privacidad`, `/cookies` (ES/EN,
   `Pages/Legal/`, contenido por cultura vía partials, CSP-safe, placeholders `[ ]`). Formulario público: tabla
   **1ª capa RGPD** + checkbox reetiquetado a **acuse de lectura** (base = ejecución de contrato, no
   consentimiento). Footer con enlaces legales en todas las páginas (`_FooterLegal`). `/Privacy` → 302
   `/privacidad`. Investigación previa con 5 sub-agentes contra AEPD/BOE (hallazgos en log W29).
   **Textos borrador → requieren revisión jurídica y rellenar placeholders antes de uso real.**

**Verificado en prod (16-07):** 3 rutas legales 200, redirección OK, footer/1ª capa/checkbox presentes, EN vía
cookie de cultura. **BD de demo intacta** (deploy solo de código; `dididai.db` conserva su mtime, comprobado por
Kudu). Validado visualmente por el usuario.

## PRÓXIMO — Entregables no-código (única tarea del MVP pendiente)

- **README**: credenciales de demo (`admin@dididai.org` / la pass de los app settings), URL de prod, cómo probar.
- **Slides** + **vídeo** de la demo.
- Deadline 20/07. Ver `context/next-steps.md`.

## ROADMAP (hablado con el usuario 16-07, NO implementado — mejoras "proyecto vivo")

- **Exportación de datos (CSV/Excel)**: gastos, cuotas/colaboraciones y **socios con todos sus datos** (la más
  valiosa: backup / gestoría). **Consideración RGPD:** solo admin, decidir **IBAN enmascarado vs completo**, y
  **registrar la exportación en auditoría** (acceso masivo a datos personales — encaja con lo montado de RGPD).
  Enunciado pendiente de afinar con el usuario (formato objetivo, decisión del IBAN).
- Otros roadmap previos: pasarela de pago real (Stripe/SEPA), email real (SendGrid/SMTP), Azure SQL. Ver
  `ORACULO.md` / `context/next-steps.md`.

---

## Estado del MVP (resumen — todo VIVO en https://dididai-ong.azurewebsites.net)

- **Front público** one-page bilingüe ES/EN (hero 99%, actividad, filosofía, transparencia, 7 objetivos,
  formulario de colaboración, contacto, footer con enlaces legales). Mobile-first, CSP-safe.
- **Formulario de colaboración** → crea `SolicitudColaboracion` (NO da de alta socio; **IBAN nunca en el form
  público**). Defensas OWASP: antiforgery + honeypot + rate-limit por IP + validación servidor. 1ª capa RGPD.
- **Back `/Admin`** (español fijo por diseño): socios (CRUD + baja lógica), colaboraciones (alta/editar/baja,
  TPH), económico (gastos + balance + **5 gráficas** Chart.js), solicitudes (máquina de estados + acciones),
  administradores (alta/desactivar, superadmin protegido, forzar cambio de pass), **auditoría transversal**
  (log inmutable + diff antes/después, IBAN enmascarado), **2FA** (páginas ES + QR en servidor).
- **Sesión OWASP** (idle 30 min + absolute 8 h, sin "Recordarme").
- **Páginas legales** aviso legal / privacidad / cookies (bilingües, borrador).
- **Datos de demo** poblados en prod (idempotente, flag off) para la evaluación del tribunal.

---

## Caveats de entorno (para retomar en frío)

- **Branch:** `main`. Todo commiteado y **pusheado a `origin/main`** (HEAD = `a2457bb`, 16-07). Sin flujo de PR.
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
  ([[bd-local-sqlite]]).
- **NU1903 (CVE SQLite):** vulnerabilidad transitiva aceptada y documentada; sin parche aún; el aviso sale en el
  build a propósito. Vigilar.

## Credenciales de desarrollo (no versionadas)

- Admin seed en User Secrets (Web) y en app settings de Azure (no en el repo): `Seed:AdminEmail` =
  `admin@dididai.org`, `Seed:AdminPassword` = (en User Secrets / app settings). Se re-siembra al arrancar si no existe.

## Contexto ONG (de www.dididai.org)

ONG de derechos infantiles y educación especial; orfanato "BalMandir" en Katmandú (Nepal) para menores con
discapacidad. Contacto: info@dididai.org. El formulario "Hacerme soci@ / Donación / Microdonación" fue la base
del modelo `Socio`/`Colaboracion`. **AVISO seguridad:** el IBAN real de la ONG está en la web vieja; NO copiarlo
al repo público. Contenido literal del front en `context/contenido-front.md`. Detalle en `ORACULO.md`.
