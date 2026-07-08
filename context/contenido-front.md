# Contenido del front público — literal de www.dididai.org

> Texto **literal** extraído de la web actual (www.dididai.org) el 2026-07-06, para montar el front público
> del MVP. Se versiona aquí para no volver a perderlo (antes solo estaba mencionado, no transcrito).
> El copy EN se traducirá a partir de esto (la infra i18n ya está montada).
>
> **AVISO seguridad:** el IBAN real de la ONG está expuesto en la web vieja. **NO copiarlo** al repo público.
> Usar datos ficticios en cualquier ejemplo. El orfanato es **BalMandir**, en **Katmandú (Nepal)**.

## Navegación (web vieja)

Inicio · COLABORAR · Actividad · Filosofía · Objetivos · Contacto · Registrarme

En nuestro front: one-page con anclas (Inicio/Actividad/Filosofía/Objetivos/Contacto) + CTA "Colaborar"
(formulario público→BD). "Registrarme" NO se replica (el registro de usuarios del back está cerrado a Admin).

## Inicio / Home

No hay texto propio de "Inicio" renderizable en la web vieja (es SPA / el bloque no lo devuelve el fetch).
Para el hero usaremos el mensaje de marca (99% a acción directa) + una frase de la Actividad. Copy del hero
por redactar/confirmar con el usuario al ejecutar.

## Actividad (literal)

> "En estos momentos la actividad de Dididai se desarrolla en Nepal, encargándose de la educación de los niños
> y jóvenes con diversidad funcional de un orfanato de Katmandú, utilizando para ello innovadores recursos
> educativos, la estimulación multisensorial y el desarrollo creativo, además de apoyo terapéutico y médico."

## Filosofía (literal)

> "Queremos que la Convención sobre los Derechos del Niño y la Declaración Universal de los Derechos Humanos
> sean una realidad en aquellos lugares en los que nos encontremos."
>
> "Buscamos acciones sostenibles que mejoren la calidad de vida a largo plazo, para ello creemos que debemos
> apoyarnos en los medios existentes en el país que dan mayor continuidad a las acciones emprendidas y además
> contribuyen a apoyar la creación de recursos en el entorno."
>
> "El 99% de nuestros ingresos va a estas acciones directas. El trabajo de nuestros colaboradores es
> desinteresado y no retribuido."
>
> "No hay gastos administrativos, ni de marketing, ni apenas bancarios (excepto tipos de cambio aplicados por
> bancos internacionales)."

## Objetivos (literal — los 7 de los Estatutos)

1. La defensa de los derechos del niño en toda su extensión.
2. Favorecer el desarrollo de la Educación, y en particular de la Educación Especial, en aquellos países donde
   se detecten más carencias.
3. Promover la concienciación pública sobre discapacidades en dichos países.
4. Apoyar para que se constituyan Centros de Referencia en los ámbitos de la Educación Especial y de la
   atención a los discapacitados.
5. Facilitar la mejora de la calidad de vida de todos los miembros de la comunidad de dichos Centros de
   Referencia, residentes y trabajadores.
6. Conseguir que las necesidades básicas y más inmediatas de estos jóvenes sean cubiertas. Esto incluye
   fisioterapia, logopedia, terapia ocupacional, sistemas aumentativos de comunicación, instrumentos de apoyo
   a la movilidad…
7. En particular, nuestra zona preferente de trabajo inicial será Nepal y el orfanato de BalMandir en Katmandú
   donde aspiramos a que los residentes con discapacidad reciban una educación de calidad, personalizada y
   adecuada a sus necesidades y estilos de aprendizaje.

## Contacto (literal)

- Email: **info@dididai.org**
- (Dirección/teléfono: no aparecen en el fetch de la web vieja. Confirmar con el usuario si se muestran.)

## Formulario Colaborar

El formulario de la web vieja no se pudo transcribir por WebFetch (SPA / embebido). No es bloqueante: el
formulario del front se diseña desde NUESTRO modelo `Socio`/`Colaboracion`, que ya replica las 3 opciones:

- **Hacerme socio/a** → `CuotaDomiciliada` (cuota periódica domiciliada; requiere IBAN).
- **Donación** → `AportacionUnica` (importe puntual).
- **Microdonación / Teaming** → `Teaming` (1 €/mes tipo Teaming).

Campos mínimos del alta pública (a confirmar al ejecutar): nombre, email, tipo de documento + documento,
teléfono (prefijo E.164 + número), país de residencia, tipo de colaboración + importe (+ IBAN si cuota
domiciliada), consentimiento RGPD obligatorio. Reusa la validación cliente=servidor ya existente.
