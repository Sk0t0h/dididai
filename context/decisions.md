# Decisions Log — DIDIDAI.ORG

> Registro de decisiones de arquitectura y producto. Conciso, sin ruido.
> Formato: fecha · título · contexto · decisión · alternativas · consecuencias · estado.

---

## 2026-07-03 · Alcance del proyecto: web ONG DIDIDAI (front público + back de gestión), MVP para TFM

- **Contexto:** TFM del Máster de Desarrollo con IA (BIG School), deadline **20/07/2026**. Cliente real: ONG
  local para la que el usuario ya montó una web básica. Se quiere una web nueva taylor-made (no
  WordPress/Joomla) con front público abierto y back de gestión cerrado.
- **Decisión — alcance del producto:** front público + back de gestión con login. Módulos deseados a largo
  plazo: gestión de socios, cuentas/contabilidad de la ONG, informes visuales, gestor de contenido (CMS).
- **Decisión — MVP para el TFM (recorte por plazo):** web pública · autenticación + roles · **gestión de
  socios** (CRUD, alta/baja, listado) · **módulo económico simple** (ingresos/gastos) · **dashboards
  visuales** sobre esos datos.
- **Fuera del MVP → roadmap documentado:** CMS completo y contabilidad avanzada (cuadres, importes
  delicados). Se dejan como evolución futura (bien vistos en slides como "visión de producto").
- **Alternativas:** construir los 4 módulos completos — descartada, inviable y arriesgado en 17 días con
  vídeo/slides/README/deploy incluidos; MVP aún más reducido (sin módulo económico) — descartada, el módulo
  económico simple aporta mucho valor demostrable a bajo coste.
- **Consecuencias:** entrega completa y pulida > ambiciosa e inacabada. El CMS/contabilidad avanzada no se
  empiezan en esta fase.
- **Estado:** acordado. Pendiente detallar entidades y páginas.

## 2026-07-03 · Persistencia (EF Core + SQLite) y despliegue (Azure App Service F1)

- **Contexto:** el TFM exige (recomienda) despliegue con URL pública, y debe ser **sin coste** para la ONG.
- **Decisión:** ORM **EF Core** con proveedor **SQLite** (BD en fichero, no es un servicio facturable →
  coste cero imposible de superar). Despliegue en **Azure App Service F1 (gratuito)** para obtener URL
  pública nativa con el stack .NET.
- **Alternativas:** Azure SQL free — descartada por fragilidad del tier gratuito y riesgo de coste;
  SQL Server en contenedor en F1 — inviable (F1 no soporta contenedores); Postgres + host no-Azure —
  descartada por ser menos nativa con .NET.
- **Consecuencias:** máximo desacople entre la entrega y el presupuesto de la ONG. F1 tiene límites (la app
  se duerme, sin always-on) aceptables para demo de TFM. **Roadmap:** migrar a Azure SQL (cambiando el
  proveedor de EF Core) cuando la ONG asuma el coste de producción.
- **Estado:** acordado.

## 2026-07-03 · Cuenta de GitHub dedicada por SSH para este repo

- **Contexto:** el usuario usa una cuenta de GitHub distinta para este proyecto y no quiere afectar a la que
  usa en el resto de proyectos.
- **Decisión:** clave SSH dedicada `~/.ssh/id_ed25519_dididai` (con passphrase) + alias de host
  `github-dididai` en `~/.ssh/config` (con `IdentitiesOnly yes`). Remote =
  `github-dididai:Sk0t0h/dididai.git`. Identidad git **local** del repo a
  `Eduardo Fraga <eduardo.fraga.lopez@gmail.com>`; la global del usuario no se toca. `core.sshCommand` local
  → `C:/Windows/System32/OpenSSH/ssh.exe` para que git use el ssh-agent de Windows.
- **Alternativas:** reutilizar `id_ed25519` (ya registrada en la cuenta) — descartada para tener clave
  dedicada al proyecto; HTTPS+PAT — descartada por el problema de credenciales compartidas del Credential
  Manager en Windows.
- **Consecuencias:** el aislamiento es real (otros repos siguen usando `github.com` con su clave). Operar el
  remoto desde PowerShell/VS Code, no Git Bash (usa otro `ssh` y pediría passphrase).
- **Estado:** aplicado y verificado (`ssh -T github-dididai` → "Hi Sk0t0h!"; push OK).

## 2026-07-03 · Licencia MIT a nombre de DIDIDAI

- **Contexto:** repo público (requisito del máster) para una ONG.
- **Decisión:** licencia **MIT**, con el copyright a nombre de **DIDIDAI** (no del desarrollador a título
  personal).
- **Alternativas:** sin licencia (todos los derechos reservados) — descartada por contradecir el "público"
  del máster; Apache 2.0 / GPL — innecesariamente formal / restrictiva para el caso.
- **Consecuencias:** permite reutilización manteniendo aviso de copyright; cláusula "sin garantía" limita
  responsabilidad.
- **Estado:** aplicado.
