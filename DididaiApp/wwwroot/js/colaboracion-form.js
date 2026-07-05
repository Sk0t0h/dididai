// Formulario de alta de colaboración: muestra u oculta los campos propios de la
// cuota domiciliada (IBAN, periodicidad) según el tipo elegido. Sin inline (CSP):
// se engancha por los atributos data-colab-*.
(function () {
    "use strict";

    function aplicar(select) {
        // El valor que representa "cuota domiciliada" viene del data-* del select
        // (Html.GetEnumSelectList emite el valor numérico del enum, no el nombre).
        var valorCuota = select.getAttribute("data-colab-cuota-val");
        var esCuota = select.value === valorCuota;
        var campos = document.querySelectorAll("[data-colab-cuota]");
        for (var i = 0; i < campos.length; i++) {
            campos[i].hidden = !esCuota;
            // Deshabilitar los inputs ocultos evita que se envíen y que jquery-validation
            // los valide cuando no aplican; al mostrarlos se rehabilitan.
            var controles = campos[i].querySelectorAll("input, select");
            for (var j = 0; j < controles.length; j++) {
                controles[j].disabled = !esCuota;
            }
        }
    }

    function init() {
        var select = document.querySelector("[data-colab-tipo]");
        if (!select) return;
        aplicar(select);
        select.addEventListener("change", function () { aplicar(select); });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
