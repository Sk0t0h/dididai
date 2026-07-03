# DIDIDAI.ORG — ORACULO

> Documento de **bootstrapping para agentes**. Su función es transferir el contexto y el **estado actual**
> del proyecto a otra sesión o a otro agente, **incluso sin acceso al código** (p. ej. para pegárselo a otra
> IA como Perplexity). Es la verdad de base ("ground truth") para el entendimiento estratégico. Redacción
> autoexplicativa: evitar jerga interna o abreviaturas que no se entiendan sin ver el repositorio.
>
> **Mantenimiento:** regenerar al cerrar cada bloque de trabajo sustancial (Active Focus + Module Status +
> Latest Work + Immediate Risks). Última actualización: 2026-07-03.

## Active Focus

Alcance del TFM definido (2026-07-03). Deadline **20/07/2026**. Producto: web taylor-made para la ONG DIDIDAI
con front público + back de gestión cerrado. **MVP acordado:** web pública · login/roles · gestión de socios
· módulo económico simple · dashboards. Stack de datos y deploy fijados (EF Core + SQLite → Azure App Service
F1). **Aún sin código de producto**: siguiente paso es empezar la implementación (modelo de datos + auth).

## Propósito real

Web para la ONG local **DIDIDAI**, sustituyendo una web básica anterior. Dos caras: un **front público**
(informativo) y un **back de gestión cerrado** (privado, con login) para que la ONG administre su día a día
sin depender de un CMS genérico tipo WordPress/Joomla — solución **a medida**. Es el TFM del Máster de
Desarrollo con IA (BIG School); repo público; licencia MIT a nombre de DIDIDAI.

**MVP a entregar (TFM):** web pública · autenticación con roles · gestión de socios (CRUD) · módulo económico
simple (ingresos/gastos) · informes visuales (dashboards).
**Roadmap (post-TFM):** gestor de contenido (CMS) completo, contabilidad avanzada.

## Module Status

> Estados: OPERATIVO (en uso) · IMPLEMENTADO (en código, uso no confirmado) · INFERRED (deducido del código,
> por validar con negocio) · PLANIFICADO (acordado, sin código aún).

| Módulo | Estado |
|--------|--------|
| Web shell (Razor Pages: Index, Privacy, Error) | IMPLEMENTADO (plantilla por defecto, sin contenido propio) |
| Front público (home, quiénes somos, contacto) | PLANIFICADO (MVP) |
| Autenticación + roles (back cerrado) | PLANIFICADO (MVP) |
| Gestión de socios (CRUD) | PLANIFICADO (MVP) |
| Módulo económico simple (ingresos/gastos) | PLANIFICADO (MVP) |
| Dashboards / informes visuales | PLANIFICADO (MVP) |
| Persistencia (EF Core + SQLite) | PLANIFICADO (base de todo lo anterior) |
| Gestor de contenido (CMS) | ROADMAP (fuera de MVP) |
| Contabilidad avanzada | ROADMAP (fuera de MVP) |

## Latest Work

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

- **Plazo ajustado: 17 días para MVP + README + despliegue + slides + vídeo** — ALTO · PENDIENTE (el riesgo
  dominante es alcance vs tiempo; mitigación: MVP recortado y roadmap para el resto).
- Repo público sin gestión de secretos establecida — MEDIO · PENDIENTE (hoy no hay secretos; el riesgo
  aparece cuando se añadan connection strings / API keys. Mitigación acordada: User Secrets + variables de
  entorno, nunca literales en `appsettings.json`).
- Datos personales de socios en repo/BD (RGPD) — MEDIO · PENDIENTE (usar datos de ejemplo/anonimizados en la
  demo pública; no subir BD con datos reales).
- ~~Alcance funcional sin especificar~~ — RESUELTO (MVP definido 2026-07-03).

## Modelo mental de funcionamiento

Aplicación monolítica ASP.NET Core Razor Pages. Un único proyecto web (`DididaiApp`) que sirve páginas
renderizadas en servidor. Sin capa de datos ni servicios externos por ahora: todo el flujo es
petición HTTP → página Razor → respuesta HTML. El punto de entrada y configuración del pipeline está en
`Program.cs`.

## Invariantes críticos (NO romper)

- **No subir secretos al repo** (es público). Ver `CLAUDE.md`.
- No commitear con la identidad global del usuario: este repo usa identidad local gmail + remote por alias
  `github-dididai`.

## Áreas frágiles / caveats

- Configuración SSH/git específica de este repo (alias de host, `core.sshCommand` local). Si se clona en otra
  máquina hay que replicar la clave y el alias; ver `CLAUDE.md` y `context/decisions.md`.

## Orden de arranque

```
cd DididaiApp
dotnet run
```
App en `https://localhost:7080` (o `http://localhost:5110`).

## Punteros a otros ficheros

- Estado de trabajo actual → `ai-context.md`
- Decisiones → `context/decisions.md`
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
