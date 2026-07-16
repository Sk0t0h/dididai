# DIDIDAI.ORG — ORACULO

> Documento de **bootstrapping para agentes**. Su función es transferir el contexto y el **estado actual**
> del proyecto a otra sesión o a otro agente, **incluso sin acceso al código** (p. ej. para pegárselo a otra
> IA como Perplexity). Es la verdad de base ("ground truth") para el entendimiento estratégico. Redacción
> autoexplicativa: evitar jerga interna o abreviaturas que no se entiendan sin ver el repositorio.
>
> **Mantenimiento:** regenerar al cerrar cada bloque de trabajo sustancial (Active Focus + Module Status +
> Latest Work + Immediate Risks). Última actualización: 2026-07-16 (paquete legal —aviso legal + privacidad +
> cookies bilingües + 1ª capa RGPD en el formulario— y distinción visual cuota mensual/anual DESPLEGADOS y
> verificados en prod. **Ya no queda desarrollo del MVP: solo entregables no-código —README/slides/vídeo—.**).

## Active Focus

**CIERRE 16-07 — Paquete legal + fix de Economía DESPLEGADOS. El MVP no tiene ya NADA de desarrollo pendiente;
solo quedan los entregables no-código (README con credenciales de demo, slides, vídeo), deadline 20/07.** Dos
commits desplegados y verificados en prod: (1) **distinción visual cuota mensual/anual** (`3dbce1a`) — badge
`Mensual`/`Anual` + sufijo `/mes` `/año` en el importe, en Economía y en la ficha del socio; era defecto
puramente visual, el cálculo del recurrente ya prorrateaba las anuales. (2) **Paquete legal bilingüe**
(`a2457bb`) — 3 páginas nuevas `/aviso-legal`, `/privacidad`, `/cookies` (ES/EN, CSP-safe, con placeholders
`[ ]` para los datos de la ONG), **tabla de información básica (1ª capa RGPD)** junto al botón del formulario y
**checkbox reetiquetado a acuse de lectura** (base jurídica = ejecución de contrato, NO consentimiento, según
AEPD), footer con enlaces legales en todas las páginas, `/Privacy`→`/privacidad`. Investigación previa con 5
sub-agentes contra fuentes oficiales AEPD/BOE: cookies del sitio todas técnicas exentas → **sin banner**; sin
fondos públicos → no Declaración de Accesibilidad. **Los textos legales son BORRADOR: requieren revisión
jurídica y rellenar los placeholders antes de uso real.** Verificado en prod (rutas 200, redirección, EN vía
cookie de cultura, BD de demo intacta —deploy de solo código—). Validado visualmente por el usuario.
**Roadmap acordado (sin implementar):** exportación de datos CSV/Excel (gastos, cuotas y **socios con todos sus
datos**), con consideración RGPD (solo admin, IBAN enmascarado vs completo, registrar la exportación en
auditoría). Detalle en el log W29 (16-07).

Deadline **20/07/2026**. Producto: web taylor-made para la ONG DIDIDAI con front público + back de gestión
cerrado. **MVP:** web pública · login/roles · gestión de socios · módulo económico simple · dashboards.
Stack: EF Core + SQLite → Azure App Service. **FINDE 1 CERRADO** ("esqueleto vivo y desplegado"): hechas
arquitectura multi-proyecto (Web + Core), capa de datos (EF Core + SQLite, `Socio` + `Colaboracion` TPH),
**autenticación** (Identity con roles, verificada), **README** (completo, verificado contra el enunciado) y
**DESPLIEGUE en producción, estable y verificado end-to-end**: la web está viva en
**https://dididai-ong.azurewebsites.net** (Azure App Service **B1**, región **Spain Central** por RGPD). Se
abandonó F1 (se caía al arrancar por `QuotaExceeded`) por B1 sin cuota, financiado por el crédito. Front
público abierto. **NÚCLEO DEL MVP COMPLETO Y DESPLEGADO EN PRODUCCIÓN (05-07):** gestión de **socios**
(validación por tipo de documento, país=residencia, teléfono E.164, cliente=servidor), **colaboraciones**
(alta/editar/baja, IBAN mod-97), **módulo económico** (gastos, balance) y **dashboards** (5 gráficas Chart.js
local, incl. previsión 6 meses). Infra **i18n** del front público lista (ES/EN, N-idiomas, solo front).
**93 tests verdes** (`DididaiApp.Tests`, xUnit); **TDD** en toda la lógica de negocio (IBAN, agregaciones,
proyección). Todo verificado por HTTP en producción. **Decisión transversal:** idioma de UI, país de
residencia y validación de datos son ejes independientes; la validación la dispara el **tipo de documento**.
**FRONT PÚBLICO COMMITEADO (08-07)** y validado visualmente por el usuario: landing one-page (hero 99%,
Actividad, Filosofía, Transparencia, 7 Objetivos, Colaborar, Contacto) recreada desde el diseño de Claude
Design como Razor + CSS/JS externos **CSP-safe** (0 inline), Fraunces+Poppins autoalojadas; **formulario
público→BD** vía entidad `SolicitudColaboracion` (el admin revisa/aprueba, IBAN nunca en público) con
antiforgery + honeypot + rate-limit + RGPD; pantalla `/Admin/Solicitudes`. Commits `58de45c` (front) y
`3782ab3` (pulido del back + fixes: modal de confirmación, tablas con tema de marca, menú admin, Identity con
estilos, validación cliente=servidor del checkbox RGPD y del teléfono, panel Teaming, periodicidad
preseleccionada). **104 tests verdes.** **SIN PUSH / SIN DESPLEGAR todavía.**

**REDISEÑO DEL FLUJO DE SOLICITUDES COMPLETO (08-07, commiteado en local, SIN push/deploy).** Máquina de
estados Pendiente(gris)→Gestionando(amarillo)→Aprobada(verde)/Cancelada(rojo); log de acciones de gestión
(`AccionSolicitud`, usuario del admin no editable); matching por email/teléfono como sugerencia (sin unicidad)
+ vincular a socio existente; alta de socio nuevo desde solicitud (precarga+privacidad+vínculo); crear la
colaboración desde la solicitud (`ColaboracionId`, no duplica; microdonación→Teaming no genera). **Disociación
clave:** solicitud (intención) ≠ socio (identidad) ≠ colaboración (aportación real); aprobar ≠ crear
colaboración (el IBAN solo entra al crear). **125 tests verdes.**

**CIERRE 10-07 — SendGrid VIVO en producción; todo el MVP desplegado.** El push del 09-07 ya estaba hecho
(`HEAD` = `origin/main` = `1144d25`). Se completó lo único que quedaba del ciclo de deploy: (1) **secretos
SendGrid añadidos a los app settings de Azure** (`SendGrid__ApiKey/FromEmail/FromName`, verificados por `az`,
sin pisar los `Seed__*`); (2) **re-deploy de `1144d25`** (125 tests verdes → `RuntimeSuccessful`, home 200,
/Admin 302, CSP, 0 inline real); (3) **SendGrid confirmado E2E en producción** — POST `ForgotPassword` → 302
sin 500 y **el correo llega** a la bandeja del admin (a spam; la entregabilidad SPF/DKIM/DMARC es mejora
opcional post-TFM). **Todo el MVP —front público, solicitudes, Identity en ES, recuperación real de contraseña,
back de gestión— está VIVO y verificado en https://dididai-ong.azurewebsites.net.** **Queda:** **Bloque 3 =
alta de admins desde /Admin** (funcionalidad nueva; crearlos con `EmailConfirmed=true`; zona sensible auth →
plan formal), traducir **EN** del front, entregables no-código (README/slides/vídeo). Deadline 20/07 con
colchón. Detalle en `context/next-steps.md`.

**CIERRE 14-07 — Bloque 4 (log de auditoría transversal) DESPLEGADO Y VIVO en producción.** Última pieza de
código del MVP. Entidad `RegistroAuditoria` + `IAuditoriaService` (registrar solo-inserción + listar con
filtros y paginación) + migraciones aditivas `AddRegistroAuditoria` + `AddCambiosAuditoria` + página
`/Admin/Auditoria` de solo lectura. **Decisión (del usuario): la traza la disparan LAS PÁGINAS** tras cada
acción exitosa (pasando `User.Identity?.Name`), no los servicios → Core queda sin dependencia de HTTP,
siguiendo el patrón de `AccionSolicitud`. **Alcance ampliado**: 13 acciones (socio alta/edición/baja/
reactivación, colaboración alta/edición/baja, solicitud aprobar/cancelar/vincular, admin alta/desactivar/
reactivar), cableadas 8 páginas. **Diff antes/después** en las ediciones (columna JSON `Cambios`, IBAN
enmascarado). **147 tests verdes.** Desplegado a Azure (`RuntimeSuccessful`) y verificado en prod
(`/Admin/Auditoria` 200 con login, migraciones aplicadas al arrancar, CSS de marca, CSP). **Ya NO queda código
del MVP:** solo traducir **EN** del front + entregables no-código (README/slides/vídeo). Deadline 20/07 con
colchón. Detalle en el log W29 (14-07).

## Propósito real

Web para la ONG **DIDIDAI**, sustituyendo la web actual (www.dididai.org). Dos caras: un **front público**
(informativo) y un **back de gestión cerrado** (privado, con login) para que la ONG administre su día a día
sin depender de un CMS genérico tipo WordPress/Joomla — solución **a medida**. Es el TFM del Máster de
Desarrollo con IA (BIG School); repo público; licencia MIT a nombre de DIDIDAI.

**Sobre la ONG (de www.dididai.org):** ONG de derechos infantiles y educación especial; opera en un orfanato
de Katmandú (Nepal) para menores con discapacidad (recursos educativos, estimulación multisensorial, apoyo
terapéutico/médico). Mensaje de marca: "el 99% de los ingresos va a acción directa". Contacto:
info@dididai.org.

**Web actual — secciones (base del front público del MVP):** Inicio · Colaborar (formulario `/form`) ·
Actividad · Filosofía · Objetivos · Contacto. **Recursos reutilizables:** logo (`/images/logo.png`),
imágenes temáticas, textos de misión/objetivos, colores de marca (por extraer). El mensaje del "99% a acción
directa" refuerza el valor del **módulo económico + dashboards** (transparencia).

**MVP a entregar (TFM):** web pública · autenticación con roles · gestión de socios (CRUD) · módulo económico
simple (ingresos/gastos) · informes visuales (dashboards).
**Roadmap (post-TFM):** gestor de contenido (CMS) completo, contabilidad avanzada.

## Module Status

> Estados: OPERATIVO (en uso) · IMPLEMENTADO (en código, uso no confirmado) · INFERRED (deducido del código,
> por validar con negocio) · PLANIFICADO (acordado, sin código aún).

| Módulo | Estado |
|--------|--------|
| Arquitectura solución (Web `DididaiApp` + Core `DididaiApp.Core`) | IMPLEMENTADO (04-07) |
| Persistencia (EF Core 10 + SQLite, `AppDbContext`, migración `InitialCreate`) | IMPLEMENTADO (04-07) |
| Modelo de datos (`Socio` + `Colaboracion` TPH: Cuota/Aportación/Teaming) | IMPLEMENTADO (04-07, sin UI) |
| Web shell (Razor Pages: Index, Privacy, Error) | IMPLEMENTADO (plantilla por defecto, sin contenido propio) |
| Autenticación + roles (Identity, back cerrado) | OPERATIVO (04-07; pulido 09-07; **DESPLEGADO 10-07**): login, `/Admin` protegido por rol, seed admin, registro público bloqueado (404). **Vistas en español y depuradas** (vivas en prod): overrides propios (Login/recuperación/Logout/Manage) con PageModel concreto; sin registro/confirmar-email/externos; 2FA en inglés. **Cabecera de marca minimalista** (logo→inicio, look crema) en las páginas sin sesión, unificada con el back con sesión; enlace "Volver a acceder" en recuperación (10-07, desplegado). **Recuperación de contraseña real vía SendGrid** — desplegada y **confirmada E2E en producción** (10-07): secretos `SendGrid__*` en Azure, el correo llega (a spam; entregabilidad SPF/DKIM/DMARC = mejora post-TFM). **Política de sesión OWASP (15-07, desplegada):** idle 30 min sliding + absolute 8 h (`OnValidatePrincipal`) + sin "Recordarme" (`isPersistent:false`) — antes eran los 14 días renovables por defecto de Identity. **2FA/TOTP traducido a ES + QR generado en servidor (15-07, EN LOCAL sin desplegar):** 8 páginas override (gestión + login-2FA), QRCoder → PNG data-URI CSP-safe, verificado E2E con happy-path TOTP real. Opcional pero presentable |
| Gestión de usuarios admin (alta/baja desde /Admin) | OPERATIVO (12-07, DESPLEGADO y verificado en prod): `/Admin/Usuarios` (listar+crear+desactivar/reactivar) vía `AdminUsuarioService`. Sustituye al registro público. Admins nacen con `EmailConfirmed=true` + rol Admin. **Salvaguarda del superadmin** (= `Seed:AdminEmail`, intocable) + nadie se desactiva a sí mismo. Baja lógica por lockout (no borra). **Forzar cambio de contraseña en el 1er login** (claim `must-change-password` + page filter). Validación cliente+servidor, `js-confirm` CSP-safe. Enlace en el menú del back; menú de gestión del front adelgazado a "Gestión"+Salir. Sin migración. 12 tests. Verificado E2E en local y prod |
| Log de auditoría transversal (`/Admin/Auditoria`) | OPERATIVO (14-07, **DESPLEGADO y verificado en prod**): entidad `RegistroAuditoria` + `IAuditoriaService` (registrar solo-inserción; listar con filtros usuario/acción/fechas + paginación, orden desc) + migraciones aditivas `AddRegistroAuditoria` + `AddCambiosAuditoria` + página de **solo lectura**. **Traza disparada por las PÁGINAS** tras cada acción exitosa (`User.Identity?.Name`), Core sin dependencia de HTTP. **15 acciones** auditadas (socio alta/edición/baja/reactivación, colaboración alta/edición/baja, solicitud aprobar/cancelar/vincular, admin alta/desactivar/reactivar, gasto alta/baja). **Diff antes/después en las ediciones** (socio y colaboración): columna JSON `Cambios` poblada por el servicio (`ConstructorCambios`), IBAN enmascarado; la vista muestra «Campo: antes → después». Inmutable (no editar/borrar). Enlace en menú del back + card en panel. 12 tests (147 verdes totales) |
| **Despliegue en producción (Azure App Service B1, Spain Central)** | **OPERATIVO (04-07)**: https://dididai-ong.azurewebsites.net, verificado end-to-end; migración+seed en arranque |
| Capa de servicios (Core `Services/`) | IMPLEMENTADO (05-07): `ISocioService`/`SocioService`; páginas no tocan `DbContext`. Nuevos módulos siguen el patrón |
| Internacionalización (i18n) front público | OPERATIVO (15-07): infra ES/EN por selector+cookie, extensible a N idiomas, `es` por defecto. Solo front; `/Admin` e Identity (login/2FA/etc.) en español fijo por diseño. **Contenido EN COMPLETO** (`Index.en.resx` 78 claves + `_PublicLayout.en.resx`), markup HTML preservado, nombres propios sin traducir. Verificado E2E (cookie de cultura: EN y ES sin residuos). EN LOCAL sin desplegar |
| **Front público (landing one-page + formulario→BD)** | **IMPLEMENTADO (07-07, SIN desplegar)**: `Index.cshtml` recreado desde el diseño de Claude Design (hero 99%, Actividad, Filosofía, Transparencia, 7 Objetivos, Colaborar, Contacto), layout propio `_PublicLayout`, CSS/JS externos **CSP-safe** (`front.css`/`front.js`, 0 inline), Fuentes Fraunces+Poppins autoalojadas. Formulario con **campos por tipo**. Contenido ES (`Index.resx`); EN pendiente (fallback a ES). Verificado E2E por HTTP |
| **Solicitudes de colaboración (flujo completo)** | **IMPLEMENTADO (08-07, SIN desplegar)**: formulario público→BD + gestión completa. **Máquina de estados** Pendiente/Gestionando/Aprobada/Cancelada; **log de acciones** de gestión (`AccionSolicitud`, usuario del admin no editable, 1ª acción→Gestionando); **matching** por email/teléfono (sugerencia, sin unicidad) → vincular a socio existente; **alta de socio nuevo** desde solicitud (precarga+privacidad+vínculo); **crear la colaboración** desde la solicitud (`ColaboracionId`, no duplica; Donación→AportacionUnica, Socio→CuotaDomiciliada con IBAN, Microdonación→Teaming no genera). Ficha del socio muestra "Solicitudes vinculadas". **IBAN nunca en público** (solo al crear la colaboración). Defensas del form: antiforgery + honeypot + rate-limit por IP solo POST + RGPD. Disociación solicitud≠socio≠colaboración |
| Tests unitarios (`DididaiApp.Tests`, xUnit) | IMPLEMENTADO (08-07): **125 tests verdes** (`ValidacionIdentidad`, `Paises`, `PrefijosTelefonicos`, `ValidacionIban`, `ColaboracionService`, `ResumenEconomicoService`, `SolicitudColaboracionService` [+acciones/matching/vinculación/crear-colaboración], **`SocioService`** [matching]). `dotnet test` |
| Gestión de socios (CRUD) | OPERATIVO (05-07, DESPLEGADO y verificado en prod): alta/listado/ficha/edición, baja lógica+reactivar, DNI único, Email no único. **Validación por TIPO de documento** (DNI/NIE letra, pasaporte/otro laxo), **país=residencia** (ISO, desplegable+buscador), **teléfono E.164** (prefijo+número), **cliente=servidor** (atributos IClientModelValidator + adaptadores jquery-validation) |
| Gestión de Colaboraciones (CRUD) | OPERATIVO (05-07, DESPLEGADO): alta (3 tipos, form con selector), **editar** (importe/periodicidad/IBAN), listado y baja lógica desde la ficha del socio; IBAN mod-97 (TDD) + `[Iban]` cliente=servidor; servicio en Core con tests de integración |
| Módulo económico simple (ingresos/gastos) | OPERATIVO (05-07, DESPLEGADO): entidad `Gasto` (CRUD, categorías ONG), servicio de resumen por TDD (recurrente mensual, ingresos por tipo, socios con colaboración, altas/mes, balance, **previsión**), página `/Admin/Economia` con vista global de colaboraciones |
| Dashboards / informes visuales | OPERATIVO (05-07, DESPLEGADO): **5 gráficas** Chart.js servido local en `/Admin/Economia` (donut ingresos por tipo, barras ingresos/gastos/balance, barras gastos por categoría, líneas altas/mes, **líneas previsión 6 meses ingresos vs gastos**); datos por `data-chart` + `dashboard.js` externo (CSP-safe, multi-serie). Pendiente solo validación visual del usuario |
| Páginas legales (aviso legal · privacidad · cookies) | OPERATIVO (16-07, DESPLEGADO y verificado en prod): 3 páginas en `Pages/Legal/` con rutas `/aviso-legal`, `/privacidad`, `/cookies` (ES/EN por partials de cultura, layout público, **CSP-safe**), conforme a LSSI art. 10, RGPD art. 13 y guía de cookies AEPD. **Textos BORRADOR con placeholders `[ ]`** (datos de la ONG) → revisión jurídica pendiente. Cookies: solo técnicas exentas, sin banner. Footer con enlaces legales en todas las páginas (`_FooterLegal`); `/Privacy` (plantilla) → 302 `/privacidad`. **Formulario público:** tabla de información básica (1ª capa RGPD) + checkbox como **acuse de lectura** (base = ejecución de contrato). Sin migración |
| Exportación de datos (CSV/Excel: gastos, cuotas, socios) | ROADMAP (16-07, acordado sin implementar): la de socios con todos sus datos es la más valiosa. RGPD: solo admin, IBAN enmascarado vs completo, registrar la exportación en auditoría |
| Gestor de contenido (CMS) | ROADMAP (fuera de MVP) |
| Contabilidad avanzada | ROADMAP (fuera de MVP) |

## Latest Work

- **2026-07-16 — Paquete legal (aviso legal + privacidad + cookies, bilingüe) + 1ª capa RGPD en el formulario,
  DESPLEGADO a prod**. Cerrado el último pendiente de contenido. **3 páginas nuevas** en `Pages/Legal/` con
  rutas amigables (`/aviso-legal`, `/privacidad`, `/cookies`), layout `_PublicLayout`, contenido por cultura vía
  partials `_X.es/.en.cshtml`, **CSP-safe** (0 inline), con **placeholders `[ ]`** para los datos de la ONG
  (borrador → revisión jurídica pendiente). **Formulario público:** tabla de **información básica (1ª capa
  RGPD)** junto al botón enviar + **checkbox reetiquetado a acuse de lectura** (antes decía "autorizo el
  tratamiento", base legal incorrecta → la base es **ejecución de contrato**, no consentimiento, hallazgo AEPD).
  **Footer** con enlaces legales en todas las páginas (`_FooterLegal` + `_HeaderLegal` compartidos); `/Privacy`
  (plantilla) → 302 `/privacidad`. CSS `.legal-page` responsive (tablas apiladas en móvil). **Investigación
  previa con 5 sub-agentes contra fuentes oficiales AEPD/BOE**: base = contrato no consentimiento; **cookies del
  sitio todas técnicas exentas → sin banner ni CMP**; aviso legal recomendable (LSSI art. 10); **sin fondos
  públicos → no Declaración de Accesibilidad** (RD 1112/2018 no aplica). El form NO pide IBAN (crea una
  solicitud) → 1ª capa ligera; el detalle IBAN/SEPA vive en la política completa. Commit `a2457bb`, build limpio,
  sin migración. Verificado en prod (3 rutas 200, redirección, footer/1ª capa/checkbox, EN por cookie de cultura;
  **BD de demo intacta** —deploy de solo código, `dididai.db` conserva mtime). Validado visualmente por el
  usuario. Detalle en el log W29 (16-07).
- **2026-07-16 — Economía: distinguir cuota mensual/anual (defecto visual del usuario), DESPLEGADO**. Una cuota
  domiciliada se mostraba igual fuese mensual o anual, y el importe (`30,00 €`) era ambiguo. Añadido badge
  `Mensual` (info) / `Anual` (gris) junto al tipo y **sufijo `/mes` o `/año`** en el importe, en `Economia/Index`
  ("Colaboraciones (todas)") y en la ficha del socio (`_TablaColaboraciones`, donde además se limpió la columna
  "Detalle" que volcaba el enum crudo). **No había bug de cálculo**: `ResumenEconomicoService` ya prorratea las
  anuales (`Importe/12`) en el recurrente y la proyección; era puramente presentación. `Solicitudes/Details` ya
  mostraba la periodicidad etiquetada → no se tocó. Commit `3dbce1a`.
- **2026-07-15 — DemoSeeder: datos de demo ficticios para la evaluación (que se hace sobre PRODUCCIÓN), EN
  LOCAL sin desplegar/poblar prod**. El tribunal del máster valida sobre el producto en producción, con la BD
  casi vacía → hace falta poblarla con datos realistas. `DemoSeeder.SeedDemoAsync` (Core), invocado en
  `Program.cs` tras `SeedAdminAsync`, activo con flag **`Seed:DemoData=true`**, **idempotente**. Volumen
  medio-alto: ~28 socios (altas en 12 meses, 2 de baja, ES + GB/FR/DE/PT), ~26 colaboraciones de los 3 tipos
  TPH, 15 gastos por las 5 categorías en 12 meses, 10 solicitudes en los 4 estados con acciones, ~50 registros
  de auditoría. **Inserción directa por `AppDbContext`** (no por los services) para poder **fechar** las altas
  en el pasado (los services fijan "ahora" y aplanarían los dashboards); los valores cumplen las validaciones
  reales — DNI/NIE con letra mód-23, **IBAN mod-97 calculado en runtime**, teléfonos E.164 — y la auditoría se
  inserta a mano (la traza la disparan las páginas, no los services). **RGPD:** datos inequívocamente ficticios
  (nombres inventados, `@example.org`, DNIs válidos-en-formato, IBAN de prueba). Verificado en local con BD
  limpia (28 socios, 26 colab, 15 gastos, 10 solicitudes, 50 auditoría; dashboards con cifras reales; los 20
  IBAN pasan mod-97). Sin migración. **DESPLEGADO y PROD POBLADA (commit `1cd137b`)**: operativa con `az`/Kudu
  (parar app → borrar `dididai.db` de `/home` → flag on → arrancar migra+siembra admin+demo → verificado en prod
  28 socios/10 solicitudes/50 auditoría/5 gráficas, login admin OK → flag off; reinicio no duplica). El reset
  limpió los restos de prueba y los «VERIF DEPLOY». **Prod lista para la evaluación del tribunal.** Detalle en
  `decisions.md` (15-07) y log W29.
- **2026-07-15 — Traducción EN del front público COMPLETA (última pieza del MVP), DESPLEGADA a prod**. La
  infra i18n ya estaba (selector + cookie, `es` por defecto); faltaba solo el contenido inglés, que caía a ES
  por fallback. Traducido todo el front: **`Index.en.resx`** (78 claves: hero del 99%, Actividad, Filosofía,
  Transparencia, 7 Objetivos, Colaborar, formulario, Contacto, footer) con el **markup HTML embebido preservado**
  y **nombres propios/marca sin traducir** (DIDIDAI, BalMandir, Teaming; Katmandú → Kathmandu); creado
  **`_PublicLayout.en.resx`** (meta description SEO). **Frontera confirmada** (a raíz de una duda del usuario):
  el bilingüe es **solo del front público**; el back `/Admin` y todas las páginas de Identity (login, 2FA, etc.)
  van en **español fijo por diseño**. Verificado E2E por HTTP con la cookie de cultura: `c=en` → `lang="en"` +
  contenido inglés con 0 residuos ES; `c=es` → español con 0 residuos EN. Build limpio. **Ya NO queda código ni
  contenido del MVP**, solo entregables no-código. Detalle en el log W29 (15-07). **Pendiente: commit + deploy.**
- **2026-07-15 — Páginas de 2FA traducidas a español + QR generado en servidor (CSP-safe), DESPLEGADO a prod**. El usuario detectó (pantallazo) que la configuración de 2FA (`EnableAuthenticator`, "Configure
  authenticator app") seguía en inglés (Default UI de Identity, nunca traducida) y que el QR no se mostraba (la
  Default UI lo pinta con una lib JS que el proyecto no carga por la CSP). Traducido **todo el flujo de 2FA**
  por override propio (mismo método que login/perfil/contraseña: PageModel concreto tipado a `IdentityUser`, ES,
  CSP-safe 0-inline): **8 páginas** — 6 de gestión (`TwoFactorAuthentication`, `EnableAuthenticator`,
  `Disable2fa`, `ResetAuthenticator`, `GenerateRecoveryCodes`, `ShowRecoveryCodes`) + 2 de login
  (`LoginWith2fa`, `LoginWithRecoveryCode`). **QR generado en servidor** con la dependencia **QRCoder** (1.6.0)
  como PNG data-URI base64 en un `<img>` (CSP-safe; `img-src` ya permite `data:`), conservando la clave manual.
  Los `OnGet` que exigían 2FA activa (o login-2FA fuera de flujo) **redirigen** en vez de lanzar 500.
  **Verificado E2E por HTTP incluido el happy-path TOTP real**: se calculó un código de 6 dígitos (RFC 6238)
  desde la clave del QR → activó la 2FA, y se hizo el login completo password→código→acceso; el QR se validó
  decodificando el data-URI (PNG real de 857 B). **La 2FA de prueba se desactivó** (BD del admin restaurada:
  login normal va directo). **147 tests verdes**, build limpio, sin migración (Identity ya trae 2FA/tokens). El
  2FA sigue siendo opcional pero ahora presentable. Detalle en `decisions.md` (15-07) y log W29. **Pendiente:
  commit + deploy.**
- **2026-07-15 — Política de sesión del back endurecida (OWASP valor-medio), EN LOCAL sin desplegar**. El
  usuario reportó que la sesión de admin duraba demasiado (siempre logueado en localhost). Diagnóstico:
  `ConfigureApplicationCookie` solo fijaba rutas → defaults de Identity (`ExpireTimeSpan` 14 días +
  `SlidingExpiration` renovable = sesión que no caducaba) + login con "Recordarme" (cookie persistente 14
  días). Corregido alineando con la OWASP Session Management Cheat Sheet para app de valor medio (RGPD):
  **idle 30 min** (sliding) + **absolute 8 h** (tope duro vía `OnValidatePrincipal` comparando `IssuedUtc`
  contra `UtcNow`; Identity no lo trae de fábrica) + **sin "Recordarme"** (`isPersistent:false` fijo, casilla
  eliminada). Sin migración, reversible. Verificado E2E por HTTP (login sin Recordarme, cookie de sesión no
  persistente, `/Admin` 200); **147 tests verdes**, build limpio. **DESPLEGADO** (`ba908c6`,
  `RuntimeSuccessful`; prod: home 200, /Admin 302, login sin Recordarme, CSP). **Corrección de un aviso previo
  mío erróneo:** el deploy **NO** cierra las sesiones abiertas (la política no es retroactiva y las claves de
  Data Protection persisten entre deploys → verificado: la sesión del usuario en prod no se cerró); rige solo
  para logins futuros, para migrar una sesión existente hay que logout+login. Detalle en `decisions.md` (15-07)
  y log W29.
- **2026-07-14 — DEPLOY del Bloque 4 (+ diff + fixes) a producción**. Desplegado a Azure (`dididai-ong`,
  B1/Spain) siguiendo el runbook: 147 tests verdes → publish Release → zip 30 MB → `az webapp deploy` =
  `RuntimeSuccessful` (sin timeout, BD ya en `/home`). Migraciones `AddRegistroAuditoria` + `AddCambiosAuditoria`
  aplicadas al arrancar. Verificado en prod: home 200, /Admin 302, `/Admin/Auditoria` 200 con login (registro +
  filtros), enlace en el menú, CSS de marca servido, CSP. **Todo el Bloque 4 —log de auditoría + diff
  antes/después + email de admin + filtros de marca— VIVO.** No queda código del MVP: solo EN + entregables.
- **2026-07-14 — Pulido post-revisión (feedback del usuario)**: (1) la auditoría de admin mostraba el GUID en
  vez del email → se resuelve el email por id antes de la acción; (2) los filtros del back salían azul plano
  (`btn-outline-primary`/`secondary` sin tematizar) → tematizados a naranja/tinta de marca en `site.css`.
- **2026-07-14 — Bloque 4: diff antes/después en las ediciones**. "Editó el socio" no bastaba: ahora las
  ediciones de socio y colaboración guardan el detalle campo-a-campo (columna JSON `Cambios`, helper
  `ConstructorCambios`, IBAN enmascarado). El diff se calcula en el servicio (donde el "antes" está limpio);
  `ActualizarAsync` de socio/colab devuelve un record (resultado + cambios). La vista muestra «Campo: antes →
  después». Migración aditiva `AddCambiosAuditoria`. 147 verdes.
- **2026-07-14 — Bloque 4: log de auditoría transversal (implementación inicial)**.
  Última pieza de código del MVP: registro automático e inmutable de las acciones de gestión, consultable en
  `/Admin/Auditoria` (solo lectura). No sustituye a `AccionSolicitud` (gestión manual por solicitud); esta es
  traza automática transversal. **Entidad `RegistroAuditoria`** (Fecha UTC, Usuario, Accion enum de 13 tipos,
  Entidad, EntidadId **string** —int para socios/colab/solicitud, GUID para admins—, Detalle legible ≤500) +
  migración **aditiva** `AddRegistroAuditoria` (solo tabla + índice por Fecha). **`IAuditoriaService`/
  `AuditoriaService`**: `RegistrarAsync` (solo-inserción, fecha la fija el servicio, respalda usuario vacío) +
  `ListarAsync` con filtros (usuario parcial, acción, rango de fechas con "hasta" inclusivo por día), orden
  desc y **paginación** (la tabla crece sin baja lógica). **Decisión (del usuario): la traza la disparan LAS
  PÁGINAS** tras la acción exitosa, pasando `User.Identity?.Name` — mismo patrón que `AccionSolicitud`, Core
  sin `IHttpContextAccessor` (alternativa "registrar en los servicios" descartada por propagar `usuario` a
  todas las firmas y acoplar Core a la sesión). **Alcance ampliado (13 acciones)**, 8 páginas cableadas.
  **Página `/Admin/Auditoria`** de solo lectura (filtros GET + tabla + paginación, CSP-safe 0 inline, tema de
  marca), enlace en el menú del back + card en el panel. **8 tests nuevos → 145 verdes.** **Verificado E2E por
  HTTP** (alta/baja de socio registran con detalle+usuario, orden desc, filtros por acción/usuario, CSP, 0
  inline; Playwright sigue bloqueado por el entorno). Datos de prueba borrados de la BD local. **Queda: commit
  + push + deploy** (migración aditiva, se aplica sola al arrancar). Detalle en el log W29 (14-07).
- **2026-07-12 — Bloque 3: gestión de usuarios admin desde /Admin, DESPLEGADO a producción** (`origin/main` =
  `b89b8b0`, verificado en prod). Última funcionalidad de código del MVP: la vía interna que sustituye al
  registro público quitado. Ejecutado en fases pequeñas (una idea por fase, compilable + verde cada una).
  **`AdminUsuarioService`** (Core, encapsula `UserManager`): crear con **`EmailConfirmed=true`** + rol Admin
  (distingue email duplicado de contraseña débil por los CÓDIGOS de error de Identity), listar (solo rol Admin),
  **desactivar/reactivar por lockout** (baja lógica: `LockoutEnd=MaxValue`, no borra → conserva la futura
  auditoría). **Salvaguarda del superadmin** (idea del usuario, mejor que la guarda "último admin activo"
  descartada): el admin primigenio = el del `Seed:AdminEmail` es intocable y nadie puede desactivarse a sí mismo
  → "quedarse sin admin" cubierto por construcción. **Forzar cambio de contraseña en el 1er login**: claim
  `must-change-password` (AspNetUserClaims, sin migración; higiene, no control fuerte) + `ForzarCambioPasswordFilter`
  (page filter global) que redirige a ChangePassword mientras el claim esté (excluye la propia página y el
  logout); ChangePassword lo quita y el `RefreshSignInAsync` re-emite la cookie sin él. Se eligió claim sobre
  columna en `AppUser` (evita migración/refactor del tipo de usuario para un flag de un solo uso; la columna se
  justificaría solo con varios flags o control fuerte). **Páginas** `/Admin/Usuarios` (Index+Create), validación
  cliente+servidor, antiforgery, `js-confirm` CSP-safe, requisitos de contraseña visibles; UI oculta el botón al
  superadmin y a uno mismo. **Menús:** enlace "Administradores" en el back; menú de gestión del **front**
  adelgazado a "Gestión" (→ panel) + Salir (el detalle vive en el back; descarga la barra en móvil). **12 tests
  nuevos → 137 verdes.** Sin migración. **Verificado E2E en local** (incl. guarda "uno mismo" contra POST
  forjado; flujo de forzar-cambio con bordes: sin bucle, logout no atrapado) **y en producción** (deploy
  `RuntimeSuccessful`; `/Admin/Usuarios` 200 con badge "Principal", menú back con "Administradores", front con
  sesión = Gestión+Salir, Create 200; home 200, /Admin 302, CSP). Sin admin de prueba en prod. Detalle en el log
  W28 (12-07).
- **2026-07-10 — Cabecera de marca en las páginas de Identity + botón volver** (desplegado y verificado en
  prod, `8288449`): las páginas de Identity sin sesión (login, recuperación de contraseña) servían la navbar
  Bootstrap genérica de plantilla, distinta del front y del back con sesión. Se sustituye la rama `else` (sin
  sesión) del `_Layout` por la **cabecera de marca minimalista** (solo logo→inicio, look crema), reutilizando
  las clases `.admin-header` de la rama con sesión (0 CSS nuevo). Añadido enlace "← Volver a acceder" en
  `ForgotPassword` y su confirmación. CSP-safe, sin tocar lógica de auth. Validado visualmente por el usuario.
  De paso: se aclaró que el autofill de la "contraseña actual" en Manage es el gestor del navegador (no un dato
  del servidor; `autocomplete=current-password` es lo recomendado) → no se toca. Y se **cerró el diagnóstico
  del bloqueo de Playwright**: por eliminación (no es localhost, ni proxy, ni Defender, ni Norton —probado
  desactivándolo—, ni sandbox de Claude Code —el CLI no lo tiene—) el bloqueo vive en la capa del servidor MCP
  de Playwright, no en la máquina; no perseguir más, validación visual la hace el usuario. Detalle en el log W28.
- **2026-07-10 — SendGrid vivo en producción (cierre del ciclo de deploy)**: al retomar, el push del 09-07 ya
  estaba hecho (`HEAD` = `origin/main` = `1144d25`). El usuario añadió los secretos `SendGrid__ApiKey/FromEmail/
  FromName` a los app settings de `dididai-ong` (verificados por `az`: presentes, valores correctos, sin pisar
  los `Seed__*`). **Re-deploy de `1144d25`** siguiendo el runbook: 125 tests verdes → publish Release → zip
  30 MB → `az webapp deploy` = `RuntimeSuccessful` (sin timeout de arranque en frío, la BD ya existía en
  `/home`). Verificado en prod: home 200, /Admin 302, CSP presente, 0 inline real. **SendGrid confirmado E2E en
  producción**: POST `ForgotPassword` → 302 a la confirmación sin 500 (la key carga bien) y **el correo llega**
  a la bandeja del admin (a la carpeta de spam). Entregabilidad (SPF/DKIM/DMARC del dominio para evitar spam) =
  mejora **opcional post-TFM**, no bloquea el MVP. Todo el MVP queda vivo y verificado en producción. Detalle
  en el log W28 (10-07).
- **2026-07-09 — Deploy verificado + Identity en español + SendGrid real** (3 commits en local, SIN push;
  el usuario se va de viaje al cerrar). (1) **Deploy** de `58ac972` a Azure (`dididai-ong`): RuntimeSuccessful,
  home 200, /Admin 302, las 3 migraciones del módulo de solicitudes aplicadas al arrancar; el riesgo del enum
  quedó descartado (nunca se había desplegado ese módulo). Commit de memoria `909b3cc`. (2) **Identity en
  español + vistas depuradas** (`c5ded0a`): método B (overrides a mano, sin scaffolder), páginas alcanzables
  con PageModel concreto tipado a `IdentityUser` (Login sin registro/externos; ForgotPassword/ResetPassword +
  confirmaciones; Logout; Manage/Perfil; Manage/Contraseña; `_ManageNav` reducido a Perfil/Contraseña/2FA;
  `_ViewImports` del área). **2FA en inglés** (servido por Identity, decisión del usuario). No se tocó
  `Program.cs` ni el middleware de bloqueo de registro (Register sigue 404). Verificado por HTTP: login POST
  302, textos ES, Register anónimo 404, menú sin Email/PersonalData/External, CSP presente y 0 inline. (3)
  **SendGrid real** (`ec6c546`): `SendGridEmailSender` (paquete SendGrid 9.29.3) sustituye al stub
  `LoggingEmailSender`; misma `IEmailSender`, la recuperación de contraseña **envía de verdad**. Config secreta
  en User Secrets (`SendGrid:ApiKey/FromEmail/FromName`; From=`info@dididai.org`, dominio autenticado en
  SendGrid). Fallback seguro sin key (loguea, no rompe el arranque). **Verificado E2E: el correo llega** a la
  bandeja del admin. **125 tests verdes.** **Queda al volver del viaje:** push + re-deploy (⚠️ añadir secretos
  `SendGrid__*` a los app settings de Azure), Bloque 3 (alta de admins desde /Admin, con `EmailConfirmed=true`),
  traducir EN, entregables no-código. Detalle en `context/next-steps.md`.
- **2026-07-07 — Front público implementado (landing + formulario→BD) desde el diseño de Claude Design**:
  último módulo del MVP. Llegó el handoff bundle de Claude Design; el prototipo (formato `x-dc`, con estilos y
  handlers **inline**) se recreó como **Razor + CSS/JS externos, CSP-safe** (0 inline verificado por HTTP), no
  copiando su estructura interna sino su salida visual. `Index.cshtml` sustituye la home de plantilla; layout
  público propio `_PublicLayout.cshtml` (el back sigue con Bootstrap `_Layout`). **Fuentes Fraunces+Poppins
  autoalojadas** en `wwwroot/fonts/` (el prototipo las traía de Google Fonts → violaba la CSP). Imágenes
  optimizadas en `wwwroot/images/front/`. **Formulario con campos por tipo** (Socio→periodicidad,
  Donación→importe, Microdonación→1€/mes fijo; mostrar/ocultar por `front.js`). **Flujo pago = solicitud →
  revisa el admin** (Stripe/SEPA = roadmap, no MVP): entidad **`SolicitudColaboracion`** + migración aditiva
  `AddSolicitudColaboracion` (tabla nueva, no toca las existentes) + `ISolicitudColaboracionService` (10 tests)
  + pantalla **`/Admin/Solicitudes`** (listar/filtrar/aprobar/rechazar con nota) + badge de pendientes; aprobar
  enlaza al alta de Socio con datos precargados (`Socios/Create.OnGet` acepta query params). **IBAN NUNCA en el
  form público.** Defensas OWASP: antiforgery + **honeypot** (respuesta neutra) + **rate-limit por IP solo en
  POST** (5/5 min; bug corregido en verificación: el atributo limitaba también los GET/visitas) + validación
  server + mensaje neutro. i18n: contenido **ES** en `Index.resx`; **EN pendiente** (cae a ES por fallback).
  **Verificado E2E** con la app corriendo (home 200 + CSP + 0 inline; assets 200; form válido→BD y visible en
  admin; honeypot→no guarda; login→ficha→aprobar cambia estado y baja el badge). **103 tests verdes.** **SIN
  COMMITEAR / SIN DESPLEGAR.** Detalle en el log W27 (07-07) y en el commit.
- **2026-07-05 (noche) — CSP estricta activada (endurecimiento previo al front público)**: el proyecto seguía
  la disciplina anti-inline (todo el JS externo) pero NO tenía cabecera `Content-Security-Policy` activa. Se
  añade middleware `SecurityHeadersMiddleware` que emite en cada respuesta una CSP `default-src 'self'` sin
  `unsafe-inline` (script/style/img/font/connect self; img+`data:`; object-src none; base-uri/frame-ancestors/
  form-action self) más `X-Content-Type-Options: nosniff`, `Referrer-Policy: no-referrer` y `X-Frame-Options:
  SAMEORIGIN`. Registrado tras `UseHttpsRedirection` para cubrir estáticos e Identity. Quitado el `<script
  type="importmap">` vacío del layout. Auditoría: 0 inline / 0 CDN en las vistas propias; verificado en local
  (cabecera en home/login/back; login+Socios+Economia 200; escaneo de 5 páginas servidas = 0 inline). Fija las
  reglas ANTES de escribir el front público (que se montará CSP-safe). Desplegado a producción tras verificar.
- **2026-07-05 (tras validación del usuario) — Fix doble-click IBAN + gestión de colaboraciones desde Edit**:
  el usuario probó el núcleo en producción (5 gráficas + editar colaboración OK) y pidió dos mejoras, ya
  hechas, commiteadas (`b95e146`) y **DESPLEGADAS a producción** (verificado por HTTP: `data-val-iban` +
  `colaboracion-form.js` en el form de colaboración, tabla de colaboraciones en Edit del socio). (1)
  **Doble-click al guardar IBAN**: el IBAN solo validaba en
  servidor, el mensaje de error persistía hasta una interacción y el primer submit se "gastaba" limpiándolo.
  Corregido con **validación de cliente real**: atributo `[Iban]` (`IClientModelValidator`, ya existía) en los
  ViewModels de Create/Edit de colaboración + adaptador jquery-validation `iban` en `colaboracion-form.js`
  (lógica mod-97 replicada 1:1 del servidor, paridad verificada con 10 casos en Node), cargado también en
  Edit. (2) **Gestión de colaboraciones desde la edición del socio**: la tabla ver/crear/editar/baja (que solo
  estaba en la ficha) se extrajo a un partial reutilizable `_TablaColaboraciones` usado en Details (elimina
  duplicación) y en Edit; `Edit.cshtml.cs` carga las colaboraciones y tiene su handler de baja. Baja lógica,
  sin borrado físico (conserva histórico). **93 tests verdes**, build OK. Verificado por HTTP (admin):
  `data-val-iban` en Create y Edit, tabla en Edit con acciones, POST de baja desde Edit OK. Detalle en el log
  semanal y en el commit.
- **2026-07-05 (cierre de finde) — Editar colaboración + previsión + DESPLIEGUE de todo el núcleo**: tras
  probar el módulo, el usuario pidió (a) **editar colaboración** (no solo borrar) y (b) una **previsión
  ingresos vs gastos**. Hecho: `ColaboracionService.ActualizarAsync` (TDD; importe/periodicidad/IBAN, no tipo
  ni socio) + página `Colaboraciones/Edit` + enlace por fila; `ResumenEconomicoService.ProyectarAsync` (TDD;
  proyección "si todo sigue igual", no predicción) como 5ª gráfica (líneas 2 series, 6 meses; `dashboard.js`
  ampliado a multi-serie). **93 tests verdes.** Antes, fixes reportados por el usuario: **combo de país**
  (input+datalist, el select+buscador fallaba) y **selects de enum** (comparaban por nombre pero emiten valor
  numérico → los campos de cuota no aparecían y el DNI no validaba en vivo; resuelto con `data-*`).
  **Desplegado todo a producción** (RuntimeSuccessful, verificado: `/Admin/Economia` 200, 5 canvas, Chart.js
  servido). El usuario iba a hacer pruebas online. Detalle en `context/decisions.md`.
- **2026-07-05 (noche) — Dashboards (cuarto módulo del MVP)**: 4 gráficas en `/Admin/Economia` con **Chart.js
  servido en local** (no CDN → CSP-safe): donut de ingresos por tipo, barras ingresos/gastos/balance, barras
  de gastos por categoría y líneas de altas por mes. Datos serializados a JSON en `data-chart` de cada
  `<canvas>` y pintados por `dashboard.js` externo (sin inline). Nueva agregación `GastosPorCategoria` por
  TDD. El módulo económico se **desplegó** antes de esto (verificado en prod). 86 tests verdes. Verificado sin
  navegador (4 canvas, Chart.js 200, JSON parseable, JS sin errores); Playwright bloqueado por el entorno →
  **pendiente validación visual del usuario** + desplegar dashboards. Ver `context/decisions.md`.
- **2026-07-05 (noche) — Módulo económico (tercer módulo del MVP)**: ingresos (desde colaboraciones) + gastos
  + balance. Entidad `Gasto` (concepto/importe/fecha/`CategoriaGasto` genérica de ONG; borrado físico; migración
  `AddGasto`) con CRUD (`IGastoService`). **Cálculo por TDD** (`ResumenEconomicoService`, 6 tests): ingreso
  recurrente mensual (solo cuotas domiciliadas activas, anual/12), ingresos por tipo, socios activos con
  colaboración activa, altas por mes, y balance = ingresos−gastos. Página `/Admin/Economia`: métricas en
  cards, ingresos por tipo, alta/borrado de gastos y **vista global de colaboraciones** (la pospuesta). Card
  de acceso en el panel. **85 tests verdes.** E2E verificado por HTTP (recurrente 20€ con anual/12, ingresos
  630€, balance 430€). Los dashboards con gráficas quedan para el siguiente bloque. **SIN desplegar.** Ver
  `context/decisions.md`.
- **2026-07-05 (tarde) — CRUD de Colaboraciones desplegado**: el segundo módulo (abajo) se desplegó a
  producción y se verificó por HTTP (alta de cuota domiciliada + ficha con IBAN). Socios + Colaboraciones
  vivos en https://dididai-ong.azurewebsites.net.
- **2026-07-05 (noche) — CRUD de Colaboraciones (segundo módulo del MVP)**: gestión de las aportaciones de un
  socio (cuota domiciliada / aportación única / Teaming, jerarquía TPH ya existente). **IBAN validado por TDD**
  (`ValidacionIban`, mod-97 ISO 13616, internacional; 17 tests rojo→verde) + atributo `[Iban]`
  (`IClientModelValidator`, cliente=servidor, mismo patrón que DNI/teléfono). Capa de servicios
  `IColaboracionService`/`ColaboracionService` (patrón de socios: la página no toca el DbContext); reglas:
  socio existe, importe>0, IBAN válido solo si cuota domiciliada; **baja lógica** idempotente ("dejar de pagar"
  conservando histórico, no borra). Cubierta con **tests de integración** (SQLite en memoria). UI: gestión
  **desde la ficha del socio** (Details lista + botón añadir + baja por fila); alta en un formulario con
  **selector de tipo** y campos de cuota que se muestran/ocultan por JS externo (CSP); ViewModel plano que
  construye el subtipo TPH en el POST. Vista global pospuesta al módulo económico. Sin migración (solo
  atributos de validación). **79 tests verdes** en total. E2E por HTTP (alta 3 tipos, rechazos de IBAN/importe,
  baja efectiva). **SIN desplegar.** Ver `context/decisions.md`.
- **2026-07-05 (tarde/noche) — Frente 1: validación de identidad por tipo de documento + país=residencia +
  teléfono E.164 + paridad cliente/servidor**: refinamiento del CRUD de socios para una base internacional.
  **Tres datos separados**: `PaisResidencia` (ISO 3166-1 alpha-2, domicilio, NO valida), `TipoDocumento` (enum
  DNI español/NIE/Pasaporte/Otro — **decide** cómo se valida el documento) y `Dni` (validado según tipo:
  DNI/NIE con letra de control, pasaporte/otro laxo). Resuelve el caso **"español residente en UK"** (declara
  DNI español → se valida la letra, aunque resida fuera). **Teléfono E.164** con UI de prefijo (select) +
  número (un solo campo en la entidad; la UI lo parte y recompone por JS externo). **Validación
  cliente=servidor sin duplicar regla**: atributos `IClientModelValidator` (`[TelefonoE164]`,
  `[DocumentoPorTipo]`) que validan en servidor y emiten `data-val-*`; adaptadores jquery-validation en
  `validacion-socio.js` (CSP-safe) que aplican la misma regla en vivo y revalidan al cambiar el tipo.
  Catálogos `Paises` y `PrefijosTelefonicos` **en código** (no BD; validez garantizada por desplegable +
  servidor). Core pasa a referenciar `Microsoft.AspNetCore.App` (ya dependía vía Identity). Migración única
  `SocioResidenciaYTipoDocumento` (drop `Pais` + add ambas columnas). **Verificado**: servidor 8 casos (incl.
  español-en-UK aceptado; DNI/NIE inválidos, tel sin prefijo y residencia inexistente rechazados con mensaje);
  cliente (paridad de lógica + glue del teléfono) probado en Node contra el fichero real (Playwright bloqueado
  por el entorno). Build OK. **SIN commitear/desplegar al cerrar esta nota.** Ver `context/decisions.md`.
- **2026-07-05 (tarde) — Infra de internacionalización (i18n) del front público**: localización estándar de
  ASP.NET Core (`AddViewLocalization` + `.resx` en `Resources/` + `RequestLocalizationMiddleware`). Idioma
  elegido por **selector en la cabecera** que persiste en **cookie** (`CookieRequestCultureProvider`); `es`
  por defecto. Página `SetLanguage` (POST con **antiforgery**, valida contra el catálogo de culturas, sin
  open-redirect) fija la cookie. `_Layout` e `Index` localizados (ES neutro + `.en`) como prueba. Selector
  CSP-safe: auto-submit por **JS externo** (`data-lang-select` en `site.js`), sin inline. **Solo front
  público**; el back `/Admin` queda en español a propósito. **Extensible a N idiomas**: añadir un idioma =
  una entrada en la lista de culturas de `Program.cs` + su `.resx`, sin `if (idioma=="en")` en el código.
  **Decisión transversal registrada:** cultura de UI, país del socio y validación de datos son ejes
  independientes; la validación (DNI/teléfono/IBAN) irá por **país** (`PaisCodigo` ISO), no por idioma —
  Frente 1 pendiente. **Verificado end-to-end** (default ES; cookie EN conmuta textos y `lang`; `/Admin` no
  afectado; POST sin token → 400). Build OK. **SIN commitear/desplegar aún.** Ver `context/decisions.md`.
- **2026-07-05 (madrugada) — CRUD de gestión de socios (primer módulo del MVP)**: en `/Admin/Socios`. Capa
  de servicios en Core (`ISocioService`/`SocioService`; las páginas no tocan el `DbContext`). Páginas Razor:
  listado con búsqueda + toggle "incluir bajas", ficha, alta, edición. **Baja lógica** (`Socio.FechaBaja`) +
  **reactivación**. **DNI único** (índice BD + servidor, normalizado); **Email NO único** (familias/gestores
  comparten correo). Consentimiento RGPD obligatorio en el alta. **Validación universal, no España-only** por
  el carácter internacional de la base de socios (el director trabajó años en UK) — decisión deliberada, ver
  `decisions.md`. Migración `AddSocioBajaAndDniIndex`. Frontend Bootstrap responsive, sin inline (CSP:
  confirmación de baja por JS externo). Seguridad revisada: EF parametriza (no SQLi), Razor escapa (no XSS),
  antiforgery (CSRF). **Verificado end-to-end en local** (alta, listado, DNI duplicado rechazado, alta sin
  RGPD rechazada, edición, baja, reactivación). Commiteado. **SIN DESPLEGAR aún.** Idea abierta del usuario:
  web bilingüe ES/EN con validación por idioma (ver Immediate Risks / decisions).
- **2026-07-04 (tarde/noche) — Despliegue en producción CERRADO (B1 / Spain Central)**: la web quedó viva y
  estable en **https://dididai-ong.azurewebsites.net**, verificada end-to-end (home 200, `/Admin` anónimo
  302, login admin 302, `/Admin` autenticado 200). Se **abandonó F1**: al reanudar, la app volvió a caer en
  `QuotaExceeded` al arrancar (F1 se cae en cada cold start), y como la corrección del TFM puede tardar >1
  mes se necesitaba estabilidad sostenida → **B1** (sin cuota, no duerme), pagado con el crédito Free Trial.
  Infra recreada en **`spaincentral`** por **RGPD** (datos en territorio nacional; además francecentral no
  tenía capacidad B1). Francia (`dididai-web`/F1) se conserva como respaldo; webapp nueva `dididai-ong` +
  plan `plan-dididai-es`. **Bug de arranque corregido:** `Program.cs` sembraba el admin sin migrar la BD → en
  Azure (`/home` vacío) petaba; fix: `Database.MigrateAsync()` antes del seed. Alerta de presupuesto creada
  (30 €/mes). Azure SQL reconsiderado y descartado (más coste/riesgo; SQLite cubre el MVP). **Pendiente del
  usuario: convertir la suscripción a Pago por uso** antes de que caduque el crédito (~agosto). Detalle en
  `context/decisions.md` y runbook actualizado en `context/deploy-azure.md`.
- **2026-07-04 — README del TFM**: README completo en español, verificado contra el enunciado (cubre los 6
  apartados obligatorios + arquitectura, modelo de datos, despliegue, seguridad, roadmap). Infra Azure creada
  inicialmente en F1 con cuenta personal `dididai@outlook.es`; escollos de entorno resueltos (Norton
  interceptaba TLS → exclusiones; la cuenta del trabajo no servía → cuenta personal dedicada).
- **2026-07-04 — Autenticación (ASP.NET Core Identity)**: back de gestión cerrado con Identity (Default UI +
  roles) sobre el `AppDbContext` (migración `AddIdentity`). Login, logout y recuperación de contraseña (con
  `IEmailSender` **stub** que loguea el enlace en vez de enviarlo). **Registro público deshabilitado** por
  middleware (404 salvo rol Admin). Zona `/Admin` protegida por rol; enlace de acceso en el layout. Seed de
  rol `Admin` + usuario admin con credenciales en **User Secrets** (nunca en el repo). El **front público
  sigue abierto sin login**. Verificado end-to-end por HTTP (la verificación destapó y corrigió dos bugs:
  faltaba `.AddDefaultUI()` y las rutas de cookie). Detalle en `context/decisions.md`.
- **2026-07-04 — Primer código: arquitectura multi-proyecto + capa de datos**: creada la solución
  `DididaiApp.sln` con `DididaiApp` (web/presentación) + `DididaiApp.Core` (dominio + datos + servicios).
  Modelo de datos disociando persona de pago: `Socio` (identidad) 1:N `Colaboracion` (aportación, jerarquía
  Table-Per-Hierarchy: `CuotaDomiciliada` / `AportacionUnica` / `Teaming`), modelado sobre el formulario real
  `/form`. EF Core 10 + SQLite: `AppDbContext`, migración `InitialCreate` aplicada, `dididai.db` creada e
  ignorada por git (RGPD). Detectada y aceptada (documentada, sin parche disponible) la vulnerabilidad
  transitiva NU1903/CVE-2025-6965 de SQLite. Detalle en `context/decisions.md` (3 decisiones del 04-07).
- **2026-07-03 — Alcance del TFM y stack de datos/deploy definidos**: acordado el MVP (web pública + login/
  roles + socios + económico simple + dashboards), con CMS y contabilidad avanzada como roadmap. Persistencia
  EF Core + SQLite; despliegue en Azure App Service F1 (coste cero). Deadline 20/07/2026. Detalle en
  `context/decisions.md`.
- **2026-07-03 — Acceso GitHub con cuenta dedicada**: configurado SSH con clave dedicada
  (`id_ed25519_dididai`, con passphrase) y alias de host `github-dididai` para usar la cuenta `Sk0t0h` solo
  en este repo, sin afectar a la identidad/credenciales del resto de proyectos del usuario. Identidad git
  local del repo puesta a gmail; global intacta. Primer push a `Sk0t0h/dididai` (rama `main`). Licencia MIT
  a nombre de DIDIDAI.
- **2026-07-03 — Inicialización del sistema de memoria del repo**: creada la estructura de memoria por capas
  (este `ORACULO.md`, `CLAUDE.md`, `ai-context.md`, `context/`, `logs/`).

## Immediate Risks

> Estado: ~~tachado~~ = RESUELTO. Resto: CRÍTICO / ALTO / MEDIO + PENDIENTE / CONFIRMADO.

- **Plazo (deadline 20/07)** — BAJO/MEDIO · MUY MEJORADO (05-07): el **núcleo funcional del MVP está completo
  y desplegado** (socios, colaboraciones, económico, dashboards). Queda solo el **front público + look & feel**
  (contenido/diseño, no lógica) y los entregables no-código (README/slides/vídeo). El riesgo dominante ya no
  es alcance funcional sino tiempo para pulir front + grabar. Colchón amplio hasta el 20/07.
- ~~Crédito Azure caduca en ~30 días (Free Trial)~~ — RESUELTO (05-07): suscripción convertida a **Pago por
  uso** (`quotaId = PayAsYouGo`), ya no caduca; la web no se apagará. Coste controlado: se agota primero el
  crédito (~175 €), luego ~13 €/mes de B1, topado por tarjeta virtual + alerta de presupuesto (30 €/mes).
- **Vulnerabilidad transitiva SQLite NU1903 / CVE-2025-6965** — MEDIO · ACEPTADO Y VIGILADO (no explotable en
  nuestra superficie —sin SQL arbitrario— y sin parche disponible a 04-07. Revisar antes del deploy; ver
  `context/decisions.md`).
- Repo público sin gestión de secretos establecida — MEDIO · MITIGADO EN CURSO (la connection string es solo
  la ruta del `.db`; **las credenciales del seed admin ya van por User Secrets**, no en el repo. Al desplegar,
  llevarlas a variables de entorno en Azure. El riesgo crece con API keys —email real, etc.).
- ~~Sin CSP activa pese a la disciplina anti-inline~~ — RESUELTO (05-07): `SecurityHeadersMiddleware` emite una
  CSP estricta `default-src 'self'` sin `unsafe-inline` en toda la app; verificado que no rompe front, login ni
  back. El front público se montará respetándola.
- ~~Formulario público→BD~~ — RESUELTO (07-07): implementado con entidad aparte `SolicitudColaboracion` (el
  admin revisa, no alta directa de Socio) + antiforgery + honeypot + rate-limit por IP (solo POST) + validación
  server + consentimiento RGPD + mensaje neutro. **IBAN nunca en el form público.** Verificado E2E (incl.
  honeypot descarta el bot). Falta: repaso de seguridad final antes de desplegar (p. ej. `/security-review`).
- **Front público sin commitear/desplegar** — BAJO · PENDIENTE (07-07): todo el módulo está en local, verificado,
  103 tests verdes, pero **sin commit ni deploy**. Al desplegar, la migración `AddSolicitudColaboracion` se
  aplica sola al arrancar (aditiva). Traducir EN pendiente (contenido ES puesto; EN cae a ES por fallback).
- Datos personales de socios en repo/BD (RGPD) — MEDIO · MITIGADO PARCIALMENTE (`*.db` en `.gitignore`; falta
  garantizar datos anonimizados en la demo. `Dni`/`Iban` sin cifrar a nivel de columna: fuera de MVP).
- ~~Alcance funcional sin especificar~~ — RESUELTO (MVP definido 2026-07-03).

## Modelo mental de funcionamiento

Aplicación ASP.NET Core Razor Pages con solución de **dos proyectos**: `DididaiApp` (web/presentación, sirve
páginas renderizadas en servidor; punto de entrada y pipeline en `Program.cs`) y `DididaiApp.Core` (biblioteca
de dominio + datos + servicios). La web referencia a Core e inyecta sus servicios; **no** accede al
`DbContext` directamente. Persistencia con EF Core sobre SQLite (`AppDbContext` en Core, `dididai.db` local).
Autenticación con ASP.NET Core Identity (usuarios/roles en el mismo `AppDbContext`): front público abierto y
back `/Admin` cerrado por rol. Sin servicios externos aún (email = stub). Flujo: petición HTTP → página Razor →
(servicio Core →
`AppDbContext` → SQLite) → respuesta HTML. En el arranque, `Program.cs` aplica migraciones
(`Database.MigrateAsync()`) y siembra el admin. **Desplegado** en Azure App Service B1 (Spain Central); la
SQLite vive en `/home` (persistente). URL pública: https://dididai-ong.azurewebsites.net

## Invariantes críticos (NO romper)

- **No subir secretos al repo** (es público). Ver `CLAUDE.md`. En particular, las **credenciales del seed
  admin** (`Seed:AdminEmail` / `Seed:AdminPassword`) van por User Secrets (dev) / variables de entorno (prod),
  **nunca** en `appsettings.json`.
- No commitear con la identidad global del usuario: este repo usa identidad local gmail + remote por alias
  `github-dididai`.
- **El front público NO debe requerir login** (requisito de negocio). Solo `/Admin` y el registro de usuarios
  van protegidos.

## Áreas frágiles / caveats

- Configuración SSH/git específica de este repo (alias de host, `core.sshCommand` local). Si se clona en otra
  máquina hay que replicar la clave y el alias; ver `CLAUDE.md` y `context/decisions.md`.
- **Registro de usuarios bloqueado por middleware** en `Program.cs` (no por convención de página, que no
  funciona con la Default UI de Identity). Si se cambia el pipeline, no reabrir el auto-registro público.
- **Seed admin:** si al arrancar no hay `Seed:*` en configuración, el admin no se crea (warning en log) y no
  se podrá entrar al back. En una máquina nueva hay que poner los User Secrets.

## Orden de arranque

```
cd DididaiApp
dotnet run
```
App en `https://localhost:7080` (o `http://localhost:5110`).

Migraciones EF (el `DbContext` está en Core, el startup es Web):
```
dotnet ef migrations add <Nombre> --project DididaiApp.Core --startup-project DididaiApp
dotnet ef database update        --project DididaiApp.Core --startup-project DididaiApp
```

## Punteros a otros ficheros

- Estado de trabajo actual → `ai-context.md`
- Decisiones → `context/decisions.md`
- **Despliegue (runbook paso a paso) → `context/deploy-azure.md`**
- Próximos pasos → `context/next-steps.md`
- Estructura/baseline técnico → `context/project-overview.md`
- Reglas operativas estables → `CLAUDE.md`

## Transfer instructions

Este fichero se puede compartir con otro agente/IA cuando:
- se empieza a trabajar en el proyecto en una sesión nueva fuera del repo,
- se transfiere contexto de un agente a otro,
- se quiere que una IA entienda el proyecto sin acceso al código.

Al compartir: enviar `ORACULO.md` (núcleo). Opcional: `context/decisions.md`, `context/next-steps.md`,
`ai-context.md`, `context/project-overview.md`. **NO** enviar: logs ni secretos/config sensible.
