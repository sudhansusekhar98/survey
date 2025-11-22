(function (window) {
    'use strict';

    // --- configure label → color mapping here (edit as needed) ---
    const LABEL_COLORS = {
        "Quartz": {
            background: "rgba(255,192,203,0.8)",
            border: "rgba(255,192,203,1)",
            hover: "rgba(255,192,203,1)"
        },
        "B-Charcoal": {
            background: "rgba(0,0,0,0.4)",
            border: "rgba(0,0,0,0.8)",
            hover: "rgba(0,0,0,0.8)"
        },
        "TN-Charcoal": {
            background: "rgba(0,0,0,0.4)",
            border: "rgba(0,0,0,0.8)",
            hover: "rgba(0,0,0,0.8)"
        },
        "N-Charcoal": {
            background: "rgba(0,0,0,0.4)",
            border: "rgba(0,0,0,0.8)",
            hover: "rgba(0,0,0,0.8)"
        },
        "R-Charcoal": {
            background: "rgba(0,0,0,0.4)",
            border: "rgba(0,0,0,0.8)",
            hover: "rgba(0,0,0,0.8)"
        },
        "Mill Scale": {
            background: "rgba(176,196,222,0.9)",
            border: "rgba(176,196,222,1)",
            hover: "rgba(176,196,222,1)"
        },
        "Lam Coke": {
            background: "rgba(0,0,0,0.6)",
            border: "rgba(0,0,0,0.8)",
            hover: "rgba(0,0,0,0.8)"
        },
        "Semi Coke": {
            background: "rgba(0,0,0,0.8)",
            border: "rgba(0,0,0,1)",
            hover: "rgba(0,0,0,1)"
        },
        "Scrap": {
            background: "rgba(255, 62, 29, 0.4)",
            border: "rgba(255, 62, 29, 1)",
            hover: "rgba(255, 62, 29, 1)"
        },
        "Wood Chip": {
            background: "rgba(139,105,20,0.6)",
            border: "rgba(139,105,20,1)",
            hover: "rgba(139,105,20,1)"
        }
    };

    const ReportCharts = {
        chart: null,

        /**
         * Create the chart.
         */
        init(opts) {
            const {
                canvasId,
                height = 100,
                labels = ["A", "B", "C", "D", "E"],
                data = [22, 18, 9, 11, 13],

                // slimmed color set (only what's used)
                chartColors = {
                    indigo: 'rgba(102, 16, 242, 0.4)',
                    indigoBorder: 'rgba(102, 16, 242, 0.8)',
                    primary: 'rgba(105, 108, 255, 0.4)',
                    primaryBorder: 'rgba(105, 108, 255, 0.8)',
                    secondary: 'rgba(133, 146, 163, 0.4)',
                    secondaryBorder: 'rgba(133, 146, 163, 0.8)',
                    success: 'rgba(113, 221, 55, 0.4)',
                    successBorder: 'rgba(113, 221, 55, 0.8)',
                    info: 'rgba(3, 195, 236, 0.4)',
                    infoBorder: 'rgba(3, 195, 236, 0.8)',
                    warning: 'rgba(255, 171, 0, 0.4)',
                    warningBorder: 'rgba(255, 171, 0, 0.8)'
                }
            } = opts || {};

            ReportCharts._ensureHeight(canvasId, height);

            const canvas = document.getElementById(canvasId);
            if (!canvas) return;

            // If chart exists already, destroy it to avoid duplicates
            if (ReportCharts.chart) {
                try { ReportCharts.chart.destroy(); } catch (e) { /* ignore */ }
                ReportCharts.chart = null;
            }

            // Normalizer to match label keys robustly (trim and use exact case by default)
            const normalizeLabel = (lbl) => (typeof lbl === 'string' ? lbl.trim() : lbl);

            // Build per-label color arrays using LABEL_COLORS map with fallbacks.
            const fallback = {
                background: chartColors.primary,
                border: chartColors.primaryBorder,
                hover: chartColors.primaryBorder
            };

            const backgroundColorArr = labels.map(lbl => {
                const key = normalizeLabel(lbl);
                const m = LABEL_COLORS[key];
                return (m && m.background) ? m.background : fallback.background;
            });

            const borderColorArr = labels.map(lbl => {
                const key = normalizeLabel(lbl);
                const m = LABEL_COLORS[key];
                return (m && m.border) ? m.border : fallback.border;
            });

            const hoverBorderColorArr = labels.map(lbl => {
                const key = normalizeLabel(lbl);
                const m = LABEL_COLORS[key];
                return (m && m.hover) ? m.hover : fallback.hover;
            });

            ReportCharts.chart = new Chart(canvas, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Aux',
                        data: data,
                        borderWidth: 1,
                        // now set per-label colors
                        backgroundColor: backgroundColorArr,
                        borderColor: borderColorArr,
                        hoverBorderColor: hoverBorderColorArr
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        // Custom legend: generate per-label entries and make them clickable
                        legend: {
                            display: true,
                            position: 'right',
                            labels: {
                                font: { size: 12 },
                                boxWidth: 12,
                                padding: 15,
                                generateLabels(chart) {
                                    const lbls = chart.data.labels || [];
                                    const ds = chart.data.datasets[0] || {};
                                    const bg = ds.backgroundColor || [];
                                    const br = ds.borderColor || [];
                                    const meta = chart.getDatasetMeta(0);
                                    return lbls.map((txt, i) => ({
                                        text: txt,
                                        fillStyle: Array.isArray(bg) ? bg[i] : bg,
                                        strokeStyle: Array.isArray(br) ? br[i] : br,
                                        hidden: !!(meta && meta.data && meta.data[i] && meta.data[i].hidden),
                                        index: i
                                    }));
                                }
                            },
                            onClick: function (e, legendItem, legend) {
                                const index = legendItem.index;
                                const chart = legend.chart;
                                const meta = chart.getDatasetMeta(0);
                                if (!meta || !meta.data || !meta.data[index]) return;

                                // toggle hidden flag on the element (bar)
                                meta.data[index].hidden = !meta.data[index].hidden;
                                chart.update();
                            }
                        },

                        title: {
                            position: "top",
                            align: 'start',
                            display: true,
                            text: '\u00A0\u00A0\u00A0 Ground Hopper : Feeding',
                            color: chartColors.indigo,
                            font: { size: 16, style: 'italic' },
                            padding: { top: 4, bottom: 15 }
                        },

                        tooltip: {
                            callbacks: {},
                            filter: function (tooltipItem) {
                                const meta = tooltipItem.chart.getDatasetMeta(tooltipItem.datasetIndex);
                                const el = meta && meta.data && meta.data[tooltipItem.dataIndex];
                                return !(el && el.hidden);
                            }
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            grid: {
                                display: true,
                                drawBorder: true,
                                drawOnChartArea: true,
                                drawTicks: true,
                                color: 'rgba(200,200,200,0.4)',
                                lineWidth: 1,
                                offset: false
                            },
                            ticks: { stepSize: 10 }
                        },
                        x: {
                            display: false,
                            grid: { offset: true }
                        }
                    }
                }
            });
        },

        /**
         * Hook form submit to capture chart to hidden input(s).
         * Accepts either `hiddenId` (preferred) or `hidden2Id` (legacy).
         */
        attachPdfCapture(opts) {
            const { formId, hiddenId, hidden2Id, scale = 2 } = opts || {};
            const form = document.getElementById(formId);
            if (!form) return;

            const hid = hiddenId || hidden2Id;
            if (!hid) return;

            form.addEventListener('submit', () => {
                if (ReportCharts.chart) ReportCharts.chart.update('none');

                const b64 = ReportCharts.chart ? ReportCharts._toBase64HiDpi(ReportCharts.chart.canvas, scale) : '';
                const h = document.getElementById(hid);
                if (h) h.value = b64;
            });
        },

        // ---------- internal helpers ----------
        _ensureHeight(canvasId, heightPx) {
            const canvas = document.getElementById(canvasId);
            if (!canvas) return;
            let container = canvas.parentElement;
            if (container && !container.style.height) {
                container.style.position = 'relative';
                container.style.height = heightPx + 'px';
            }
        },

        _toBase64HiDpi(srcCanvas, scale) {
            const off = document.createElement('canvas');
            off.width = srcCanvas.width * scale;
            off.height = srcCanvas.height * scale;
            const offctx = off.getContext('2d');
            offctx.scale(scale, scale);
            offctx.drawImage(srcCanvas, 0, 0);
            return off.toDataURL('image/png');
        }
    };

    window.ReportCharts = ReportCharts;
})(window);
