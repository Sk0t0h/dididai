// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Confirmación de acciones sensibles (p. ej. dar de baja un socio) sin usar
// manejadores inline (CSP): el botón lleva la clase .js-confirm y el mensaje en
// data-confirm; si el usuario cancela, se detiene el envío del formulario.
document.addEventListener("submit", function (e) {
    var boton = e.submitter;
    if (boton && boton.classList.contains("js-confirm")) {
        var mensaje = boton.getAttribute("data-confirm") || "¿Confirmar la acción?";
        if (!window.confirm(mensaje)) {
            e.preventDefault();
        }
    }
});

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
