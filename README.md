# DIDIDAI.ORG

Aplicación web a medida para la ONG **DIDIDAI**: un **front público** informativo abierto y un
**back de gestión cerrado** (privado, con login) para administrar el día a día de la organización sin
depender de un gestor de contenidos genérico.

> **Trabajo Fin de Máster** — Máster de Desarrollo con IA (BIG School).
> Repositorio público · Licencia MIT a nombre de DIDIDAI · .NET 10 · ASP.NET Core Razor Pages.
>
> **Aplicación en producción:** https://dididai-ong.azurewebsites.net

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
9. [Pruebas](#pruebas)
10. [Despliegue](#despliegue)
11. [Seguridad y privacidad](#seguridad-y-privacidad)
12. [Acceso de prueba](#acceso-de-prueba)
13. [Enlaces del proyecto](#enlaces-del-proyecto)
14. [Roadmap](#roadmap)
15. [Licencia](#licencia)

---

## Descripción del proyecto

**DIDIDAI** es una ONG de derechos infantiles y educación especial que opera un orfanato en Katmandú
(Nepal) para menores con discapacidad, ofreciendo recursos educativos, estimulación multisensorial y apoyo
terapéutico. Su mensaje de marca es que **el 99 % de los ingresos va a acción directa**.

Este proyecto sustituye la web actual de la ONG por una **solución propia a medida**, con dos caras
claramente separadas:

- **Front público (abierto):** presenta la organización —misión, actividad, filosofía, objetivos y
  contacto—, está disponible en **español e inglés** y permite a las personas interesadas colaborar mediante
  un formulario (cuota de socio, donación única o microdonación).
- **Back de gestión (privado, con autenticación):** permite al equipo de la ONG administrar socios,
  colaboraciones, control económico, solicitudes de colaboración recibidas, usuarios administradores y una
  auditoría transversal de las acciones, sin depender de un CMS genérico tipo WordPress/Joomla.

La transparencia económica (el mensaje del "99 % a acción directa") es un eje del proyecto: por eso el
módulo económico y los informes visuales tienen un papel destacado en el back de gestión.

## Funcionalidades

Todas las funcionalidades del MVP están **implementadas y desplegadas en producción**.

| Área | Funcionalidad | Estado |
|---|---|---|
| **Front público** | Landing one-page bilingüe ES/EN (hero, actividad, filosofía, transparencia, objetivos, contacto) | ✅ |
| | Formulario público de colaboración → crea una solicitud (no da de alta socio directamente) | ✅ |
| | Páginas legales: aviso legal, política de privacidad y política de cookies (bilingües) | ✅ |
| **Autenticación** | Login con roles, back protegido, registro público deshabilitado (altas desde dentro) | ✅ |
| | Recuperación de contraseña por email real (SendGrid) | ✅ |
| | Autenticación en dos pasos (2FA / TOTP) con QR generado en servidor | ✅ |
| | Política de sesión OWASP (inactividad 30 min, máximo absoluto 8 h) | ✅ |
| **Gestión** | Socios (CRUD, baja lógica y reactivación; validación por tipo de documento y país) | ✅ |
| | Colaboraciones (cuota domiciliada / aportación única / Teaming; IBAN validado; baja lógica) | ✅ |
| | Módulo económico (gastos con periodicidad, ingresos, balance) | ✅ |
| | Dashboards: 5 gráficas (ingresos por tipo, balance, gastos por categoría, altas y previsión) | ✅ |
| | Solicitudes de colaboración (máquina de estados + alta de socio y colaboración desde la solicitud) | ✅ |
| | Usuarios administradores (alta / desactivación, superadmin protegido) | ✅ |
| | Auditoría transversal (registro inmutable de acciones con diff antes/después) | ✅ |

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| **Lenguaje / runtime** | C# · .NET 10 |
| **Framework web** | ASP.NET Core **Razor Pages** |
| **ORM / datos** | Entity Framework Core 10 |
| **Base de datos** | SQLite (fichero; coste cero, en almacenamiento persistente de App Service) |
| **Autenticación** | ASP.NET Core Identity (roles + 2FA/TOTP) |
| **Email** | SendGrid (recuperación de contraseña real en producción) |
| **Frontend** | Bootstrap · jQuery · Chart.js (servido local) · **mobile-first** · sin inline (CSP estricta) |
| **Internacionalización** | Localización ASP.NET Core (`.resx` + cookie de cultura), ES/EN en el front |
| **Pruebas** | xUnit (156 pruebas unitarias y de integración) |
| **Despliegue** | Azure App Service (plan B1, Linux) · región Spain Central |
| **CI** | GitHub Actions (build + test en cada push/PR) |
| **Control de versiones** | Git · GitHub (repositorio público) |

## Arquitectura

Solución de **dos proyectos** (+ uno de pruebas), separando presentación de dominio:

- **`DididaiApp`** — proyecto **web** (Razor Pages). Presentación, `Areas/Identity` (páginas de login/perfil/
  2FA personalizadas y en español) y configuración del pipeline (`Program.cs`). Consume los servicios del
  proyecto Core; **no** accede directamente al `DbContext`.
- **`DididaiApp.Core`** — **biblioteca de dominio**: entidades (`Models/`), acceso a datos
  (`Data/AppDbContext`), lógica de negocio (`Services/`), sembradores de datos y migraciones de EF Core.
- **`DididaiApp.Tests`** — pruebas unitarias y de integración (xUnit).

```
┌─────────────────────┐        ┌──────────────────────────┐
│   DididaiApp (web)   │  ───▶  │   DididaiApp.Core         │
│   Razor Pages        │        │   Models · Data · Services│
│   Areas/Identity     │        │   Migrations · Seeders    │
│   Program.cs         │        └──────────────┬───────────┘
└─────────────────────┘                        │
                                        ┌───────▼──────┐
                                        │    SQLite    │
                                        └──────────────┘
```

Flujo de una petición: **HTTP → página Razor → servicio (Core) → `AppDbContext` → SQLite → respuesta HTML**.

Decisión transversal de auditoría: la traza de acciones la disparan **las páginas** tras cada operación
exitosa (pasando el usuario), de modo que el dominio (Core) no depende de HTTP. El detalle de las decisiones de
arquitectura y su porqué está documentado en [`context/decisions.md`](context/decisions.md).

## Modelo de datos

El dominio distingue la **persona** de sus **formas de aportación**, y registra tanto la actividad económica
como la trazabilidad de la gestión:

- **`Socio`** — identidad estable del colaborador: datos personales, contacto, domicilio (país de residencia
  ISO), tipo y número de documento, teléfono E.164, consentimiento RGPD, fecha de alta y baja lógica.
- **`Colaboracion`** — cada forma de aportar del socio, en jerarquía **Table-Per-Hierarchy** (una tabla con
  discriminador): `CuotaDomiciliada` (importe, modalidad mensual/anual, IBAN), `AportacionUnica` (importe,
  fecha) y `Teaming` (microdonación recurrente). Relación 1:N con `Socio`.
- **`SolicitudColaboracion`** — intención de colaborar recibida por el formulario público. Sigue una **máquina
  de estados** (Pendiente → Gestionando → Aprobada / Cancelada) con su historial de acciones
  (**`AccionSolicitud`**). Es independiente del socio: aprobar una solicitud ≠ crear el socio o la
  colaboración (el IBAN solo entra al formalizar la colaboración, nunca en el formulario público).
- **`Gasto`** — gasto de la organización (concepto, importe, categoría, periodicidad mensual/anual), base del
  módulo económico junto con los ingresos derivados de las colaboraciones.
- **`RegistroAuditoria`** — traza inmutable de las acciones de gestión (fecha, usuario, acción, entidad y un
  diff **antes/después** en las ediciones, con el IBAN enmascarado).

La validación de identidad (DNI/NIE con letra de control, IBAN por mod-97, teléfono E.164) vive en Core como
**funciones puras**, cubiertas por pruebas, y se aplica igual en cliente y en servidor.

## Estructura del repositorio

```
DIDIDAI.ORG/
├── DididaiApp/                 # Proyecto web (Razor Pages)
│   ├── Pages/                  # Index (front), Legal/, Admin/ (Socios, Colaboraciones,
│   │                           #   Economia, Solicitudes, Usuarios, Auditoria), Shared/
│   ├── Areas/Identity/         # Login, perfil, contraseña y 2FA personalizados (en español)
│   ├── Services/               # Servicios de presentación (p. ej. envío de email)
│   ├── Resources/              # Ficheros de localización (.resx) ES/EN del front
│   ├── wwwroot/                # Estáticos (CSS, JS, fuentes, Chart.js, librerías)
│   └── Program.cs              # Configuración del pipeline, seguridad y servicios
├── DididaiApp.Core/            # Biblioteca de dominio
│   ├── Models/                 # Socio, Colaboracion, SolicitudColaboracion, Gasto,
│   │                           #   RegistroAuditoria, enums y validaciones
│   ├── Data/                   # AppDbContext, sembradores (admin y datos de demo)
│   ├── Services/               # Lógica de negocio (socios, colaboraciones, economía, etc.)
│   └── Migrations/             # Migraciones de EF Core
├── DididaiApp.Tests/           # Pruebas (xUnit)
├── context/                    # Documentación durable (decisiones, next-steps, overview)
├── logs/                       # Crónica del proyecto por semanas
├── .github/workflows/          # CI (build + test)
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

# Ejecutar (la base de datos SQLite se crea y migra sola en el primer arranque)
cd DididaiApp
dotnet run
```

La aplicación queda disponible en `https://localhost:7080` (o `http://localhost:5110`).

> El esquema se aplica automáticamente al arrancar (`Database.Migrate()`). Si prefieres aplicar las
> migraciones a mano: `dotnet ef database update --project DididaiApp.Core --startup-project DididaiApp`
> (requiere la herramienta: `dotnet tool install --global dotnet-ef`).

## Configuración y secretos

El repositorio es **público**, por lo que **no contiene secretos**. En desarrollo se usan
[User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets); en producción, variables de
entorno (App Settings de Azure).

Para que la aplicación siembre el **usuario administrador inicial** hay que proporcionar sus credenciales
por configuración (si faltan, el seed se omite y se registra un aviso):

```bash
dotnet user-secrets set "Seed:AdminEmail"    "<email-admin>" --project DididaiApp
dotnet user-secrets set "Seed:AdminPassword" "<contraseña>"  --project DididaiApp
```

Ajustes opcionales: `SendGrid:ApiKey` / `FromEmail` / `FromName` (email real; sin ellos, la recuperación de
contraseña se registra en el log en lugar de enviarse) y `Seed:DemoData=true` (siembra datos de ejemplo para
demostración; ver [Acceso de prueba](#acceso-de-prueba)). La cadena de conexión por defecto apunta a un fichero
SQLite local y no contiene credenciales.

## Pruebas

**156 pruebas** (xUnit) cubren la lógica de negocio con enfoque TDD: validación de identidad (DNI/NIE, teléfono
E.164), IBAN (mod-97), catálogos de países y prefijos, y los servicios de socios, colaboraciones, resumen
económico, solicitudes, auditoría y usuarios administradores. Las de servicios usan SQLite en memoria.

```bash
dotnet test
```

El pipeline de **GitHub Actions** (`.github/workflows/`) ejecuta build + test en cada push y pull request.

## Despliegue

Desplegado en **Azure App Service** (plan **B1**, Linux) en la región **Spain Central**, mediante Azure CLI.
El procedimiento completo y reproducible está en [`context/deploy-azure.md`](context/deploy-azure.md); en
resumen:

```bash
# (solo la primera vez) crear recursos
az group create --name rg-dididai --location spaincentral
az appservice plan create --name plan-dididai-es --resource-group rg-dididai --location spaincentral --sku B1 --is-linux
az webapp create --name dididai-ong --resource-group rg-dididai --plan plan-dididai-es --runtime "DOTNETCORE:10.0"

# credenciales y BD persistente como App Settings (no en el repo)
az webapp config appsettings set --name dididai-ong --resource-group rg-dididai --settings \
  "Seed__AdminEmail=<email>" "Seed__AdminPassword=<contraseña>" \
  "ConnectionStrings__DefaultConnection=Data Source=/home/dididai.db"

# publicar (empaquetar ./publish en un zip y desplegarlo)
dotnet publish DididaiApp/DididaiApp.csproj -c Release -o ./publish
az webapp deploy --name dididai-ong --resource-group rg-dididai --src-path dididai.zip --type zip
```

> **Región Spain Central:** los datos personales de socios se alojan en territorio nacional, alineado con las
> recomendaciones de RGPD/LOPD sobre minimización de transferencias.
>
> **Plan B1:** sin la cuota diaria de CPU del plan gratuito F1; la aplicación permanece disponible de forma
> estable durante todo el periodo de evaluación. La base de datos SQLite se ubica en `/home` (almacenamiento
> persistente de App Service) para que sobreviva a los reinicios. El esquema se crea y migra automáticamente
> en el arranque, por lo que un despliegue de solo código no toca los datos existentes.

## Seguridad y privacidad

- **Sin secretos en el repositorio** (es público): credenciales y cadenas de conexión sensibles van por
  User Secrets (desarrollo) o variables de entorno (producción).
- **Front público abierto; back de gestión protegido** por rol. El **registro público está deshabilitado**;
  las altas de administrador se hacen desde dentro del back (con el superadministrador protegido).
- **Autenticación reforzada:** 2FA/TOTP opcional y política de sesión OWASP (inactividad 30 min + máximo
  absoluto de 8 h, sin cookie persistente).
- **Buenas prácticas OWASP en formularios:** validación en servidor además de en cliente, protección
  antiforgery/CSRF, honeypot y rate-limiting en el formulario público, y no exposición de datos sensibles en
  respuestas, logs ni URL (el IBAN se enmascara en la auditoría).
- **CSP estricta:** cabecera `Content-Security-Policy` sin `unsafe-inline`; sin estilos ni manejadores de
  eventos inline en el HTML. Librerías (Chart.js, fuentes) servidas en local, no desde CDN.
- **RGPD:** región nacional para los datos; 1.ª capa informativa y política de privacidad en el formulario;
  la base de datos con datos personales **no se versiona** (`*.db` en `.gitignore`) y la demo usa datos
  ficticios anonimizados.

> **Nota:** los textos legales (aviso legal / privacidad / cookies) son un **borrador** orientativo con
> marcadores `[ ]` pendientes de revisión jurídica y de completar con los datos reales de la ONG.

## Acceso de prueba

- **URL:** https://dididai-ong.azurewebsites.net → acceso en `/Admin`.
- **Usuario y contraseña de demostración:** facilitados en el **formulario de entrega** del TFM (no se
  publican en este repositorio público).

> El entorno de demostración está poblado con **datos ficticios anonimizados** (nombres inventados, correos
> `@example.org`, documentos e IBAN válidos solo en formato). No contiene datos reales de socios ni de la ONG.

## Enlaces del proyecto

- **Repositorio:** https://github.com/Sk0t0h/dididai
- **Despliegue (URL pública):** https://dididai-ong.azurewebsites.net
- **Presentación (slides):** _(pendiente)_
- **Vídeo demostrativo:** _(pendiente)_

## Roadmap

Fuera del alcance del MVP del TFM, como evolución futura del producto:

- **Exportación de datos** (CSV/Excel) de gastos, colaboraciones y socios, con controles RGPD (solo
  administración, IBAN enmascarado y registro de la exportación en la auditoría).
- **Tienda virtual de merchandising** como nueva vía de ingresos para la ONG (venta de productos con la
  marca). Es lo que justifica incorporar una **pasarela de pago real** (Stripe / adeudos SEPA) —asumiendo su
  coste— que además serviría para formalizar las colaboraciones en línea.
- **Gestor de contenidos (CMS)** para que la ONG edite el front público sin tocar código.
- **Contabilidad avanzada** (cuadres, ejercicios, categorías detalladas).
- **Mejora de entregabilidad de email** (SPF/DKIM/DMARC del dominio).

## Licencia

Distribuido bajo licencia [MIT](LICENSE) © 2026 DIDIDAI.
