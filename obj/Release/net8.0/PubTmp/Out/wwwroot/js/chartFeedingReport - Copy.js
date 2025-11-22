/**
 * Dashboard Chart.js
 */

'use strict';

const chartColors = {
    blue: 'rgba(0, 123, 255, 0.4)',
    blueBorder: 'rgba(0, 123, 255, 1)',

    indigo: 'rgba(102, 16, 242, 0.4)',
    indigoBorder: 'rgba(102, 16, 242, 1)',

    purple: 'rgba(105, 108, 255, 0.4)',
    purpleBorder: 'rgba(105, 108, 255, 1)',

    pink: 'rgba(232, 62, 140, 0.4)',
    pinkBorder: 'rgba(232, 62, 140, 1)',

    red: 'rgba(255, 62, 29, 0.4)',
    redBorder: 'rgba(255, 62, 29, 1)',

    orange: 'rgba(253, 126, 20, 0.4)',
    orangeBorder: 'rgba(253, 126, 20, 1)',

    yellow: 'rgba(255, 171, 0, 0.4)',
    yellowBorder: 'rgba(255, 171, 0, 1)',

    green: 'rgba(113, 221, 55, 0.4)',
    greenBorder: 'rgba(113, 221, 55, 1)',

    teal: 'rgba(32, 201, 151, 0.4)',
    tealBorder: 'rgba(32, 201, 151, 1)',

    cyan: 'rgba(3, 195, 236, 0.4)',
    cyanBorder: 'rgba(3, 195, 236, 1)',

    black: 'rgba(34, 48, 62, 0.4)',
    blackBorder: 'rgba(34, 48, 62, 1)',

    white: 'rgba(255, 255, 255, 0.4)',
    whiteBorder: 'rgba(255, 255, 255, 1)',

    gray: 'rgba(34, 48, 62, 1)',
    grayBorder: 'rgba(34, 48, 62, 1)',

    gray25: 'rgba(34, 48, 62, 0.025)',
    gray25Border: 'rgba(34, 48, 62, 1)',

    gray60: 'rgba(34, 48, 62, 0.06)',
    gray60Border: 'rgba(34, 48, 62, 1)',

    gray80: 'rgba(34, 48, 62, 0.08)',
    gray80Border: 'rgba(34, 48, 62, 0.8)',

    primary: 'rgba(105, 108, 255, 0.4)',
    primaryBorder: 'rgba(105, 108, 255, 1)',

    secondary: 'rgba(133, 146, 163, 0.4)',
    secondaryBorder: 'rgba(133, 146, 163, 1)',

    success: 'rgba(113, 221, 55, 0.4)',
    successBorder: 'rgba(113, 221, 55, 1)',

    info: 'rgba(3, 195, 236, 0.4)',
    infoBorder: 'rgba(3, 195, 236, 1)',

    warning: 'rgba(255, 171, 0, 0.4)',
    warningBorder: 'rgba(255, 171, 0, 1)',

    danger: 'rgba(255, 62, 29, 0.4)',
    dangerBorder: 'rgba(255, 62, 29, 1)',

    light: 'rgba(219, 222, 224, 0.4)',
    lightBorder: 'rgba(219, 222, 224, 1)',

    dark: 'rgba(43, 44, 64, 0.4)',
    darkBorder: 'rgba(43, 44, 64, 1)'

};

//'rgba(255,192,203,0.8)',
//    'rgba(0,0,0,0.4)',
//    'rgba(176,196,222,0.9)',
//    'rgba(0,0,0,0.6)',
//    'rgba(0,0,0,0.8)',
//    'rgba(139,105,20,0.6)',
function initFeedingChart(chartId, labels, values) {
    const ctxPowerAux = document.getElementById(chartId);

    // Define label → color mapping
    const labelColors = {
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

    // Generate arrays of colors based on labels
    const backgroundColors = labels.map(lbl => labelColors[lbl]?.background || "rgba(105, 108, 255, 0.4)");
    const borderColors = labels.map(lbl => labelColors[lbl]?.border || "rgba(105, 108, 255, 1)");
    const hoverColors = labels.map(lbl => labelColors[lbl]?.hover || "rgba(105, 108, 255, 1)");

    new Chart(ctxPowerAux, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                borderWidth: 1,
                backgroundColor: backgroundColors,
                borderColor: borderColors,
                hoverBackgroundColor: hoverColors
            }],
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: { stepSize: 10 },
                    grid: { color: 'rgba(200,200,200,0.4)' }
                },
                x: { display: false }
            },
            plugins: {
                legend: {
                    display: true,
                    position: 'right',
                    labels: {
                        font: { size: 12, }, boxWidth: 12, padding: 15,

                        generateLabels: function (chart) {
                            const labels = chart.data.labels;
                            const bg = chart.data.datasets[0].backgroundColor;
                            return labels.map((label, i) => ({
                                text: label,
                                fillStyle: bg[i],
                                strokeStyle: bg[i],
                                lineWidth: 1,
                                hidden: false,
                                index: i
                            }));
                        }
                    }
                }
            },
            aspectRatio: 2
        }
    });
}
