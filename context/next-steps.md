# Next Steps — Continuidad de ejecución

> Acciones concretas y ejecutables. Actualizado: 2026-07-03.
> Deadline TFM: **domingo 20/07/2026**. Hoy: viernes 03/07/2026.

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
