# AI Context — Estado actual de trabajo

> Memoria de trabajo **volátil**: el "ahora" del proyecto (foco, próximos pasos inmediatos). Se
> **sobreescribe** en cada cierre de bloque, no crece. Para la crónica histórica → `logs/`. Para el tablero
> estratégico estable → `ORACULO.md`. Para las acciones detalladas → `context/next-steps.md`.
> Actualizado: 2026-07-03.

## Estado actual

Fase de arranque. Definidos: alcance del TFM (MVP: web pública + login/roles + socios + económico simple +
dashboards; CMS y contabilidad avanzada a roadmap), stack de datos (EF Core + SQLite), despliegue (Azure App
Service F1) y **plan de entrega por fines de semana** (deadline domingo 20/07/2026). Memoria del repo creada.
**Aún sin código de producto**: el repo es la plantilla Razor Pages por defecto.

Ritmo real: se trabaja sobre todo en **findes** y en **ratos cortos entre semana**. Trabajo pesado a findes;
ratos de diario solo tareas pequeñas y sin riesgo.

## Foco inmediato (Finde 1, 4-5 jul — "Esqueleto vivo y desplegado")

1. Commit inicial de la memoria del repo.
2. EF Core + SQLite + entidad `Socio` (primera migración).
3. Auth (Identity + rol Admin, seed admin) con login y zona protegida.
4. Desplegar "hola mundo" en Azure App Service F1 para validar el pipeline.
5. README con secciones vacías.

## Pendientes abiertos

- Detallar campos exactos de la entidad `Socio` (con lo que la ONG registra hoy).
- Elegir librería de gráficas para el dashboard compatible con CSP.
- Decidir si el formulario de contacto envía email o solo persiste.

## Caveats de rama/entorno

- Branch de trabajo: `main`. (Sin flujo de ramas/PR definido aún.)
- Remoto solo desde **PowerShell / terminal de VS Code** (no Git Bash); clave SSH con passphrase en el
  ssh-agent de Windows.
- Sesiones cortas y espaciadas: dejar este fichero actualizado al cerrar cada rato para poder retomar en frío.
