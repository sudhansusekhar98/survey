// wwwroot/js/reportCharts.js
// Minimal vanilla module for two charts.
// Exposes: ReportCharts.init(opts), ReportCharts.attachPdfCapture(opts)

(function (window) {
    'use strict';

    const ReportCharts = {
        chart1: null,
        chart2: null,

        /**
         * Create both charts.
         * @param {Object} opts
         * @param {string} opts.canvasId1 - e.g. 'powerDetailChart'
         * @param {string} opts.canvasId2 - e.g. 'auxChart'
         * @param {number} [opts.height1=320] - px height for chart 1 container
         * @param {number} [opts.height2=260] - px height for chart 2 container
         * @param {Array<string>} [opts.labels1] - labels for chart 1
         * @param {Array<number>} [opts.data1]   - data for chart 1
         * @param {Array<string>} [opts.labels2] - labels for chart 2
         * @param {Array<number>} [opts.data2]   - data for chart 2
         */
        init(opts) {
            const {
                canvasId1,
                canvasId2,
                height1 = 320,
                height2 = 320,
                labels1 = ["GCP", "PUMP HOUSE", "RMHS", "COMPRESSOR", "Others"],
                data1 = [146.381, 156.301, 25.401, 25.388, 43.508],
                labels2 = ["A", "B", "C", "D", "E"],
                data2 = [22, 18, 9, 11, 13]
            } = opts || {};

            // Ensure parents have height so Chart.js can render
            ReportCharts._ensureHeight(canvasId1, height1);
            ReportCharts._ensureHeight(canvasId2, height2);

            // Build Chart 1
            const c1 = document.getElementById(canvasId1);
            ReportCharts.chart1 = new Chart(c1, {
                type: 'bar',
                data: {
                    labels: labels1,
                    datasets: [{
                        label: 'Consumption',
                        data: data1,
                        borderWidth: 1,
                        backgroundColor: [
                            'rgba(8, 71, 129,0.9)',
                            'rgba(17, 98, 170,0.9)',
                            'rgba(17, 98, 170,0.7)',
                            'rgba(36, 156, 255,1)',
                            'rgba(36, 156, 255,0.8)'
                        ]
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { display: false }
                        , title: {
                            position: "top",
                            align: 'start',
                            display: true, text: '\u00A0\u00A0\u00A0 Auxiliary-MTD Consumption',
                            font: { size: 14, style: 'italic' },
                            padding: { top: 4, bottom: 20 }
                        }
                    },
                    scales: {
                        x: { grid: { display: false } },
                        y: {
                            beginAtZero: true,
                            grid: { color: 'rgba(200,200,200,0.4)', lineWidth: 1 }
                        }
                    }
                }
            });

            // Build Chart 2
            const c2 = document.getElementById(canvasId2);
            ReportCharts.chart2 = new Chart(c2, {
                type: 'bar',
                data: {
                    labels: labels2,
                    datasets: [{
                        label: 'Aux',
                        data: data2,
                        borderWidth: 1,
                        backgroundColor: [
                            'rgba(113, 221, 55, 0.9)',
                            'rgba(3, 195, 236, 0.9)',
                            'rgba(105, 108, 255, 0.9)',
                            'rgba(255, 171, 0, 0.9)',
                            'rgba(133, 146, 163, 0.9)'
                        ]
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { display: false }
                        , title: {
                            position: "top",
                            align: 'start',
                            display: true, text: '\u00A0\u00A0\u00A0 Auxiliary-MTD Consumption',
                            font: { size: 14, style: 'italic' },
                            padding: {top: 4,bottom: 20}
                        }
    
                    },
                    scales: {
                        x: { grid: { display: false } },
                        y: { beginAtZero: true }
                    }
                }
            });
        },

        /**
         * Hook form submit to capture both charts to hidden inputs.
         * @param {Object} opts
         * @param {string} opts.formId - e.g. 'pdfForm'
         * @param {string} opts.hidden1Id - hidden input for chart1, e.g. 'hiddenChartImg1'
         * @param {string} opts.hidden2Id - hidden input for chart2, e.g. 'hiddenChartImg2'
         * @param {number} [opts.scale=2] - DPI scale for crisp PDF images
         */
        attachPdfCapture(opts) {
            const { formId, hidden1Id, hidden2Id, scale = 2 } = opts || {};
            const form = document.getElementById(formId);
            if (!form) return;

            form.addEventListener('submit', () => {
                if (ReportCharts.chart1) ReportCharts.chart1.update('none');
                if (ReportCharts.chart2) ReportCharts.chart2.update('none');

                const b64_1 = ReportCharts.chart1 ? ReportCharts._toBase64HiDpi(ReportCharts.chart1.canvas, scale) : '';
                const b64_2 = ReportCharts.chart2 ? ReportCharts._toBase64HiDpi(ReportCharts.chart2.canvas, scale) : '';

                const h1 = document.getElementById(hidden1Id);
                const h2 = document.getElementById(hidden2Id);
                if (h1) h1.value = b64_1;
                if (h2) h2.value = b64_2;
            });
        },

        // ---------- internal helpers ----------
        _ensureHeight(canvasId, heightPx) {
            const canvas = document.getElementById(canvasId);
            if (!canvas) return;
            let container = canvas.parentElement;
            // If parent has no explicit height, set it
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
