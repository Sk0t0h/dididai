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
    // El <select> del tipo lleva data-tipo-dni / data-tipo-nie con los valores que
    // representan DNI y NIE (Html.GetEnumSelectList emite valores numéricos), así que
    // se compara contra esos y no contra un nombre.
    function documentoValido(doc, tipoSelect) {
        doc = (doc || "").trim().toUpperCase();
        if (doc.length === 0) return true; // la obligatoriedad la cubre 'required'
        if (!tipoSelect) return true;
        var v = tipoSelect.value;
        if (v === tipoSelect.getAttribute("data-tipo-dni")) return dniValido(doc);
        if (v === tipoSelect.getAttribute("data-tipo-nie")) return nieValido(doc);
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

        // Casilla obligatoria (consentimiento): válida solo si el checkbox está MARCADO.
        // Se valida por element.checked, no por el value: para un bool ASP.NET añade un
        // hidden value="false" con el mismo name que haría pasar un 'required' normal.
        $.validator.addMethod("casillaobligatoria", function (value, element) {
            return element.checked === true;
        });
        $.validator.unobtrusive.adapters.addBool("casillaobligatoria");

        // Documento por tipo: lee el <select> del tipo (mismo formulario) y revalida
        // el documento cuando ese tipo cambia.
        $.validator.addMethod("documentoportipo", function (value, element, params) {
            var tipoSelect = element.form.querySelector("[data-tipo-doc]");
            return documentoValido(value, tipoSelect);
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

    // ASP.NET añade un 'required' implícito al checkbox (bool no-nullable). Con su hidden
    // value="false" da un falso positivo (lo cuenta como relleno) y trae mensaje en inglés.
    // Se retira ese data-val-required de los checkboxes que ya llevan casillaobligatoria y,
    // si unobtrusive ya había parseado el form, se fuerza el reparseo para que la regla
    // 'required' desaparezca de verdad y solo aplique 'casillaobligatoria'.
    function limpiarRequiredDeCasillas() {
        if (!window.jQuery || !window.jQuery.validator || !window.jQuery.validator.unobtrusive) return;
        var $ = window.jQuery;
        var casillas = document.querySelectorAll('input[type="checkbox"][data-val-casillaobligatoria][data-val-required]');
        if (!casillas.length) return;
        casillas.forEach(function (el) { el.removeAttribute("data-val-required"); });
        // Reparsear cada formulario afectado: se descarta el validador ya construido y se
        // vuelve a leer del DOM (ya sin el required).
        casillas.forEach(function (el) {
            var form = el.closest("form");
            if (!form) return;
            var $form = $(form);
            $form.removeData("validator").removeData("unobtrusiveValidation");
            $.validator.unobtrusive.parse(form);
        });
    }

    function init() {
        var forms = document.querySelectorAll("form");
        for (var i = 0; i < forms.length; i++) {
            if (forms[i].querySelector("[data-tel-completo]")) initTelefono(forms[i]);
        }
        initAdaptadores();
        limpiarRequiredDeCasillas();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
