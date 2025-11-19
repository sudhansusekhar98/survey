/**
 * Dashboard Chart.js
 */

'use strict';

const chartColors = {
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

};

// custom plugin to add the text in Power chart
const centerValues = {
    beforeDraw: function (chart) {
        /* if (chart.config.type === 'doughnut') {*/
        var ctx = chart.ctx;
        var width = chart.width;
        var height = chart.height;
        var centerX = width / 2;
        var centerY = height / 2;

        // Set text properties
        ctx.save();
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        var fontSize = (height / 114).toFixed(2); // Adjust the font size dynamically
        ctx.font = fontSize + "em sans-serif";

        // Custom value to display
        var customText = "75%";

        // Draw the text in the center of the chart
        ctx.fillText(customText, centerX + 60, centerY);
        ctx.restore();
        //}
    }
};

// Define a plugin to provide data labels
const pluginsLabeldata = {
    id: 'pluginsLabeldata',
    afterDatasetsDraw: function (chart) {
        var ctx = chart.ctx;

        chart.data.datasets.forEach(function (dataset, i) {
            var meta = chart.getDatasetMeta(i);
            if (!meta.hidden) {
                meta.data.forEach(function (element, index) {
                    // Draw the text in black, with the specified font
                    ctx.fillStyle = 'rgba(0, 0, 0,1)';

                    var fontSize = 14;
                    var fontStyle = 'normal';
                    var fontFamily = 'Helvetica Neue';
                    ctx.font = Chart.helpers.fontString(fontSize, fontStyle, fontFamily);

                    // Just naively convert to string for now
                    var dataString = dataset.data[index].toString() + "%";

                    // Make sure alignment settings are correct
                    ctx.textAlign = 'center';
                    ctx.textBaseline = 'middle';

                    var padding = -4;
                    var position = element.tooltipPosition();
                    ctx.fillText(dataString, position.x, position.y - (fontSize / 2) - padding);
                });
            }
        });
    }
};



// Power Auxilary Chart
function powerAuxilaryChart(chartId, labels, values) {
    // Power Auxilary Chart
    const ctxPowerAux = document.getElementById(chartId);
    new Chart(ctxPowerAux, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                data: values,
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

            }],
        },

        options: {
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        display: true,
                        drawBorder: true,
                        drawOnChartArea: true,
                        drawTicks: true,
                        color: 'rgba(200,200,200,0.4)', // subtle but visible
                        lineWidth: 1,
                        offset: false // ✅ ensures grid aligns with ticks like 0
                    },
                    ticks: {
                        stepSize: 10 // optional: controls spacing
                    }
                },
                x: {
                    display: false,
                    grid: {
                        offset: true
                    }
                }
            },
            plugins: {
                legend: {
                        display: true,
                        position: 'right',
                        labels: { font: {size: 12,},
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
                }
            },

            aspectRatio: 2.5
        }
    });

   
}

// Power Detail Chart
function powerAllDetailChart(chartId, labels, target, furnaceI, furnaceII, auxilary) {

    const ctxPowerToday = document.getElementById(chartId);
    new Chart(ctxPowerToday, {
        type: 'bar',
        data: {
            labels: labels,
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
            scales: {
                x: {
                    stacked: true,
                    grid: {
                        display: false,
                        
                    }
                },
                y: {
                    stacked: true,
                    beginAtZero: true,
                    grid: {
                        display: true,
                        drawBorder: true,
                        drawOnChartArea: true,
                        drawTicks: true,
                        color: 'rgba(200,200,200,0.4)', // subtle but visible
                        lineWidth: 1,
                        offset: false // ✅ ensures grid aligns with ticks like 0
                    },
                    ticks: {
                        stepSize: 10 // optional: controls spacing
                    }
                }
            },

            plugins: {
                tooltip: {
                    mode: 'index',
                    intersect: false
                }
            }
        }
    });
}

