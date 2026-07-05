// Validación de cliente del formulario de socio, alineada 1:1 con la de servidor.
// Sin código inline (CSP): este fichero se engancha por selector/data-*.
//
// Dos responsabilidades:
//   1) Teléfono: UI con prefijo (select) + número (input) que se combinan en un
//      campo oculto Telefono en formato E.164. En edición, descompone el valor
//      existente. Preselecciona el prefijo según el país de residencia (comodidad).
//   2) Adaptadores jquery-validation para las reglas custom del servidor
//      (telefonoe164 y documentoportipo), de modo que la MISMA regla valida en vivo.
(function () {
    "use strict";

    // ---- Utilidades compartidas con el servidor (misma semántica) ----
    var LETRAS_DNI = "TRWAGMYFPDXBNJZSQVHLCKE";

    function normalizaTelefono(v) {
        return (v || "").trim().replace(/[\s\-.()]/g, "");
    }
    function esE164(v) {
        return /^\+[1-9]\d{7,14}$/.test(normalizaTelefono(v));
    }
    function letraDniCorrecta(numero8, letra) {
        return LETRAS_DNI.charAt(parseInt(numero8, 10) % 23) === letra.toUpperCase();
    }
    function dniValido(doc) {
        var m = /^(\d{8})([A-Za-z])$/.exec(doc);
        return !!m && letraDniCorrecta(m[1], m[2]);
    }
    function nieValido(doc) {
        var m = /^([XYZxyz])(\d{7})([A-Za-z])$/.exec(doc);
        if (!m) return false;
        var pre = { X: "0", Y: "1", Z: "2" }[m[1].toUpperCase()];
        return letraDniCorrecta(pre + m[2], m[3]);
    }
    // Valida el documento según el tipo declarado (mismo criterio que el servidor).
    function documentoValido(doc, tipoTexto) {
        doc = (doc || "").trim().toUpperCase();
        if (doc.length === 0) return true; // la obligatoriedad la cubre 'required'
        // tipoTexto es el value del <select> del enum: "DniEspanol","Nie","Pasaporte","Otro"
        if (tipoTexto === "DniEspanol") return dniValido(doc);
        if (tipoTexto === "Nie") return nieValido(doc);
        return true; // Pasaporte / Otro: laxo
    }

    // ---- 1) Teléfono: componer / descomponer prefijo + número ----
    function initTelefono(scope) {
        var prefijo = scope.querySelector("[data-tel-prefijo]");
        var numero = scope.querySelector("[data-tel-numero]");
        var completo = scope.querySelector("[data-tel-completo]");
        var paisCombo = scope.querySelector("[data-pais-combo]");
        var paisCodigo = scope.querySelector("[data-pais-codigo]");
        if (!prefijo || !numero || !completo) return;

        // Descomponer el valor E.164 existente (edición) en prefijo + resto.
        var actual = normalizaTelefono(completo.value);
        if (actual && actual.charAt(0) === "+") {
            var encontrado = null;
            for (var i = 0; i < prefijo.options.length; i++) {
                var code = prefijo.options[i].value; // p.ej. "+34"
                if (code && actual.indexOf(code) === 0) {
                    if (!encontrado || code.length > encontrado.length) encontrado = code;
                }
            }
            if (encontrado) {
                prefijo.value = encontrado;
                numero.value = actual.substring(encontrado.length);
            } else {
                numero.value = actual; // no reconocido: se muestra tal cual
            }
        }

        function componer() {
            var num = (numero.value || "").replace(/[\s\-.()]/g, "");
            completo.value = num ? (prefijo.value + num) : "";
            // Disparar validación del campo oculto tras recomponer.
            if (window.jQuery) {
                var $c = window.jQuery(completo);
                var $f = $c.closest("form");
                if ($f.length && $f.data("validator")) $f.validate().element($c);
            }
        }
        prefijo.addEventListener("change", componer);
        numero.addEventListener("input", componer);
        componer(); // fijar el estado inicial

        // Comodidad: al elegir país de residencia (combo), preseleccionar su prefijo si
        // el número aún está vacío (no pisar una elección deliberada del usuario). El
        // código ISO se lee del campo oculto que sincroniza site.js tras el input.
        if (paisCombo && paisCodigo) {
            paisCombo.addEventListener("input", function () {
                if (numero.value) return;
                var pais = paisCodigo.value; // ya resuelto por el manejador de site.js
                for (var i = 0; i < prefijo.options.length; i++) {
                    if (prefijo.options[i].getAttribute("data-pais") === pais) {
                        prefijo.value = prefijo.options[i].value;
                        componer();
                        break;
                    }
                }
            });
        }
    }

    // ---- 2) Adaptadores jquery-validation (misma regla que el servidor) ----
    function initAdaptadores() {
        if (!window.jQuery || !window.jQuery.validator) return;
        var $ = window.jQuery;

        // Teléfono E.164
        $.validator.addMethod("telefonoe164", function (value, element) {
            return this.optional(element) || esE164(value);
        });
        $.validator.unobtrusive.adapters.addBool("telefonoe164");

        // Documento por tipo: lee el <select> del tipo (mismo formulario) y revalida
        // el documento cuando ese tipo cambia.
        $.validator.addMethod("documentoportipo", function (value, element, params) {
            var tipoSelect = element.form.querySelector("[data-tipo-doc]");
            var tipo = tipoSelect ? tipoSelect.value : "";
            return documentoValido(value, tipo);
        });
        $.validator.unobtrusive.adapters.add("documentoportipo", ["tipocampo", "patrondni", "patronnie"], function (options) {
            options.rules["documentoportipo"] = options.params;
            options.messages["documentoportipo"] = options.message;
        });

        // Al cambiar el tipo de documento, revalidar el campo del documento en vivo.
        document.addEventListener("change", function (e) {
            if (e.target && e.target.hasAttribute("data-tipo-doc")) {
                var form = e.target.form;
                var doc = form ? form.querySelector("[data-documento]") : null;
                if (doc && window.jQuery) {
                    var $form = $(form);
                    if ($form.data("validator")) $form.validate().element(doc);
                }
            }
        });
    }

    function init() {
        var forms = document.querySelectorAll("form");
        for (var i = 0; i < forms.length; i++) {
            if (forms[i].querySelector("[data-tel-completo]")) initTelefono(forms[i]);
        }
        initAdaptadores();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
