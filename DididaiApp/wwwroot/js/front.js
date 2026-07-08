// front.js — comportamiento del front público de DIDIDAI.
// CSP-safe: sin inline. Se engancha por selectores/atributos data-*.
// Adaptado del prototipo de Claude Design (que lo llevaba inline).
(function () {
  "use strict";

  var reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  // Activa las animaciones de scroll (clase .anim) solo si el usuario no ha pedido
  // reducir el movimiento. Se hace por JS (no inline) para respetar la CSP: sin JS o
  // con reduce-motion, el contenido queda visible sin animar.
  if (!reduce) document.body.classList.add("anim");

  // ---------- Menú móvil ----------
  var menuBtn = document.querySelector("[data-menu-btn]");
  var navLinks = document.querySelector("[data-nav-links]");
  if (menuBtn && navLinks) {
    menuBtn.addEventListener("click", function () {
      var open = navLinks.classList.toggle("open");
      menuBtn.setAttribute("aria-expanded", open ? "true" : "false");
    });
    // Al pulsar un enlace del menú, cerrarlo (navegación por anclas).
    navLinks.addEventListener("click", function (e) {
      if (e.target.closest("a")) navLinks.classList.remove("open");
    });
  }

  // ---------- Selector de tipo de colaboración + campos por tipo ----------
  // Cada tarjeta .tipo tiene data-tipo con el valor del enum (0/1/2).
  // Al elegir, se marca visualmente, se escribe el hidden y se ajusta el formulario:
  //   Socio        -> muestra periodicidad.
  //   Donación     -> importe puntual.
  //   Microdonación-> NO usa este formulario (se gestiona en Teaming): se ocultan los
  //                   campos y se muestra el panel con el enlace a Teaming.
  var tipos = Array.prototype.slice.call(document.querySelectorAll("[data-tipo]"));
  var hiddenTipo = document.querySelector("[data-tipo-input]");
  var campoPeriodicidad = document.querySelector("[data-campo-periodicidad]");
  var formCampos = document.querySelector("[data-form-campos]");
  var panelTeaming = document.querySelector("[data-panel-teaming]");

  // Valores del enum TipoColaboracionSolicitada (deben coincidir con el back).
  var T_SOCIO = "0", T_DONACION = "1", T_MICRO = "2";

  function aplicarTipo(valor) {
    if (hiddenTipo) hiddenTipo.value = valor;
    tipos.forEach(function (t) {
      var sel = t.getAttribute("data-tipo") === valor;
      t.classList.toggle("sel", sel);
      t.setAttribute("aria-pressed", sel ? "true" : "false");
    });

    // Microdonación se deriva a Teaming: sin campos ni envío por este formulario.
    var esMicro = valor === T_MICRO;
    if (formCampos) formCampos.hidden = esMicro;
    if (panelTeaming) panelTeaming.hidden = !esMicro;

    // Periodicidad solo para "Hacerme socio/a".
    if (campoPeriodicidad) campoPeriodicidad.hidden = valor !== T_SOCIO;
  }

  tipos.forEach(function (t) {
    var valor = t.getAttribute("data-tipo");
    t.addEventListener("click", function () { aplicarTipo(valor); });
    t.addEventListener("keydown", function (e) {
      if (e.key === "Enter" || e.key === " ") { e.preventDefault(); aplicarTipo(valor); }
    });
  });
  // Estado inicial: el que venga marcado en el hidden (o Socio por defecto).
  if (tipos.length) aplicarTipo(hiddenTipo ? (hiddenTipo.value || T_SOCIO) : T_SOCIO);

  // ---------- Selector de idioma (envía el form de cultura) ----------
  document.querySelectorAll("[data-lang-btn]").forEach(function (btn) {
    btn.addEventListener("click", function () {
      var form = btn.closest("[data-lang-form]");
      var culture = form ? form.querySelector("[data-lang-value]") : null;
      if (form && culture) { culture.value = btn.getAttribute("data-lang-btn"); form.submit(); }
    });
  });

  if (reduce) return; // el resto es puramente decorativo

  // ---------- Scroll reveal + count-up ----------
  // Reparte un retardo escalonado a los hijos de un grupo [data-stagger].
  document.querySelectorAll("[data-stagger]").forEach(function (g) {
    Array.prototype.slice.call(g.children).forEach(function (c, i) {
      if (c.hasAttribute("data-rv")) c.setAttribute("data-dl", String(i * 90));
    });
  });

  function countUp(el) {
    var target = parseFloat(el.getAttribute("data-count"));
    var suffix = el.getAttribute("data-suffix") || "";
    var t0 = null;
    function step(t) {
      if (t0 === null) t0 = t;
      var p = Math.min(1, (t - t0) / 1400);
      el.textContent = Math.round(target * (1 - Math.pow(1 - p, 3))) + suffix;
      if (p < 1) requestAnimationFrame(step);
    }
    requestAnimationFrame(step);
  }

  if ("IntersectionObserver" in window) {
    var io = new IntersectionObserver(function (entries) {
      entries.forEach(function (en) {
        if (!en.isIntersecting) return;
        var el = en.target;
        el.style.transitionDelay = (el.getAttribute("data-dl") || "0") + "ms";
        el.classList.add("in");
        var num = el.querySelector("[data-count]");
        if (num) countUp(num);
        io.unobserve(el);
      });
    }, { threshold: 0.12, rootMargin: "0px 0px -60px 0px" });
    document.querySelectorAll("[data-rv]").forEach(function (el) { io.observe(el); });
  } else {
    document.querySelectorAll("[data-rv]").forEach(function (el) { el.classList.add("in"); });
  }

  // ---------- Parallax hero ----------
  var heroBg = document.querySelector("[data-plx-hero]");
  var heroFg = document.querySelector("[data-hero-fg]");
  var tick = false;
  function onScroll() {
    if (tick) return;
    tick = true;
    requestAnimationFrame(function () {
      tick = false;
      var y = window.scrollY, vh = window.innerHeight;
      if (heroBg) heroBg.style.transform = "translateY(" + Math.min(y * 0.28, heroBg.offsetHeight * 0.18).toFixed(1) + "px)";
      if (heroFg) {
        heroFg.style.transform = "translateY(" + (y * 0.16).toFixed(1) + "px)";
        heroFg.style.opacity = String(Math.max(0, 1 - y / (vh * 0.85)));
      }
    });
  }
  window.addEventListener("scroll", onScroll, { passive: true });
  window.addEventListener("resize", onScroll);
  onScroll();
})();
