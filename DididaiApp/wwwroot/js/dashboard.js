// Dashboards del módulo económico con Chart.js (servido local, sin CDN). Sin inline
// (CSP): los datos viajan en atributos data-* de cada <canvas>; este fichero externo
// los lee y crea las gráficas. No hay <script> con datos incrustados.
(function () {
    "use strict";

    // Paleta accesible y coherente (evita depender de los colores por defecto). El primer
    // color es el naranja de marca DIDIDAI (ingresos = la serie/segmento principal).
    var COLORES = ["#f7941d", "#20c997", "#ffc107", "#dc3545", "#6f42c1", "#6c757d"];

    function leerDatos(canvas) {
        try { return JSON.parse(canvas.getAttribute("data-chart") || "null"); }
        catch (e) { return null; }
    }

    function crear(canvas) {
        if (typeof Chart === "undefined") return;
        var cfg = leerDatos(canvas);
        if (!cfg || !cfg.tipo) return;

        var labels = cfg.labels || [];
        var datasets;

        if (cfg.series && cfg.series.length) {
            // Multi-serie (p. ej. proyección: ingresos y gastos). Un color por serie.
            datasets = cfg.series.map(function (s, i) {
                var color = COLORES[i % COLORES.length];
                return {
                    label: s.etiqueta || "",
                    data: s.valores || [],
                    backgroundColor: color,
                    borderColor: color,
                    fill: false,
                    tension: 0.3,
                };
            });
        } else {
            // Serie única.
            datasets = [{
                label: cfg.etiqueta || "",
                data: cfg.valores || [],
                backgroundColor: (cfg.tipo === "line")
                    ? "rgba(247,148,29,0.15)"
                    : labels.map(function (_, i) { return COLORES[i % COLORES.length]; }),
                borderColor: (cfg.tipo === "line") ? "#f7941d" : undefined,
                fill: cfg.tipo === "line",
                tension: 0.3,
            }];
        }

        var multi = datasets.length > 1;
        new Chart(canvas, {
            type: cfg.tipo,
            data: { labels: labels, datasets: datasets },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: multi || cfg.tipo === "doughnut" || cfg.tipo === "pie" },
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
