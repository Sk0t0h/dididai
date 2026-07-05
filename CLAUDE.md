# CLAUDE.md — DIDIDAI.ORG

Instrucciones estables del proyecto para Claude Code. Las preferencias de trato del
usuario viven en su `CLAUDE.md` global y aplican aquí también (no se repiten).

## Overview

Aplicación web de la ONG **DIDIDAI**. Proyecto de un máster; el repositorio es
**público** en GitHub (cuenta `Sk0t0h`, remote vía alias SSH `github-dididai`).
Arrancado desde la plantilla estándar de ASP.NET Core Razor Pages; sin desarrollo de
producto propio aún (a fecha 2026-07-03).

## Stack y arquitectura

- **Framework:** ASP.NET Core **Razor Pages** (no MVC, no Blazor).
- **TargetFramework:** `net10.0`. `Nullable` e `ImplicitUsings` habilitados.
- **Proyecto:** `DididaiApp/DididaiApp.csproj` (Sdk `Microsoft.NET.Sdk.Web`).
- **Persistencia:** ninguna todavía (sin EF, sin BD). *Por confirmar* cuál se usará.
- **Auth:** ninguna todavía (`app.UseAuthorization()` está pero sin esquema configurado).
- **Integraciones externas:** ninguna todavía.
- **Frontend:** Bootstrap + jQuery + jquery-validation (librerías de plantilla en
  `wwwroot/lib/`). CSS propio en `wwwroot/css/site.css`.

## Arranque y build

```powershell
cd DididaiApp
dotnet run                 # perfil por defecto
```

- Puertos (perfil `https`): `https://localhost:7080` y `http://localhost:5110`.
- Perfil `http`: solo `http://localhost:5110`.
- `ASPNETCORE_ENVIRONMENT=Development` en ambos perfiles de launch.

Build/test: `dotnet build` / `dotnet test`. Tests unitarios en **`DididaiApp.Tests`** (xUnit): cubren la
validación de identidad (`ValidacionIdentidad`: DNI/NIE/E.164) y los catálogos (`Paises`,
`PrefijosTelefonicos`). Son funciones puras (sin BD ni HTTP). Al tocar esa lógica, añadir/actualizar tests.

## Convenciones y zonas sensibles

- **Repo público → nunca subir secretos.** Connection strings, API keys, tokens NO
  van en `appsettings.json` ni `appsettings.Development.json` (se versionan). Usar
  **User Secrets** en desarrollo y variables de entorno en producción.
- **Frontend sin inline (CSP):** sin `style="..."` ni `on*=""` en el HTML; estilos a
  clases, comportamiento a JS externo. Responsive siempre.
- **Seguridad de formularios (OWASP):** validación en servidor además de cliente;
  antiforgery/CSRF; no exponer datos sensibles en respuestas, logs ni URL.

## Git / cuenta

- Identidad **local** de este repo: `Eduardo Fraga <eduardo.fraga.lopez@gmail.com>`
  (la global del usuario es distinta y NO se toca).
- Remote vía alias SSH dedicado: `github-dididai:Sk0t0h/dididai.git`. Clave
  `~/.ssh/id_ed25519_dididai` (con passphrase), cargada en el ssh-agent de Windows.
- **Operar el remoto desde PowerShell o la terminal de VS Code**, no desde Git Bash
  (Git Bash usa otro `ssh` y volvería a pedir la passphrase). `core.sshCommand` local
  apunta a `C:/Windows/System32/OpenSSH/ssh.exe`.

## Memoria del proyecto (vive EN EL REPO, versionada)

La memoria del proyecto reside en el repositorio y se versiona, para que cualquier desarrollador (o agente,
o una IA sin acceso al código) la herede. **No** guardar estado de los desarrollos en memorias privadas del
agente; lo privado se limita a preferencias de trato del usuario y atajos. Ficheros y su rol (filosofía por
capas: cargar siempre lo barato, consultar lo caro solo cuando hace falta):

- **`ORACULO.md`** — tablero estratégico + estado actual del proyecto, **autoexplicativo y transferible a
  otra IA sin repo**. Active Focus, Module Status, Latest Work, Immediate Risks. Crece poco.
- **`ai-context.md`** — estado vigente **volátil** (foco de hoy, próximos pasos inmediatos). Se sobreescribe.
- **`context/`** — conocimiento durable: `decisions.md` (decisiones + porqué), `next-steps.md` (acciones
  ejecutables), `project-overview.md` (baseline técnico). Se consulta por relevancia.
- **`logs/AAAA/AAAA-Wnn.md`** — crónica semanal a nivel de decisión/cambio (el detalle fino vive en commits).

**Regla de arranque:** al empezar una sesión o retomar contexto, leer `ai-context.md` y `ORACULO.md` (Active
Focus) antes de planificar; consultar `context/next-steps.md` para el detalle accionable.

**Regla de cierre (proactiva, sin que el usuario lo pida):** al cerrar cada bloque de trabajo sustancial,
actualizar la crónica en `logs/`, **regenerar `ORACULO.md`** (estado real, avances, lo que viene) y
sobreescribir `ai-context.md`. El detalle técnico exacto va al commit, no al log.
