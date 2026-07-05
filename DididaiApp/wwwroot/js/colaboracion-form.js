// Formulario de colaboración (alta y edición). Dos responsabilidades, sin inline (CSP):
//   1) Alta: muestra u oculta los campos propios de la cuota domiciliada (IBAN,
//      periodicidad) según el tipo elegido; se engancha por los atributos data-colab-*.
//   2) Adaptador jquery-validation para el IBAN, alineado 1:1 con ValidacionIban del
//      servidor (mod-97, ISO 13616). Así el IBAN valida en vivo y el mensaje de error
//      desaparece en cuanto se corrige, sin necesidad de reenviar el formulario.
(function () {
    "use strict";

    // ---- IBAN: misma semántica que ValidacionIban (servidor) ----
    // Longitud total por país (ISO 13616); si el país no está aquí, se rechaza.
    var IBAN_LONGITUD = {
        ES: 24, GB: 22, DE: 22, FR: 27, IT: 27, PT: 25,
        NL: 18, BE: 16, IE: 22, CH: 21, AT: 20, SE: 24,
        NO: 15, DK: 18, FI: 18, PL: 28, GR: 27, LU: 20
    };

    function normalizaIban(v) {
        return (v || "").replace(/\s/g, "").trim().toUpperCase();
    }

    // mod-97 (ISO 7064) sobre cadenas largas: se procesa por trozos para no desbordar
    // el entero de JS (equivale al BigInteger del servidor).
    function mod97(iban) {
        var reordenado = iban.substring(4) + iban.substring(0, 4);
        var numerico = "";
        for (var i = 0; i < reordenado.length; i++) {
            var c = reordenado.charAt(i);
            if (c >= "0" && c <= "9") numerico += c;
            else numerico += (c.charCodeAt(0) - 65 + 10).toString(); // A=10 … Z=35
        }
        var resto = 0;
        for (var j = 0; j < numerico.length; j++) {
            resto = (resto * 10 + (numerico.charCodeAt(j) - 48)) % 97;
        }
        return resto;
    }

    function ibanValido(v) {
        var s = normalizaIban(v);
        if (s.length < 4 || s.length > 34) return false;
        if (!/^[A-Z]{2}\d{2}[A-Z0-9]+$/.test(s)) return false;
        var pais = s.substring(0, 2);
        var longitud = IBAN_LONGITUD[pais];
        if (!longitud || s.length !== longitud) return false;
        return mod97(s) === 1;
    }

    // ---- 1) Alta: mostrar/ocultar los campos de cuota domiciliada ----
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

    // ---- 2) Adaptador jquery-validation para el IBAN ----
    function initAdaptadorIban() {
        if (!window.jQuery || !window.jQuery.validator) return;
        var $ = window.jQuery;
        $.validator.addMethod("iban", function (value, element) {
            return this.optional(element) || ibanValido(value);
        });
        // El atributo [Iban] emite data-val-iban sin parámetros → adaptador booleano.
        $.validator.unobtrusive.adapters.addBool("iban");
    }

    function init() {
        var select = document.querySelector("[data-colab-tipo]");
        if (select) {
            aplicar(select);
            select.addEventListener("change", function () { aplicar(select); });
        }
        initAdaptadorIban();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
