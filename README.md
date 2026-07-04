# DIDIDAI.ORG

Aplicación web a medida para la ONG **DIDIDAI**: un **front público** informativo abierto y un
**back de gestión cerrado** (privado, con login) para administrar el día a día de la organización sin
depender de un gestor de contenidos genérico.

> **Trabajo Fin de Máster** — Máster de Desarrollo con IA (BIG School).
> Repositorio público · Licencia MIT a nombre de DIDIDAI · .NET 10 · ASP.NET Core Razor Pages.

---

## Índice

1. [Descripción del proyecto](#descripción-del-proyecto)
2. [Funcionalidades](#funcionalidades)
3. [Stack tecnológico](#stack-tecnológico)
4. [Arquitectura](#arquitectura)
5. [Modelo de datos](#modelo-de-datos)
6. [Estructura del repositorio](#estructura-del-repositorio)
7. [Instalación y ejecución](#instalación-y-ejecución)
8. [Configuración y secretos](#configuración-y-secretos)
9. [Despliegue](#despliegue)
10. [Seguridad](#seguridad)
11. [Acceso de prueba](#acceso-de-prueba)
12. [Enlaces del proyecto](#enlaces-del-proyecto)
13. [Roadmap](#roadmap)
14. [Licencia](#licencia)

---

## Descripción del proyecto

**DIDIDAI** es una ONG de derechos infantiles y educación especial que opera un orfanato en Katmandú
(Nepal) para menores con discapacidad, ofreciendo recursos educativos, estimulación multisensorial y apoyo
terapéutico. Su mensaje de marca es que **el 99 % de los ingresos va a acción directa**.

Este proyecto sustituye la web actual de la ONG por una **solución propia a medida**, con dos caras
claramente separadas:

- **Front público (abierto):** presenta la organización —misión, actividad, filosofía, objetivos y
  contacto— y permite a las personas interesadas conocer y colaborar con la ONG.
- **Back de gestión (privado, con autenticación):** permite al equipo de la ONG administrar su día a día
  —socios, control económico simple e informes visuales— sin depender de un CMS genérico tipo
  WordPress/Joomla.

La transparencia económica (el mensaje del "99 % a acción directa") es un eje del proyecto: por eso el
módulo económico y los informes visuales tienen un papel destacado en el back de gestión.

## Funcionalidades

Alcance comprometido para el MVP del TFM:

| Funcionalidad | Estado |
|---|---|
| Autenticación con roles (back cerrado; front público abierto) | ✅ Implementada |
| Recuperación de contraseña | ✅ Implementada |
| Gestión de socios (CRUD: alta, edición, baja, listado) | ⏳ En desarrollo |
| Módulo económico simple (ingresos a partir de las colaboraciones) | ⏳ Planificado |
| Informes visuales (dashboards) | ⏳ Planificado |
| Front público con las secciones de la ONG | ⏳ Planificado |

> Leyenda: ✅ implementada · ⏳ pendiente. Ver el [roadmap](#roadmap) para lo que queda fuera del MVP.

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| **Lenguaje / runtime** | C# · .NET 10 |
| **Framework web** | ASP.NET Core **Razor Pages** |
| **ORM / datos** | Entity Framework Core 10 |
| **Base de datos** | SQLite (fichero local; coste cero) |
| **Autenticación** | ASP.NET Core Identity (con roles) |
| **Frontend** | Bootstrap · jQuery · diseño **mobile-first** · sin estilos/scripts inline (CSP) |
| **Despliegue** | Azure App Service (plan F1, gratuito) |
| **Control de versiones** | Git · GitHub (repositorio público) |

## Arquitectura

Solución de **dos proyectos**, separando presentación de dominio:

- **`DididaiApp`** — proyecto **web** (Razor Pages). Solo presentación y configuración del pipeline
  (`Program.cs`). Consume los servicios del proyecto Core; **no** accede directamente al `DbContext`.
- **`DididaiApp.Core`** — **biblioteca de dominio**: entidades (`Models/`), acceso a datos
  (`Data/AppDbContext`), lógica de negocio (`Services/`) y migraciones de EF Core (`Migrations/`).

```
┌─────────────────────┐        ┌──────────────────────────┐
│   DididaiApp (web)   │  ───▶  │   DididaiApp.Core         │
│   Razor Pages        │        │   Models · Data · Services│
│   Program.cs         │        │   Migrations              │
└─────────────────────┘        └──────────────┬───────────┘
                                               │
                                        ┌──────▼──────┐
                                        │   SQLite    │
                                        └─────────────┘
```

Flujo de una petición: **HTTP → página Razor → servicio (Core) → `AppDbContext` → SQLite → respuesta HTML**.

El detalle de las decisiones de arquitectura y su porqué está documentado en
[`context/decisions.md`](context/decisions.md).

## Modelo de datos

El dominio distingue la **persona** de sus **formas de aportación**, en una relación 1:N:

- **`Socio`** — identidad estable del colaborador: datos personales, contacto, domicilio, consentimiento
  RGPD y fecha de alta.
- **`Colaboracion`** — cada forma de aportar del socio. Es una jerarquía **Table-Per-Hierarchy** (una sola
  tabla con discriminador) con los subtipos:
  - `CuotaDomiciliada` (importe, modalidad mensual/anual, IBAN),
  - `AportacionUnica` (importe, fecha),
  - `Teaming` (microdonación recurrente).

Este diseño permite que un mismo socio tenga varias colaboraciones (activas o históricas) de distinto tipo,
y que el módulo económico calcule los ingresos a partir de las colaboraciones, no de los socios.

## Estructura del repositorio

```
DIDIDAI.ORG/
├── DididaiApp/                 # Proyecto web (Razor Pages)
│   ├── Pages/                  # Páginas: Index, Privacy, Admin/, Shared/
│   ├── Services/               # Servicios de presentación (p. ej. envío de email)
│   ├── wwwroot/                # Estáticos (CSS, JS, librerías)
│   └── Program.cs              # Configuración del pipeline y servicios
├── DididaiApp.Core/            # Biblioteca de dominio
│   ├── Models/                 # Socio, Colaboracion, enums
│   ├── Data/                   # AppDbContext, DbSeeder
│   └── Migrations/             # Migraciones de EF Core
├── context/                    # Documentación durable (decisiones, next-steps, overview)
├── logs/                       # Crónica del proyecto por semanas
├── DididaiApp.sln
├── ORACULO.md                  # Tablero estratégico y estado del proyecto
└── README.md
```

## Instalación y ejecución

**Requisitos:** [SDK de .NET 10](https://dotnet.microsoft.com/download).

```bash
# Clonar
git clone https://github.com/Sk0t0h/dididai.git
cd dididai

# Aplicar migraciones (crea la base de datos SQLite)
dotnet ef database update --project DididaiApp.Core --startup-project DididaiApp

# Ejecutar
cd DididaiApp
dotnet run
```

La aplicación queda disponible en `https://localhost:7080` (o `http://localhost:5110`).

> Si no tienes la herramienta de EF Core: `dotnet tool install --global dotnet-ef`.

## Configuración y secretos

El repositorio es **público**, por lo que **no contiene secretos**. En desarrollo se usan
[User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets); en producción, variables de
entorno.

Para que la aplicación siembre el **usuario administrador inicial** hay que proporcionar sus credenciales
por configuración (si faltan, el seed se omite y se registra un aviso):

```bash
dotnet user-secrets set "Seed:AdminEmail"    "<email-admin>"  --project DididaiApp
dotnet user-secrets set "Seed:AdminPassword" "<contraseña>"    --project DididaiApp
```

La cadena de conexión por defecto apunta a un fichero SQLite local (`Data Source=dididai.db`) y no contiene
credenciales.

## Despliegue

Desplegado en **Azure App Service** (plan **F1**, gratuito) mediante Azure CLI:

```bash
az group create --name rg-dididai --location francecentral
az appservice plan create --name plan-dididai --resource-group rg-dididai --sku F1 --is-linux
az webapp create --name dididai-web --resource-group rg-dididai --plan plan-dididai --runtime "DOTNETCORE:10.0"

# Credenciales y BD persistente como App Settings (no en el repo)
az webapp config appsettings set --name dididai-web --resource-group rg-dididai --settings \
  "Seed__AdminEmail=<email>" "Seed__AdminPassword=<contraseña>" \
  "ConnectionStrings__DefaultConnection=Data Source=/home/dididai.db"

# Publicar
dotnet publish DididaiApp/DididaiApp.csproj -c Release -o ./publish
# (empaquetar ./publish en un zip y desplegarlo)
az webapp deploy --name dididai-web --resource-group rg-dididai --src-path dididai.zip --type zip
```

> **Nota sobre el plan F1 (gratuito):** tiene una cuota diaria de CPU (~60 min) y la aplicación se suspende
> por inactividad; al recibir una visita tarda unos segundos en reactivarse. Es una limitación aceptable
> para una demo de TFM. La base de datos se ubica en `/home` (almacenamiento persistente de App Service)
> para que sobreviva a los reinicios.

## Seguridad

- **Sin secretos en el repositorio** (es público): credenciales y cadenas de conexión sensibles van por
  User Secrets (desarrollo) o variables de entorno (producción).
- **Front público abierto; back de gestión protegido** por rol de administración. El **registro público de
  usuarios está deshabilitado**: las altas se hacen desde dentro del back.
- **Buenas prácticas OWASP en formularios:** validación en servidor además de en cliente, protección
  antiforgery/CSRF (integrada en Razor Pages) y no exposición de datos sensibles en logs ni URL.
- **CSP-friendly:** sin estilos ni manejadores de eventos inline en el HTML.
- **RGPD:** la base de datos con datos personales de socios **no se versiona** (`*.db` en `.gitignore`);
  en la demo se emplean datos de ejemplo anonimizados.

## Acceso de prueba

- **Usuario:** _(se indicará para la demo)_
- **Contraseña:** _(se indicará para la demo)_

> El acceso de prueba usa datos ficticios; no se publican credenciales reales de la ONG.

## Enlaces del proyecto

- **Repositorio:** https://github.com/Sk0t0h/dididai
- **Despliegue (URL pública):** https://dididai-web.azurewebsites.net
- **Presentación (slides):** _(pendiente)_
- **Vídeo demostrativo:** _(pendiente)_

## Roadmap

Fuera del alcance del MVP del TFM, como evolución futura del producto:

- **Gestor de contenidos (CMS)** completo para que la ONG edite el front público sin tocar código.
- **Contabilidad avanzada** (cuadres, categorías de gasto, ejercicios).
- **Proveedor de email real** (SendGrid/SMTP) para la recuperación de contraseña en producción.
- **Autenticación en dos factores (2FA)**.

## Licencia

Distribuido bajo licencia [MIT](LICENSE) © DIDIDAI.
