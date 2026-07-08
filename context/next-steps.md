# Next Steps — Continuidad de ejecución

> Acciones concretas y ejecutables. Actualizado: 2026-07-08.
> Deadline TFM: **domingo 20/07/2026**.

## PLAN VIGENTE (08-07) — Rediseño del flujo de solicitudes de colaboración

Rediseño del módulo de solicitudes (formulario público → gestión → alta de socio). Acordado con el usuario
tras la revisión visual del front. **Se ejecuta en 3 bloques, un commit por bloque** (cada uno compilable +
tests). Migración única al inicio del bloque A con todo el esquema nuevo (no encadenar 3 migraciones).

**Decisiones de diseño (cerradas 08-07):**
- **Máquina de estados** nueva: `Pendiente (gris) → Gestionando (amarillo) → Aprobada (verde) / Cancelada
  (rojo)`. Se elimina "Rechazada" como estado (el motivo va en la nota). Enum reordenado a orden lógico
  `Pendiente=0, Gestionando=1, Aprobada=2, Cancelada=3`; **se resetean los datos de prueba** (el módulo no
  está en producción, en local solo hay solicitudes de prueba) para evitar reinterpretar valores guardados.
- **Log de acciones de gestión** (entidad nueva `AccionSolicitud`): `Tipo` (Email/Teléfono/Nota/Otro),
  `Nota`, `Fecha` (auto UTC), `Usuario` (**el admin logueado; NO editable, lo fija el servidor**). Historial
  ordenado en la ficha. **Registrar la 1ª acción mueve Pendiente → Gestionando automáticamente.**
- **Matching con socios = SUGERENCIA, sin unicidad.** Email y teléfono NO llevan índice único (familias/
  gestores comparten contacto; la identidad real es el DNI, que el formulario público no pide). Al abrir la
  ficha se listan socios que coinciden por email o teléfono como "posibles coincidencias"; **el admin decide**
  vincular a uno existente o crear socio nuevo. Nunca enlaza automático.
- **Vinculación** solicitud↔socio: `SolicitudColaboracion.SocioId` (FK nullable). Al crear socio desde
  solicitud, se le asocian sus solicitudes; las pendientes de esa persona también quedan ligadas.
- **Direcciones opcionales:** `Direccion`/`CodigoPostal`/`Localidad` del `Socio` pasan a opcionales
  (`string?`, sin `[Required]`) — el formulario público no los recoge; el admin los completa luego.
- **Privacidad preseleccionada** en el alta SI viene de una solicitud (ya consintió en el formulario público);
  en alta manual sigue siendo obligatoria marcarla.
- **Al aprobar+crear socio se crea también su Colaboración** (tipo/importe/periodicidad leídos de la BD por
  `solicitudId`, NO de la URL). Excepción: microdonación/Teaming NO genera colaboración (se gestiona en Teaming).

**Troceo (un commit por bloque):**
- [ ] **Bloque A — Estados + colores + migración.** Enum nuevo, badges (gris/amarillo/verde/rojo), migración
      única con TODO el esquema nuevo (SocioId en solicitud, tabla `AccionSolicitud`, direcciones nullable,
      enum). Reset de datos de prueba. Pequeño y visible.
- [x] **Bloque B — Log de acciones.** HECHO (08-07, commit B `f25e62d`): entidad `AccionSolicitud` + servicio
      (usuario del admin logueado, no editable) + UI en la ficha + transición automática a Gestionando. TDD.
- [x] **Bloque C — Matching + vinculación + alta desde solicitud.** HECHO (08-07, commits C1 `a69293c` + C2
      `0b5f6fe`). C1: matching por email/teléfono (sugerencia) + vincular a socio existente. C2: crear la
      colaboración desde la solicitud (socio existente) y alta de socio nuevo desde solicitud (precarga + privacidad
      + vínculo). `SolicitudColaboracion.ColaboracionId` para no duplicar. Direcciones opcionales. TDD.

**RESUELTO EL REDISEÑO (08-07).** Los 4 bloques A-C2 hechos, validados visualmente y commiteados en local
(SIN push/deploy todavía). **Cerrar:** ~~actualizar memoria~~ (hecho) → **push + deploy** (pendiente; ojo al
enum en datos de PRODUCCIÓN: `Rechazada=2` → ahora `Aprobada=2`, valorar data-fix o reset antes de desplegar) →
validación en prod. Sigue pendiente traducir EN del front (contenido ES puesto, EN cae a ES por fallback).

## PENDIENTES SUELTOS (para después)

- [x] **Bug "Acceso gestión" del front ignora la sesión.** RESUELTO (08-07): la cabecera del front (en
      `Index.cshtml`, no en `_PublicLayout`) ahora comprueba sesión: con sesión muestra el menú de gestión
      (Admin/Socios/Economía/Solicitudes + Salir); sin sesión, enlace "Acceso gestión". En el back, "Panel"→
      "Admin", sin "Inicio", y quitado el "Gestión" duplicado del `_LoginPartial`. Además, la cabecera del back
      se rediseñó para replicar el lenguaje del front (logo + crema + línea naranja, email a la izquierda,
      secciones a la derecha, sin selector de idioma); las páginas públicas que heredan `_Layout` (Privacy,
      login, Error) conservan su navbar pública con idioma.
- [ ] **MAÑANA (09-07): pulir páginas de Identity** (login, gestión de cuenta, etc.). Recordatorio: ya adoptan el
      `_Layout` del back vía `Areas/Identity/Pages/_ViewStart.cshtml`, pero su contenido interno sigue siendo el
      de la Default UI (inglés, opciones que no aplican: registrarse, proveedores externos, confirmar email…).
      Requiere **scaffold** de esas páginas para traducirlas y quitar lo que no aplica (zona sensible: auth).
- [ ] **Decidir (a validar con C2 completo):** una solicitud de tipo "Socio" puede quedar **Aprobada sin
      cuota creada** — aprobar y "Crear colaboración" son acciones distintas (el IBAN se pide solo al crear la
      cuota, no al aprobar; esto es intencional: aprobar ya, pedir IBAN al socio y crear la cuota después).
      Revisar si el flujo debe avisar/forzar la creación de la cuota, o dejarlo así con un indicador visible de
      "aprobada, cuota pendiente". Observado por el usuario el 08-07.

---

## (Histórico) Plan original por findes — SUPERADO
> El plan de abajo es del arranque (03-07) y quedó superado: el núcleo del MVP se completó y desplegó el 05-07
> y el front público el 07-07. Se conserva como referencia de los requisitos formales del TFM.

## Ritmo de trabajo (condicionante real)

El usuario avanza **sobre todo en fines de semana** y en **ratos sueltos y cortos entre semana** ("de
estranjis" en el trabajo). Implicación para el plan:
- El **trabajo pesado y continuado** (features nuevas, migraciones, despliegue) va a los **findes**.
- Los **ratos de diario** deben ser tareas **pequeñas, autocontenidas y sin riesgo**: revisar código
  generado, apuntar decisiones, pulir README, meter datos de prueba, corregir un bug acotado. Nunca empezar
  algo grande que quede a medias y difícil de retomar.
- Al cerrar cada rato de trabajo, dejar `ai-context.md` actualizado para poder retomar en frío sin perder
  contexto (importante cuando las sesiones son cortas y espaciadas).

## Filosofía del plan

Estructurado por **fines de semana** (el grueso del trabajo) + huecos de diario para rascar ratos. Principios:
- **Esqueleto vivo y desplegado cuanto antes** (finde 1): el despliegue se valida al principio, no al final.
- **Un MVP pequeño 100% terminado > uno grande al 80%.**
- **Colchón sagrado:** los días de diario y el margen del domingo 20 son para absorber imprevistos, NO para
  añadir features. Cuando CRUD + dashboard funcionen "suficientemente bien", se para de añadir código.

## Finde 1 — 4 y 5 jul → "Esqueleto vivo y desplegado"

- [x] Commit inicial del sistema de memoria del repo.
- [ ] Proyecto Razor Pages arrancando en local (ya arranca la plantilla; validar).
- [x] EF Core + proveedor SQLite añadidos. Entidad `Socio` en BD vía EF Core (primera migración).
      *(Hecho 04-07: además arquitectura multi-proyecto Web+Core y modelo `Socio`+`Colaboracion` TPH.)*
- [x] Autenticación funcionando: login + zona protegida (ASP.NET Core Identity + rol `Admin`, seed admin).
      *(Hecho 04-07: Default UI, registro público bloqueado, recuperación con email stub, /Admin protegido.
      Verificado end-to-end.)*
- [ ] **URL pública desplegada en Azure App Service F1** (aunque muestre poco). Validar el pipeline ya.
      **← siguiente tarea.**
- [ ] `README.md` creado con secciones vacías (las del enunciado).
- [ ] **Pendiente de UI (cuando toque front):** mobile-first (principio acordado 04-07).
- [ ] **Antes/para el deploy:** proveedor de email real (SendGrid/SMTP) en lugar del `LoggingEmailSender`;
      credenciales del seed admin como variables de entorno en Azure (hoy en User Secrets local).
- [ ] **Vigilar** parche de `SQLitePCLRaw`/EF para NU1903; actualizar y revisar antes del deploy.
- [ ] **Opcional (si sobra tiempo):** scaffolding de 2FA de Identity.

## Entre semana 6-10 jul (si hay ratos)

- [ ] Revisar con calma el código generado por Claude Code.
- [ ] Ir apuntando decisiones de arquitectura (→ `context/decisions.md`) para las slides.
- [ ] Afinar el modelo de datos de `Socio` si falta algo.

## Finde 2 — 11 y 12 jul → "El grueso funcional"

- [ ] CRUD de socios completo: alta, edición, baja, listado con búsqueda/filtro. Validación servidor +
      cliente, antiforgery.
- [ ] Dashboard con 3-4 métricas y 1-2 gráficas leyendo datos reales de socios (librería de charts sin
      inline, respetando CSP).
- [ ] Front público mínimo digno (adaptar secciones de la web actual de DIDIDAI).
- [ ] Cada avance desplegado; README actualizándose sobre la marcha.
- [ ] **Regla:** cuando CRUD + dashboard funcionen suficientemente bien, FIN del código nuevo.
      (Módulo económico simple: solo si sobra tiempo; es el primer candidato a recortar.)

## Entre semana 13-17 jul

- [ ] Pruebas de uso reales (meter socios de mentira, navegar, buscar fallos). Datos de ejemplo anonimizados.
- [ ] Corregir bugs. Pulir README. **Nada de features nuevas.**

## Requisitos formales del enunciado (Módulo 12 — verificados 04-07)

De `Documentacion-TFM-Fundae-1.md`. Lo que se entrega vía formulario del Proyecto Final:
- **README** con: (a) descripción, (b) stack, (c) instalación/ejecución, (d) estructura, (e) funcionalidades,
  (f) usuario/contraseña de prueba. → **Ya cubierto en README.md** (falta rellenar credenciales de demo).
- **Repo público en GitHub** ✅ (si fuera privado, habría que dar acceso a `mouredev@gmail.com` — no aplica).
- **URL de despliegue** (recomendado) → en la doc. En curso (Azure F1).
- **Slides** con presentación: URL pública **y** enlazadas/incluidas en el repo/doc.
- **Vídeo OBLIGATORIO** con explicación + **captura de pantalla obligatoria** (rostro opcional). URL pública.
- Formulario de entrega pide: nombre, email de inscripción al máster, URL repo, URL despliegue, URL slides,
  URL vídeo, usuario/contraseña de prueba.

## Finde 3 — 18 y 19 jul → "Entregables no-código"

- [ ] **Sábado 18 por la mañana: tope absoluto para tocar código.** A partir de ahí congelado (salvo bug
      crítico).
- [ ] `README.md` terminado: descripción, stack, instalación/ejecución, estructura, funcionalidades,
      usuario/contraseña de prueba, URL despliegue, URL slides, URL vídeo, + roadmap (CMS, contabilidad).
- [ ] Slides creadas y publicadas con URL pública (incluir roadmap como visión de producto).
- [ ] Vídeo 3-5 min con captura de pantalla (landing, login, CRUD, dashboard), subido con URL pública.
- [ ] Verificar que en el repo están: documentación, info de despliegue, y slides (o enlaces).

## Domingo 20 jul → Entrega

- [ ] Rellenar formulario del Proyecto Final: nombre, email de inscripción, URL repo, URL despliegue,
      URL slides, URL vídeo, usuario/contraseña de prueba.
- [ ] **Entregar por la mañana (o dejarlo listo el sábado 19).** No dejarlo para la noche.
