// Dashboard JavaScript Module
var DashboardModule = (function() {
    'use strict';

    // Private variables to store chart data
    var chartData = {
        surveyStatus: {},
        regions: {},
        implementation: {},
        monthly: {},
        monthlyCompletion: {}
    };

    // Initialize function
    function init(data) {
        chartData.surveyStatus = data.surveyStatusData || {};
        chartData.regions = data.regionData || {};
        chartData.implementation = data.implementationData || {};
        chartData.monthly = data.monthlyData || {};
        chartData.monthlyCompletion = data.monthlyCompletionData || {};

        // Initialize date/time widget
        updateDateTime();
        setInterval(updateDateTime, 1000);

        // Animate numbers on load
        animateMetrics();

        // Initialize all charts
        initSurveyTrendChart();
        initStatusDonutChart();
        initImplementationTypeChart();
        initRegionChart();
    }

    // Update Date and Time
    function updateDateTime() {
        var now = new Date();
        
        // Format time (HH:MM:SS)
        var hours = String(now.getHours()).padStart(2, '0');
        var minutes = String(now.getMinutes()).padStart(2, '0');
        var seconds = String(now.getSeconds()).padStart(2, '0');
        var timeString = hours + ':' + minutes + ':' + seconds;
        
        // Format date (Month DD, YYYY)
        var months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
        var dateString = months[now.getMonth()] + ' ' + now.getDate() + ', ' + now.getFullYear();
        
        // Format day (MONDAY)
        var days = ['SUNDAY', 'MONDAY', 'TUESDAY', 'WEDNESDAY', 'THURSDAY', 'FRIDAY', 'SATURDAY'];
        var dayString = days[now.getDay()];
        
        // Update DOM
        $('#currentTime').text(timeString);
        $('#currentDate').text(dateString);
        $('#currentDay').text(dayString);
    }

    // Animate metric values
    function animateMetrics() {
        $('.metric-value').each(function() {
            var $this = $(this);
            var targetValue = parseInt($this.text());
            if (!isNaN(targetValue)) {
                $this.prop('Counter', 0).animate({
                    Counter: targetValue
                }, {
                    duration: 1000,
                    easing: 'swing',
                    step: function(now) {
                        $this.text(Math.ceil(now));
                    }
                });
            }
        });
    }

    // Survey Trend Line Chart
    function initSurveyTrendChart() {
        var months = Object.keys(chartData.monthly).length > 0 ? Object.keys(chartData.monthly) : ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'];
        var surveyValues = Object.keys(chartData.monthly).length > 0 ? Object.values(chartData.monthly) : [4, 7, 5, 9, 6, 8];
        var completionValues = Object.keys(chartData.monthlyCompletion).length > 0 ? Object.values(chartData.monthlyCompletion) : [2, 5, 3, 6, 4, 6];

        var options = {
            series: [{
                name: 'Total Surveys',
                data: surveyValues
            }, {
                name: 'Completed',
                data: completionValues
            }],
            chart: {
                height: 300,
                type: 'area',
                toolbar: {
                    show: false
                }
            },
            colors: ['#696cff', '#71dd37'],
            dataLabels: {
                enabled: false
            },
            stroke: {
                curve: 'smooth',
                width: 2
            },
            fill: {
                type: 'gradient',
                gradient: {
                    shadeIntensity: 1,
                    opacityFrom: 0.4,
                    opacityTo: 0.1
                }
            },
            xaxis: {
                categories: months
            },
            yaxis: {
                title: {
                    text: 'Number of Surveys'
                }
            },
            legend: {
                position: 'top',
                horizontalAlign: 'left'
            },
            grid: {
                borderColor: '#f1f1f1'
            }
        };

        var chart = new ApexCharts(document.querySelector("#surveyTrendChart"), options);
        chart.render();
    }

    // Status Donut Chart
    function initStatusDonutChart() {
        var statusLabels = Object.keys(chartData.surveyStatus);
        var statusValues = Object.values(chartData.surveyStatus);
        
        if (statusLabels.length === 0) {
            statusLabels = ['No Data'];
            statusValues = [1];
        }

        var colors = [];
        statusLabels.forEach(function(status) {
            switch(status) {
                case 'Completed': colors.push('#71dd37'); break;
                case 'In Progress': colors.push('#ffab00'); break;
                case 'Pending': colors.push('#8592a3'); break;
                case 'On Hold': colors.push('#ff3e1d'); break;
                default: colors.push('#696cff');
            }
        });

        var options = {
            series: statusValues,
            chart: {
                height: 310,
                type: 'donut'
            },
            labels: statusLabels,
            colors: colors,
            plotOptions: {
                pie: {
                    donut: {
                        size: '70%',
                        labels: {
                            show: true,
                            total: {
                                show: true,
                                label: 'Total',
                                formatter: function (w) {
                                    return w.globals.seriesTotals.reduce((a, b) => a + b, 0);
                                }
                            }
                        }
                    }
                }
            },
            legend: {
                show: true,
                position: 'bottom'
            },
            dataLabels: {
                enabled: true,
                formatter: function (val) {
                    return Math.round(val) + '%';
                }
            }
        };

        var chart = new ApexCharts(document.querySelector("#statusDonutChart"), options);
        chart.render();
    }

    // Implementation Type Bar Chart
    function initImplementationTypeChart() {
        var typeLabels = Object.keys(chartData.implementation);
        var typeValues = Object.values(chartData.implementation);

        if (typeLabels.length === 0) {
            typeLabels = ['No Data'];
            typeValues = [0];
        }

        var options = {
            series: [{
                name: 'Surveys',
                data: typeValues
            }],
            chart: {
                type: 'bar',
                height: 300,
                toolbar: {
                    show: false
                }
            },
            plotOptions: {
                bar: {
                    horizontal: false,
                    columnWidth: '50%',
                    borderRadius: 6,
                    dataLabels: {
                        position: 'top'
                    }
                }
            },
            dataLabels: {
                enabled: true,
                offsetY: -20,
                style: {
                    fontSize: '12px',
                    colors: ['#304758']
                }
            },
            colors: ['#696cff'],
            xaxis: {
                categories: typeLabels,
                labels: {
                    rotate: -45,
                    style: {
                        fontSize: '11px'
                    }
                }
            },
            yaxis: {
                title: {
                    text: 'Number of Surveys'
                }
            },
            grid: {
                borderColor: '#f1f1f1'
            }
        };

        var chart = new ApexCharts(document.querySelector("#implementationTypeChart"), options);
        chart.render();
    }

    // Region Chart
    function initRegionChart() {
        var regionLabels = Object.keys(chartData.regions).slice(0, 6);
        var regionValues = Object.values(chartData.regions).slice(0, 6);

        if (regionLabels.length === 0) {
            regionLabels = ['No Data'];
            regionValues = [0];
        }

        var options = {
            series: [{
                name: 'Surveys',
                data: regionValues
            }],
            chart: {
                type: 'bar',
                height: 300,
                toolbar: {
                    show: false
                }
            },
            plotOptions: {
                bar: {
                    horizontal: true,
                    borderRadius: 6,
                    dataLabels: {
                        position: 'top'
                    }
                }
            },
            dataLabels: {
                enabled: true,
                offsetX: 30,
                style: {
                    fontSize: '12px',
                    colors: ['#304758']
                }
            },
            colors: ['#03c3ec'],
            xaxis: {
                categories: regionLabels,
                title: {
                    text: 'Number of Surveys'
                }
            },
            yaxis: {
                labels: {
                    style: {
                        fontSize: '11px'
                    }
                }
            },
            grid: {
                borderColor: '#f1f1f1'
            }
        };

        var chart = new ApexCharts(document.querySelector("#regionChart"), options);
        chart.render();
    }

    // Public API
    return {
        init: init
    };
})();
