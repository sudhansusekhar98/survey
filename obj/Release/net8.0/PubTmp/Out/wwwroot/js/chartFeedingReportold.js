(function (window) {
    'use strict';

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

            ReportCharts.chart = new Chart(canvas, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Aux',
                        data: data,
                        borderWidth: 1,
                        backgroundColor: [
                            chartColors.success,
                            chartColors.info,
                            chartColors.primary,
                            chartColors.warning,
                            chartColors.secondary
                        ],
                        borderColor: [
                            chartColors.successBorder,
                            chartColors.infoBorder,
                            chartColors.primaryBorder,
                            chartColors.warningBorder,
                            chartColors.secondaryBorder
                        ],
                        // allow per-element hover color by using hoverBorderColor
                        hoverBorderColor: [
                            chartColors.successBorder,
                            chartColors.infoBorder,
                            chartColors.primaryBorder,
                            chartColors.warningBorder,
                            chartColors.secondaryBorder
                        ]
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
                                // generate one legend item per label (not per dataset)
                                generateLabels(chart) {
                                    const lbls = chart.data.labels || [];
                                    const bg = (chart.data.datasets[0] && chart.data.datasets[0].backgroundColor) || [];
                                    const meta = chart.getDatasetMeta(0);
                                    return lbls.map((txt, i) => ({
                                        text: txt,
                                        fillStyle: Array.isArray(bg) ? bg[i] : bg,
                                        strokeStyle: Array.isArray(bg) ? bg[i] : bg,
                                        hidden: !!(meta && meta.data && meta.data[i] && meta.data[i].hidden),
                                        index: i
                                    }));
                                }
                            },
                            // toggles visibility of the specific bar (per-index)
                            onClick: function (e, legendItem, legend) {
                                const index = legendItem.index;
                                const chart = legend.chart;
                                const meta = chart.getDatasetMeta(0);
                                if (!meta || !meta.data || !meta.data[index]) return;

                                // toggle hidden flag on the element (bar)
                                meta.data[index].hidden = !meta.data[index].hidden;

                                // When hiding/showing, we may want to dim label appearance:
                                // Chart will re-render automatically on update()
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
                            // ensure tooltips skip hidden elements
                            callbacks: {},
                            filter: function (tooltipItem) {
                                // tooltipItem has datasetIndex and dataIndex
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
