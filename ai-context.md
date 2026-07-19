# AI Context — Estado actual de trabajo

> Memoria de trabajo **volátil**: el "ahora" del proyecto (foco, próximos pasos inmediatos). Se
> **sobreescribe** en cada cierre de bloque, no crece. Para la crónica histórica → `logs/`. Para el tablero
> estratégico estable → `ORACULO.md`. Para las acciones detalladas → `context/next-steps.md`.
> Actualizado: 2026-07-19 (tarde) — MVP + README + slides + **guion de vídeo listo y web pulida (favicon/títulos)
> DESPLEGADA**. Único pendiente del TFM: **GRABAR el vídeo** (lo hace el usuario) y las 2 URLs del formulario.

## FOCO ACTUAL (19-07) — SOLO QUEDA GRABAR EL VÍDEO. Guion listo; prod pulida y lista para grabar.

Código, contenido, README, slides **y el guion del vídeo** al día. App viva y pulida en
https://dididai-ong.azurewebsites.net (`HEAD` = `origin/main` = `6f99f08`).

**GUION DEL VÍDEO ✅ ESCRITO** (en `OneDrive\Documentos\CLAUDE\dididai-front\guion-video-dididai.md`):
- 5–8 min, voz en off, front + back equilibrado. Tabla VOZ | PANTALLA con tiempos (teleprompter).
- Hilo end-to-end: solicitud entra por front → admin la gestiona (socio → colaboración/IBAN → economía → auditoría).
- **Tramo 1 "punto de partida":** enseñar en vivo la web anterior (`www.dididai.org`, hecha por el usuario al
  empezar) como contraste de progreso. ⚠️ NO enfocar el IBAN real que hay en la web vieja.
- **Se graba sobre PRODUCCIÓN** (refuerza "desplegado y funcionando"), no localhost. Datos ficticios `@example.org`.

**FAVICON + TÍTULOS ✅ DESPLEGADOS** (commit `6f99f08`, log W29): favicon de marca (desde `logo.png`) en front y
back; título del back unificado a `· DIDIDAI` (antes `- DididaiApp`). Verificado en prod. **Falta que el usuario
lo valide visualmente** en la pestaña del navegador.

**HERRAMIENTAS DE GRABACIÓN (asesoradas):** OBS Studio (pantalla + webcam PiP + micro de cascos en una pasada;
NVENC, mp4 híbrido) → Clipchamp (cortar + rótulos) → subir a YouTube "no listado". OBS ya instalado y configurado
por el usuario (falta añadir la fuente de webcam).

**PENDIENTE (deadline 20-07):**
1. **Grabar y montar el VÍDEO** (usuario) siguiendo el guion, sobre producción.
2. **Rellenar el formulario de entrega** con las 2 URLs: Slides (la de Gamma) + Vídeo (la que resulte).
3. Cuando exista la URL del vídeo → **añadirla al README** (hoy "_(pendiente)_" en Enlaces del proyecto).

**SLIDES ✅ CERRADAS Y PUBLICADAS** (detalle en log W29, 19-07):
- URL pública (verificada en incógnito): `https://gamma.app/docs/Una-web-a-medida-para-una-ONG-que-lo-da-casi-todo-67m0ryts67r74k2`
- Ya **añadida al README** (Enlaces del proyecto). Falta pegarla en el formulario de entrega.

**Nota deploy (19-07):** la suscripción del TFM vive en el tenant `c074f8bf...` que **exige MFA** → el token de
`az` caduca; re-loguear con `az login` a secas (Enter en el selector). Ver `context/deploy-azure.md`.
**Nota Gamma:** el agente NO puede ver el doc (WebFetch 403) → verificación de render la hace el usuario.

---

### (Contexto previo, 17-07) Estado del MVP — sigue vigente

`HEAD` = `1647db4`. Todo el MVP de código/contenido vivo y verificado en prod.

**Entrega del TFM — formulario (9 campos, todos por URL):**

| Campo | Estado |
|---|---|
| Nombre / Apellidos / Email | ✅ (email contacto `eduardofragalopez@gmail.com`) |
| URL Repositorio GitHub | ✅ `https://github.com/Sk0t0h/dididai` |
| URL Despliegue | ✅ `https://dididai-ong.azurewebsites.net` |
| **URL Slides** | ✅ `https://gamma.app/docs/Una-web-a-medida-para-una-ONG-que-lo-da-casi-todo-67m0ryts67r74k2` (falta pegarla en el form) |
| **URL Vídeo** | ⏳ **PENDIENTE** (mañana) |
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

- **Branch:** `main`. Todo commiteado y **pusheado a `origin/main`** (HEAD = `d9f029e`, 19-07). Sin flujo de PR.
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
