# DIDIDAI.ORG — ORACULO

> Documento de **bootstrapping para agentes**. Su función es transferir el contexto y el **estado actual**
> del proyecto a otra sesión o a otro agente, **incluso sin acceso al código** (p. ej. para pegárselo a otra
> IA como Perplexity). Es la verdad de base ("ground truth") para el entendimiento estratégico. Redacción
> autoexplicativa: evitar jerga interna o abreviaturas que no se entiendan sin ver el repositorio.
>
> **Mantenimiento:** regenerar al cerrar cada bloque de trabajo sustancial (Active Focus + Module Status +
> Latest Work + Immediate Risks). Última actualización: 2026-07-05.

## Active Focus

Deadline **20/07/2026**. Producto: web taylor-made para la ONG DIDIDAI con front público + back de gestión
cerrado. **MVP:** web pública · login/roles · gestión de socios · módulo económico simple · dashboards.
Stack: EF Core + SQLite → Azure App Service. **FINDE 1 CERRADO** ("esqueleto vivo y desplegado"): hechas
arquitectura multi-proyecto (Web + Core), capa de datos (EF Core + SQLite, `Socio` + `Colaboracion` TPH),
**autenticación** (Identity con roles, verificada), **README** (completo, verificado contra el enunciado) y
**DESPLIEGUE en producción, estable y verificado end-to-end**: la web está viva en
**https://dididai-ong.azurewebsites.net** (Azure App Service **B1**, región **Spain Central** por RGPD). Se
abandonó F1 (se caía al arrancar por `QuotaExceeded`) por B1 sin cuota, financiado por el crédito. Front
público abierto. **Siguiente foco (Finde 2): MVP funcional — CRUD de gestión de socios** (arrastra crear la
capa de servicios en Core). Detalle en `context/next-steps.md`.

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
| Capa de servicios (Core `Services/`) | PLANIFICADO (crear con el CRUD; páginas no tocan `DbContext` directo) |
| Front público (home, quiénes somos, contacto) | PLANIFICADO (MVP) — UI mobile-first |
| Gestión de socios (CRUD) | PLANIFICADO (MVP) |
| Módulo económico simple (ingresos/gastos) | PLANIFICADO (MVP) — ingresos saldrán de `Colaboracion` |
| Dashboards / informes visuales | PLANIFICADO (MVP) |
| Gestor de contenido (CMS) | ROADMAP (fuera de MVP) |
| Contabilidad avanzada | ROADMAP (fuera de MVP) |

## Latest Work

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

- **Plazo ajustado: quedan ~15 días para MVP funcional + slides + vídeo** — ALTO · PENDIENTE (el riesgo
  dominante es alcance vs tiempo; mitigación: MVP recortado y roadmap para el resto. El esqueleto y el
  despliegue ya están; falta el producto: CRUD socios + económico + dashboards).
- **Crédito Azure caduca en ~30 días (Free Trial)** — ALTO · PENDIENTE · **ACCIÓN DEL USUARIO**: si no se
  convierte la suscripción a Pago por uso, al caducar el crédito la suscripción se deshabilita y la web se
  apaga — posiblemente en pleno periodo de corrección. Mitigación: convertir a Pago por uso + alerta de
  presupuesto ya creada (30 €/mes).
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
