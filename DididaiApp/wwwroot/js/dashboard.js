// Dashboards del módulo económico con Chart.js (servido local, sin CDN). Sin inline
// (CSP): los datos viajan en atributos data-* de cada <canvas>; este fichero externo
// los lee y crea las gráficas. No hay <script> con datos incrustados.
(function () {
    "use strict";

    // Paleta accesible y coherente (evita depender de los colores por defecto).
    var COLORES = ["#0d6efd", "#20c997", "#ffc107", "#dc3545", "#6f42c1", "#6c757d"];

    function leerDatos(canvas) {
        try { return JSON.parse(canvas.getAttribute("data-chart") || "null"); }
        catch (e) { return null; }
    }

    function crear(canvas) {
        if (typeof Chart === "undefined") return;
        var cfg = leerDatos(canvas);
        if (!cfg || !cfg.tipo) return;

        var labels = cfg.labels || [];
        var valores = cfg.valores || [];
        var dataset = {
            label: cfg.etiqueta || "",
            data: valores,
            backgroundColor: (cfg.tipo === "line")
                ? "rgba(13,110,253,0.15)"
                : labels.map(function (_, i) { return COLORES[i % COLORES.length]; }),
            borderColor: (cfg.tipo === "line") ? "#0d6efd" : undefined,
            fill: cfg.tipo === "line",
            tension: 0.3,
        };

        new Chart(canvas, {
            type: cfg.tipo,
            data: { labels: labels, datasets: [dataset] },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: cfg.tipo === "doughnut" || cfg.tipo === "pie" },
                },
                scales: (cfg.tipo === "bar" || cfg.tipo === "line")
                    ? { y: { beginAtZero: true } }
                    : {},
            },
        });
    }

    function init() {
        var canvases = document.querySelectorAll("canvas[data-chart]");
        for (var i = 0; i < canvases.length; i++) crear(canvases[i]);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init);
    } else {
        init();
    }
})();
