# DIDIDAI.ORG — ORACULO

> Documento de **bootstrapping para agentes**. Su función es transferir el contexto y el **estado actual**
> del proyecto a otra sesión o a otro agente, **incluso sin acceso al código** (p. ej. para pegárselo a otra
> IA como Perplexity). Es la verdad de base ("ground truth") para el entendimiento estratégico. Redacción
> autoexplicativa: evitar jerga interna o abreviaturas que no se entiendan sin ver el repositorio.
>
> **Mantenimiento:** regenerar al cerrar cada bloque de trabajo sustancial (Active Focus + Module Status +
> Latest Work + Immediate Risks). Última actualización: 2026-07-05 (tarde).

## Active Focus

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
**ÚNICO MÓDULO QUE QUEDA: el front público + look & feel** (mobile-first, marca DIDIDAI) + traducir EN el
contenido. Después, entregables no-código (README/slides/vídeo). Detalle en `context/next-steps.md` y
`context/decisions.md`.

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
| Autenticación + roles (Identity, back cerrado) | IMPLEMENTADO (04-07, verificado): login, recuperación (email stub), registro solo Admin, `/Admin` protegido, seed admin |
| **Despliegue en producción (Azure App Service B1, Spain Central)** | **OPERATIVO (04-07)**: https://dididai-ong.azurewebsites.net, verificado end-to-end; migración+seed en arranque |
| Capa de servicios (Core `Services/`) | IMPLEMENTADO (05-07): `ISocioService`/`SocioService`; páginas no tocan `DbContext`. Nuevos módulos siguen el patrón |
| Internacionalización (i18n) front público | IMPLEMENTADO (05-07, verificado): infra ES/EN por selector+cookie, extensible a N idiomas, `es` por defecto. Solo front; `/Admin` en español. Contenido real por traducir |
| Front público (home, quiénes somos, contacto) | PLANIFICADO (MVP) — UI mobile-first, contenido localizable |
| Tests unitarios (`DididaiApp.Tests`, xUnit) | IMPLEMENTADO (05-07): 55 tests verdes sobre `ValidacionIdentidad` (DNI/NIE/E.164), `Paises`, `PrefijosTelefonicos`. `dotnet test` |
| Gestión de socios (CRUD) | OPERATIVO (05-07, DESPLEGADO y verificado en prod): alta/listado/ficha/edición, baja lógica+reactivar, DNI único, Email no único. **Validación por TIPO de documento** (DNI/NIE letra, pasaporte/otro laxo), **país=residencia** (ISO, desplegable+buscador), **teléfono E.164** (prefijo+número), **cliente=servidor** (atributos IClientModelValidator + adaptadores jquery-validation) |
| Gestión de Colaboraciones (CRUD) | OPERATIVO (05-07, DESPLEGADO): alta (3 tipos, form con selector), **editar** (importe/periodicidad/IBAN), listado y baja lógica desde la ficha del socio; IBAN mod-97 (TDD) + `[Iban]` cliente=servidor; servicio en Core con tests de integración |
| Módulo económico simple (ingresos/gastos) | OPERATIVO (05-07, DESPLEGADO): entidad `Gasto` (CRUD, categorías ONG), servicio de resumen por TDD (recurrente mensual, ingresos por tipo, socios con colaboración, altas/mes, balance, **previsión**), página `/Admin/Economia` con vista global de colaboraciones |
| Dashboards / informes visuales | OPERATIVO (05-07, DESPLEGADO): **5 gráficas** Chart.js servido local en `/Admin/Economia` (donut ingresos por tipo, barras ingresos/gastos/balance, barras gastos por categoría, líneas altas/mes, **líneas previsión 6 meses ingresos vs gastos**); datos por `data-chart` + `dashboard.js` externo (CSP-safe, multi-serie). Pendiente solo validación visual del usuario |
| Gestor de contenido (CMS) | ROADMAP (fuera de MVP) |
| Contabilidad avanzada | ROADMAP (fuera de MVP) |

## Latest Work

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
