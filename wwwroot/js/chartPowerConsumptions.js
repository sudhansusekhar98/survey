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
                canvasId2,
                height1 = 100,
                height2 = 100,
                labels1 = ["One", "Two", "Three", "Four", "Five"],
                labels2 = ["A", "B", "C", "D", "E"],
                data2 = [22, 18, 9, 11, 13],
                target = [22, 18, 9, 11, 13],
                furnaceI = [22, 18, 9, 11, 13],
                furnaceII = [22, 18, 9, 11, 13],
                auxilary = [22, 18, 9, 11, 13],


             

                chartColors = {
                    blue: 'rgba(0, 123, 255, 0.4)',
                    blueBorder: 'rgba(0, 123, 255, 0.8)',
                    indigo: 'rgba(102, 16, 242, 0.4)',
                    indigoBorder: 'rgba(102, 16, 242, 0.8)',
                    purple: 'rgba(105, 108, 255, 0.4)',
                    purpleBorder: 'rgba(105, 108, 255, 0.8)',
                    pink: 'rgba(232, 62, 140, 0.4)',
                    pinkBorder: 'rgba(232, 62, 140, 0.8)',
                    red: 'rgba(255, 62, 29, 0.4)',
                    redBorder: 'rgba(255, 62, 29, 0.8)',
                    orange: 'rgba(253, 126, 20, 0.4)',
                    orangeBorder: 'rgba(253, 126, 20, 0.8)',
                    yellow: 'rgba(255, 171, 0, 0.4)',
                    yellowBorder: 'rgba(255, 171, 0, 0.8)',
                    green: 'rgba(113, 221, 55, 0.4)',
                    greenBorder: 'rgba(113, 221, 55, 0.8)',
                    teal: 'rgba(32, 201, 151, 0.4)',
                    tealBorder: 'rgba(32, 201, 151, 0.8)',
                    cyan: 'rgba(3, 195, 236, 0.4)',
                    cyanBorder: 'rgba(3, 195, 236, 0.8)',
                    black: 'rgba(34, 48, 62, 0.4)',
                    blackBorder: 'rgba(34, 48, 62, 0.8)',
                    white: 'rgba(255, 255, 255, 0.4)',
                    whiteBorder: 'rgba(255, 255, 255, 0.8)',
                    gray: 'rgba(34, 48, 62, 0.8)',
                    grayBorder: 'rgba(34, 48, 62, 0.8)',
                    gray25: 'rgba(34, 48, 62, 0.025)',
                    gray25Border: 'rgba(34, 48, 62, 0.8)',
                    gray60: 'rgba(34, 48, 62, 0.06)',
                    gray60Border: 'rgba(34, 48, 62, 0.8)',
                    gray80: 'rgba(34, 48, 62, 0.08)',
                    gray80Border: 'rgba(34, 48, 62, 0.8)',
                    primary: 'rgba(105, 108, 255, 0.4)',
                    primaryBorder: 'rgba(105, 108, 255, 0.8)',
                    secondary: 'rgba(133, 146, 163, 0.4)',
                    secondaryBorder: 'rgba(133, 146, 163, 0.8)',
                    success: 'rgba(113, 221, 55, 0.4)',
                    successBorder: 'rgba(113, 221, 55, 0.8)',
                    info: 'rgba(3, 195, 236, 0.4)',
                    infoBorder: 'rgba(3, 195, 236, 0.8)',
                    warning: 'rgba(255, 171, 0, 0.4)',
                    warningBorder: 'rgba(255, 171, 0, 0.8)',
                    danger: 'rgba(255, 62, 29, 0.4)',
                    dangerBorder: 'rgba(255, 62, 29, 0.8)',
                    light: 'rgba(219, 222, 224, 0.4)',
                    lightBorder: 'rgba(219, 222, 224, 0.8)',
                    dark: 'rgba(43, 44, 64, 0.4)',
                    darkBorder: 'rgba(43, 44, 64, 0.8)'
                }
            } = opts || {};

            ReportCharts._ensureHeight(canvasId1, height1);
            ReportCharts._ensureHeight(canvasId2, height2);

            // Chart 1: Stacked bar + line
            const c1 = document.getElementById(canvasId1);
            ReportCharts.chart1 = new Chart(c1, {
                type: 'bar',
                data: {
                    labels: labels1,
                    datasets: [
                        {
                            type: 'line',
                            label: "Target",
                            data: target,
                            backgroundColor: 'transparent',
                            borderColor: chartColors.danger,
                            pointBorderColor: 'transparent',
                            pointBackgroundColor: 'transparent',
                            borderWidth: 1,
                            borderDash: [8, 5]
                        },
                        {
                            label: "Furnace I",
                            stack: 'Stack 0',
                            data: furnaceI,
                            backgroundColor: chartColors.indigo,
                            borderColor: chartColors.indigoBorder,
                            hoverBackgroundColor: chartColors.indigoBorder,
                            borderWidth: 1
                        },
                        {
                            label: "Furnace II",
                            stack: 'Stack 0',
                            data: furnaceII,
                            backgroundColor: chartColors.purple,
                            borderColor: chartColors.purpleBorder,
                            hoverBackgroundColor: chartColors.purpleBorder,
                            borderWidth: 1
                        },
                        {
                            label: "Auxilary",
                            stack: 'Stack 0',
                            data: auxilary,
                            backgroundColor: chartColors.blue,
                            borderColor: chartColors.blueBorder,
                            hoverBackgroundColor: chartColors.blueBorder,
                            borderWidth: 1
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    aspectRatio: 1.5,
                    plugins: {
                        title: {
                            display: true,
                            position: 'top',
                            align: 'start',
                            text: '\u00A0\u00A0\u00A0 Power Consumption',
                            color:chartColors.indigo,
                            font: { size: 16, style: 'italic', weight: 'bold' },
                            padding: { top: 4, bottom: -10 }
                        },
                        tooltip: {
                            mode: 'index',
                            intersect: false
                        }
                    },
                    scales: {
                        x: {
                            stacked: true,
                            grid: { display: false }
                        },
                        y: {
                            stacked: true,
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
                        }
                    }
                }
            });

            // Chart 2: Simple bar with custom legend
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
                        hoverBackgroundColor: [
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
                        legend: {
                            display: true,
                            position: 'right',
                            labels: {
                                font: { size: 12 },
                                boxWidth: 12,
                                padding: 15,
                                generateLabels: function (chart) {
                                    const labels = chart.data.labels;
                                    const backgroundColors = chart.data.datasets[0].backgroundColor;
                                    return labels.map((label, i) => ({
                                        text: label,
                                        fillStyle: backgroundColors[i],
                                        strokeStyle: backgroundColors[i],
                                        lineWidth: 1,
                                        hidden: false,
                                        index: i
                                    }));
                                }
                            }
                        },
                        title: {
                            position: "top",
                            align: 'start',
                            display: true,
                            text: '\u00A0\u00A0\u00A0 Auxiliary-MTD Consumption',
                            color: chartColors.indigo,
                            font: { size: 16, style: 'italic' },
                            padding: { top: 4, bottom: 15 }
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