(function (window) {
    'use strict';

    const ReportCharts = {
        chart1: null,
        chart2: null,

        /**
         * Create both charts.
         */
        init(opts) {
            const {


                /*  first Chart */
                canvasId1,
                height1 = 100,
                labels1 = ["One", "Two", "Three", "Four", "Five"],
                Quartz = [22, 18, 9, 11, 13],
                Charcoal = [22, 18, 9, 11, 13],
                MillScale = [22, 18, 9, 11, 13],
                LamCoke = [22, 18, 9, 11, 13],
                WoodChip = [22, 18, 9, 11, 13],




                /* second Chart */
                canvasId2,
                height2 = 100,
                labels2 = ["A", "B", "C", "D", "E"],
                data2 = [22, 18, 9, 11, 13],
                daysLeft = 0,
                disDate = '2025-06-04'

            } = opts || {};

            ReportCharts._ensureHeight(canvasId1, height1);
            ReportCharts._ensureHeight(canvasId2, height2);

            // Chart 1: Stacked bar + line
            const c1 = document.getElementById(canvasId1);
            if (c1) {
                if (ReportCharts.chart1) {
                    try { ReportCharts.chart1.destroy(); } catch (e) { /* ignore */ }
                }

                ReportCharts.chart1 = new Chart(c1, {
                    type: 'line',
                    data: {
                        labels: labels1,
                        datasets: [
                            {
                                label: "Quartz",
                                data: Quartz,
                                borderColor: 'rgba(255,192,203,1)',
                                borderWidth: 1,
                                tension: 0.3,
                                fill: true,
                                backgroundColor: 'rgba(255,192,203,0.1)',
                                pointRadius: 2,
                                pointBorderColor: 'transparent',
                                pointBackgroundColor: 'transparent'
                            },
                            {
                                label: "Mill Scale",
                                data: MillScale,
                                borderColor: 'rgba(30,144,255,1)',
                                borderWidth: 1,
                                tension: 0.3,
                                fill: true,
                                backgroundColor: 'rgba(30,144,255,0.3)',
                                pointBorderColor: 'transparent',
                                pointBackgroundColor: 'transparent'
                            },
                            {
                                label: "Charcoal",
                                data: Charcoal,
                                borderColor: 'rgba(128,128,128,1)',
                                borderWidth: 1,
                                tension: 0.3,
                                fill: true,
                                backgroundColor: 'rgba(128,128,128,0.2)',
                                pointBorderColor: 'transparent',
                                pointBackgroundColor: 'transparent'
                            },
                            {
                                label: "Lam Coke",
                                data: LamCoke,
                                borderColor: 'rgba(0,0,0,1)',
                                borderWidth: 1,
                                tension: 0.3,
                                fill: true,
                                backgroundColor: 'rgba(0,0,0,0.3)',
                                pointBorderColor: 'transparent',
                                pointBackgroundColor: 'transparent'
                            },
                            {
                                label: "Wood Chip",
                                data: WoodChip,
                                borderColor: 'rgba(139,105,20,1)',
                                borderWidth: 1,
                                tension: 0.3,
                                fill: true,
                                backgroundColor: 'rgba(139,105,20,0.6)',
                                pointBorderColor: 'transparent',
                                pointBackgroundColor: 'transparent'
                            }
                        ]
                    },
                    options: {
                        plugins: {
                            tooltip: {
                                mode: 'index',
                                intersect: false
                            },
                            legend: {
                                display: true,
                                position: 'top',
                                labels: {
                                    boxWidth: 30,
                                    padding: 10
                                }
                            },
                            title: {
                                position: "top",
                                align: 'start',
                                display: true,
                                text: '\u00A0\u00A0 Daily RM Consumption Trend',
                                color: 'rgba(102,16,242,0.4)',
                                font: { size: 16, style: 'italic' },
                                padding: { top: 4, bottom: -10 }
                            }
                        },

                        // <-- SCALES MUST BE HERE (top-level under options)
                        scales: {
                            y: {
                                beginAtZero: true,
                                // Ensure tick interval is 10
                                ticks: {
                                    stepSize: 10
                                },
                                grid: {
                                    display: true,
                                    drawBorder: true,
                                    drawOnChartArea: true,
                                    drawTicks: true,
                                    color: 'rgba(200,200,200,0.4)',
                                    lineWidth: 1,
                                    offset: false
                                }
                            },
                            x: {
                                display: true,
                                // remove vertical gridlines
                                grid: {
                                    display: false,
                                    offset: true
                                }
                            }
                        },

                        responsive: true,
                        maintainAspectRatio: false
                    }
                });
            }


            // Chart 2: Simple bar with DaysLeft labels on each bar
            const c2 = document.getElementById(canvasId2);
            if (c2) {
                if (ReportCharts.chart2) { try { ReportCharts.chart2.destroy(); } catch (e) { } }

                ReportCharts.chart2 = new Chart(c2, {
                    type: 'bar',
                    data: {
                        labels: labels2,
                        datasets: [{
                            label: 'Aux',
                            data: data2,
                            borderWidth: 1,
                            backgroundColor: [
                                'rgba(255,192,203,0.8)',
                                'rgba(128,128,128,0.8)',
                                'rgb(192,192,192,1)',
                                'rgba(0,0,0,0.8)',
                                'rgba(139,105,20,0.8)'
                            ],
                            borderColor: [
                                'rgba(255,192,203,1)',
                                'rgba(128,128,128,1)',
                                'rgb(192,192,192,1)',
                                'rgba(0,0,0,1)',
                                'rgba(139,105,20,1)'
                            ],
                            hoverBackgroundColor: [
                                'rgba(255,192,203,1)',
                                'rgba(128,128,128,1)',
                                'rgba(30,144,255,1)',
                                'rgba(0,0,0,1)',
                                'rgba(139,105,20,1)'
                            ],
                            // put datalabels config inside dataset
                            datalabels: {
                                clamp: true,
                                clip: false,
                                font: { weight: '100', size: 12 },
                                formatter: function (value, context) {
                                    const num = (typeof value === 'number') ? value.toLocaleString() : String(value);
                                    const d = (Array.isArray(daysLeft) && daysLeft[context.dataIndex] !== undefined)
                                        ? String(daysLeft[context.dataIndex])
                                        : '';
                                    const unit = 'MT';
                                    return d ? `${num} ${unit} \u2022 ${d} Days` : `${num} ${unit}`;
                                },
                                // dynamic placement + color based on bar length vs axis max
                                anchor: function (ctx) {
                                    const val = Number(ctx.dataset.data[ctx.dataIndex]) || 0;
                                    const max = (ctx.chart.scales.x && ctx.chart.scales.x.max) || Math.max(...ctx.dataset.data);
                                    return val >= (max * 0.25) ? 'center' : 'end';
                                },
                                align: function (ctx) {
                                    const val = Number(ctx.dataset.data[ctx.dataIndex]) || 0;
                                    const max = (ctx.chart.scales.x && ctx.chart.scales.x.max) || Math.max(...ctx.dataset.data);
                                    return val >= (max * 0.25) ? 'center' : 'right';
                                },
                                color: function (ctx) {
                                    const val = Number(ctx.dataset.data[ctx.dataIndex]) || 0;
                                    const max = (ctx.chart.scales.x && ctx.chart.scales.x.max) || Math.max(...ctx.dataset.data);
                                    return val >= (max * 0.25) ? '#ffffff' : '#000000';
                                },
                                offset: function (ctx) {
                                    const val = Number(ctx.dataset.data[ctx.dataIndex]) || 0;
                                    const max = (ctx.chart.scales.x && ctx.chart.scales.x.max) || Math.max(...ctx.dataset.data);
                                    return val >= (max * 0.25) ? 0 : 8;
                                }
                            }

                        }]
                    },
                    options: {
                        indexAxis: 'y', // keep as horizontal bars
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: { display: false },
                            title: {
                                position: "top",
                                align: 'start',
                                display: true,
                                text: '\u00A0\u00A0\u00A0 Stock As on date: ' + disDate,
                                color: 'rgba(102,16,242,0.4)',
                                font: { size: 16, style: 'italic' },
                                padding: { top: 4, bottom: 5 }
                            }
                        },
                        scales: {
                            y: {
                                beginAtZero: true,
                                grid: { display: false, offset: true }
                            },
                            x: {
                                display: true,
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
                            }
                        }
                    },
                    plugins: [ChartDataLabels] // v5: plugin passed directly here, no register needed
                });
            }

        },

        /**
         * Hook form submit to capture both charts to hidden inputs.
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
