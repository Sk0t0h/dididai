// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Confirmación de acciones sensibles (p. ej. dar de baja un socio) sin usar
// manejadores inline (CSP): el botón lleva la clase .js-confirm y el mensaje en
// data-confirm. En vez del window.confirm() nativo (feo y fuera de la marca), se
// muestra una modal propia; el envío del formulario se reanuda solo si se confirma.
(function () {
    var modal = null;      // nodo de la modal (se crea una vez, perezosamente)
    var textoEl = null;    // párrafo del mensaje
    var btnOk = null;      // botón que ejecuta la acción
    var btnCancel = null;  // botón cancelar
    var pendiente = null;  // formulario cuyo envío está a la espera de confirmación
    var pendienteBoton = null; // botón que disparó el envío (lleva asp-page-handler y asp-route-*)

    function crearModal() {
        modal = document.createElement("div");
        modal.className = "confirm-overlay";
        modal.setAttribute("hidden", "");
        modal.innerHTML =
            '<div class="confirm-box" role="dialog" aria-modal="true" aria-labelledby="confirm-txt">' +
            '  <p class="confirm-txt" id="confirm-txt"></p>' +
            '  <div class="confirm-acciones">' +
            '    <button type="button" class="btn btn-sm confirm-cancel" data-confirm-cancel>Cancelar</button>' +
            '    <button type="button" class="btn btn-sm confirm-ok" data-confirm-ok>Confirmar</button>' +
            '  </div>' +
            '</div>';
        document.body.appendChild(modal);
        textoEl = modal.querySelector(".confirm-txt");
        btnOk = modal.querySelector("[data-confirm-ok]");
        btnCancel = modal.querySelector("[data-confirm-cancel]");

        function cerrar() { modal.setAttribute("hidden", ""); pendiente = null; pendienteBoton = null; }
        function confirmar() {
            var form = pendiente;
            var boton = pendienteBoton;
            cerrar();
            // Hay que reenviar CON el submitter: el asp-page-handler y los asp-route-*
            // (id, estado…) viven en el <button>, no en el <form>. requestSubmit() sin
            // argumento los perdería y el POST caería sin handler ni parámetros.
            // El submit ya pasó el interceptor con el flag de confirmado.
            if (form) { form.dataset.confirmed = "1"; form.requestSubmit(boton || undefined); }
        }
        btnCancel.addEventListener("click", cerrar);
        btnOk.addEventListener("click", confirmar);
        // Clic fuera del cuadro = cancelar; Esc = cancelar.
        modal.addEventListener("click", function (e) { if (e.target === modal) cerrar(); });
        document.addEventListener("keydown", function (e) {
            if (!modal.hasAttribute("hidden") && e.key === "Escape") cerrar();
        });
    }

    document.addEventListener("submit", function (e) {
        var form = e.target;
        var boton = e.submitter;
        if (!boton || !boton.classList.contains("js-confirm")) return;
        // Segundo paso: ya confirmado, dejar pasar el envío.
        if (form.dataset.confirmed === "1") { delete form.dataset.confirmed; return; }

        // Primer paso: frenar el envío y pedir confirmación en la modal.
        e.preventDefault();
        if (!modal) crearModal();
        textoEl.textContent = boton.getAttribute("data-confirm") || "¿Confirmar la acción?";
        // El texto del botón de acción refleja la acción real (p. ej. "Rechazar"),
        // no un "Confirmar" genérico. Se toma de data-confirm-ok del botón que dispara.
        btnOk.textContent = boton.getAttribute("data-confirm-ok") || "Confirmar";
        pendiente = form;
        pendienteBoton = boton;
        modal.removeAttribute("hidden");
        // En acciones potencialmente destructivas, el foco va a Cancelar (opción segura),
        // no al botón que ejecuta.
        btnCancel.focus();
    });
})();

// Date pickers en Firefox de escritorio (>=109): a diferencia de Chrome/Edge, clicar la
// zona de texto de un <input type="date"> ya NO abre el calendario (comportamiento
// intencionado de Mozilla, WONTFIX); solo lo abre su icono interno, cuyo clic además NO
// burbujea hasta un listener en `document`. Para que abra clicando en CUALQUIER parte
// (texto o icono) en Firefox y Chrome/Edge, se llama a showPicker() con:
//   - fase de CAPTURA: corre antes de que el control interno del icono consuma el gesto;
//   - `pointerdown`: un único evento por gesto (evita el toggle abre/cierra de click);
//   - closest(): el target puede ser el input o un nodo interno;
//   - candado `abriendo`: descarta reentradas en el mismo tick (anti-parpadeo).
// Solo date/datetime-local (Firefox no soporta showPicker en time/month/week). CSP-safe.
(function () {
    if (!("showPicker" in HTMLInputElement.prototype)) return; // navegador antiguo: nativo
    var abriendo = false;
    document.addEventListener("pointerdown", function (e) {
        var el = e.target;
        var input = el && el.closest ? el.closest('input[type="date"], input[type="datetime-local"]') : null;
        if (!input || input.disabled || input.readOnly) return;
        if (abriendo) return;
        abriendo = true;
        try { input.showPicker(); } catch (ex) { /* el input sigue editable a mano */ }
        window.setTimeout(function () { abriendo = false; }, 0);
    }, true); // fase de captura
})();

// Menú hamburguesa del back: en pantallas estrechas la barra de gestión no cabe (6
// secciones + usuario + Salir) y desbordaba en horizontal. El botón ☰ despliega/oculta
// la nav (clase .open). Manejador externo, sin inline (CSP). aria-expanded para lectores.
(function () {
    var btn = document.querySelector("[data-admin-menu-btn]");
    var nav = document.querySelector("[data-admin-nav]");
    if (!btn || !nav) return;
    btn.addEventListener("click", function () {
        var abierto = nav.classList.toggle("open");
        btn.setAttribute("aria-expanded", abierto ? "true" : "false");
    });
})();

// Selector de idioma: al cambiar la opción, se envía el formulario que fija la
// cultura en la cookie (manejador externo, sin inline, para respetar la CSP).
document.addEventListener("change", function (e) {
    var select = e.target;
    if (select && select.hasAttribute("data-lang-select")) {
        var form = select.closest("[data-lang-form]");
        if (form) {
            form.submit();
        }
    }
});

// Combo de país (input + datalist): el usuario ve/teclea el NOMBRE; este manejador
// resuelve el código ISO correspondiente y lo escribe en el campo oculto que se
// bindea y valida (data-pais-codigo). Manejador externo (CSP): sin inline.
// Robusto: si el texto no corresponde a ningún país de la lista, el código queda
// vacío y la validación de servidor lo rechaza con un mensaje claro.
document.addEventListener("input", function (e) {
    var combo = e.target;
    if (!combo || !combo.hasAttribute("data-pais-combo")) return;

    var contenedor = combo.parentElement;
    var lista = contenedor ? contenedor.querySelector("datalist") : null;
    var oculto = contenedor ? contenedor.querySelector("[data-pais-codigo]") : null;
    if (!lista || !oculto) return;

    var valor = (combo.value || "").trim().toLowerCase();
    var codigo = "";
    for (var i = 0; i < lista.options.length; i++) {
        if ((lista.options[i].value || "").trim().toLowerCase() === valor) {
            codigo = lista.options[i].getAttribute("data-codigo") || "";
            break;
        }
    }
    oculto.value = codigo;

    // Revalidar el campo oculto tras el cambio, si hay jquery-validation activo.
    if (window.jQuery) {
        var $o = window.jQuery(oculto);
        var $f = $o.closest("form");
        if ($f.length && $f.data("validator")) { $f.validate().element($o); }
    }
});
