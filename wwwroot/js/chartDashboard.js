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
    gray80Border: 'rgba(34, 48, 62, 1)',

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


function initPowerDetailChart(chartId,labels, values) {
    const ctxPowerAux = document.getElementById(chartId);
   // const colors = window.config.colors;
    new Chart(ctxPowerAux, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                data: values,
                borderWidth: 1,
                backgroundColor: [
                    chartColors.primary,
                    chartColors.success,
                    chartColors.warning,
                    chartColors.info,
                    chartColors.secondary
                ],
                borderColor: [
                    chartColors.primaryBorder,
                    chartColors.successBorder,
                    chartColors.warningBorder,
                    chartColors.infoBorder,
                    chartColors.secondaryBorder
                ],
                hoverBackgroundColor: [
                    chartColors.primaryBorder,
                    chartColors.successBorder,
                    chartColors.warningBorder,
                    chartColors.infoBorder,
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
                    display: true,
                    grid: {
                        offset: true
                    }
                }
            },
            plugins: {
                legend: {
                    display: true,
                    position: 'bottom',
                    labels: {
                        font: {
                            size: 12,

                        },
                        boxWidth: 12,
                        padding: 8,
                        
                    }
                }
            },

            aspectRatio: 1.0
        }
    });
}