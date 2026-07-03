# Project Overview — DIDIDAI.ORG

> Baseline estructural. Más estable que `ai-context.md`, menos táctico que `ORACULO.md`.
> Actualizado: 2026-07-03.

## 0. Qué es

Web taylor-made para la ONG local **DIDIDAI** (sustituye una web básica previa). Dos caras: **front público**
informativo + **back de gestión cerrado** con login. Es el TFM del Máster de Desarrollo con IA (BIG School),
deadline 20/07/2026. MVP: web pública · auth/roles · socios · económico simple · dashboards. Roadmap: CMS,
contabilidad avanzada.

## 1. Stack y layout

- **Lenguaje/framework:** C# · ASP.NET Core **Razor Pages** · **.NET 10.0**.
- **Persistencia:** EF Core + **SQLite** (decidido; aún sin implementar).
- **Auth:** ASP.NET Core Identity + roles (decidido; aún sin implementar).
- **Despliegue:** Azure App Service **F1** (gratuito).
- `Nullable` e `ImplicitUsings` habilitados.

| Ruta | Responsabilidad |
|------|-----------------|
| `DididaiApp/DididaiApp.csproj` | Proyecto web (Sdk `Microsoft.NET.Sdk.Web`) |
| `DididaiApp/Program.cs` | Entry point y configuración del pipeline HTTP |
| `DididaiApp/Pages/` | Páginas Razor (Index, Privacy, Error) + `Shared/_Layout` |
| `DididaiApp/wwwroot/` | Estáticos: `css/`, `js/`, `lib/` (Bootstrap, jQuery, jquery-validation) |
| `DididaiApp/appsettings*.json` | Configuración (SIN secretos — repo público) |
| Memoria del repo (raíz) | `ORACULO.md`, `ai-context.md`, `CLAUDE.md`, `context/`, `logs/` |

## 2. Topología en runtime

Monolito web de un solo proceso ASP.NET Core (Kestrel). Front público + back con login sobre el mismo app.
BD SQLite en fichero local (viaja con la app). Sin procesos background ni servicios externos. Puertos dev:
https 7080 / http 5110. En producción: Azure App Service F1.

## 3. Persistencia / datos

EF Core + SQLite (fichero). Entidades previstas para el MVP (por refinar):
- `Socio` — datos del socio, estado (alta/baja), fecha de alta.
- `MovimientoEconomico` — ingreso/gasto: fecha, categoría, importe, concepto; relación opcional con `Socio`.
- Identidad (`Usuario`/roles) — vía ASP.NET Core Identity.

**RGPD:** los socios son datos personales. En la demo pública usar datos de ejemplo/anonimizados; no subir BD
con datos reales al repo público.

## 4. Integraciones externas

Ninguna en el MVP. (Formulario de contacto del front: definir si envía email o solo persiste.)

## 5. Convenciones y zonas sensibles

- **Secretos fuera del repo** (público): User Secrets en dev, variables de entorno en prod.
- **Frontend sin inline (CSP):** estilos a clases, comportamiento a JS externo; responsive. Aplica también a
  la librería de gráficas del dashboard (elegir una compatible con CSP).
- **Formularios OWASP:** validación en servidor + antiforgery/CSRF.
- **Datos personales (RGPD):** ver punto 3.
- **Git:** identidad local gmail + remote por alias `github-dididai` (ver `context/decisions.md`).
