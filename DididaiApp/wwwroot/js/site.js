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
