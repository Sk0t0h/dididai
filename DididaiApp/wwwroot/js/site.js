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

// Buscador del desplegable de país: al teclear en el input marcado con
// data-pais-buscador, filtra las opciones del <select data-pais-select> hermano
// mostrando solo las que contienen el texto (sin distinguir mayúsculas/acentos).
// Manejador externo (CSP): sin inline. El value guardado sigue siendo el código ISO.
document.addEventListener("input", function (e) {
    var buscador = e.target;
    if (!buscador || !buscador.hasAttribute("data-pais-buscador")) return;

    var contenedor = buscador.parentElement;
    var select = contenedor ? contenedor.querySelector("[data-pais-select]") : null;
    if (!select) return;

    var normaliza = function (s) {
        return s.toLowerCase().normalize("NFD").replace(/[̀-ͯ]/g, "");
    };
    var termino = normaliza(buscador.value.trim());

    for (var i = 0; i < select.options.length; i++) {
        var opt = select.options[i];
        var coincide = termino === "" || normaliza(opt.text).indexOf(termino) !== -1;
        opt.hidden = !coincide;
    }
});
