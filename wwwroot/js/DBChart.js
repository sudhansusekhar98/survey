/* report-charts.js
   Requires:
   - Chart.js v5+ loaded before this script
   - (optional) chartjs-plugin-datalabels loaded before this script if you want datalabels
*/

(function (window) {
    'use strict';

    // -------------------- ReportCharts module --------------------
    const ReportCharts = {
        _instances: {},

        _centerTextPlugin(centerTextOrFn, options = {}) {
            return {
                id: options.id || ('centerText-' + Math.random().toString(36).slice(2)),
                beforeDraw(chart) {
                    const ctx = chart.ctx;
                    const width = chart.width;
                    const height = chart.height;
                    const centerX = width / 2;
                    const centerY = height / 2;

                    ctx.save();
                    ctx.textAlign = 'center';
                    ctx.textBaseline = 'middle';

                    const fontSizeEm = (height / (options.sizeFactor || 114));
                    ctx.font = (fontSizeEm.toFixed(2) + 'em ' + (options.fontFamily || 'sans-serif'));
                    ctx.fillStyle = options.color || '#000';

                    let customText = '';
                    if (typeof centerTextOrFn === 'function') {
                        customText = centerTextOrFn(chart) || '';
                    } else {
                        customText = centerTextOrFn || '';
                    }

                    const offsetX = options.offsetX || 0;
                    const offsetY = options.offsetY || 0;
                    ctx.fillText(customText, centerX + offsetX, centerY + offsetY);
                    ctx.restore();
                }
            };
        },

        _sliceLabelPlugin: {
            id: 'pluginsLabeldata',
            afterDatasetsDraw(chart) {
                const ctx = chart.ctx;
                chart.data.datasets.forEach(function (dataset, i) {
                    const meta = chart.getDatasetMeta(i);
                    if (!meta.hidden) {
                        meta.data.forEach(function (element, index) {
                            ctx.fillStyle = 'rgba(0,0,0,1)';
                            const fontSize = 14;
                            ctx.font = Chart.helpers.fontString(fontSize, 'normal', 'Helvetica Neue');
                            const value = dataset.data[index];
                            const dataString = (value !== undefined && value !== null) ? String(value) + '%' : '';
                            ctx.textAlign = 'center';
                            ctx.textBaseline = 'middle';
                            const position = element.tooltipPosition();
                            ctx.fillText(dataString, position.x, position.y - (fontSize / 2) - 4);
                        });
                    }
                });
            }
        },

        _destroyIfExists(key) {
            const inst = ReportCharts._instances[key];
            if (inst) {
                try { inst.destroy(); } catch (e) { /* ignore */ }
                delete ReportCharts._instances[key];
            }
        },

        /**
         * init(opts)
         * Pass a configuration object containing only the charts you want to create.
         */
        init(opts = {}) {
            if (typeof Chart === 'undefined') {
                console.error('Chart.js is required before ReportCharts.init');
                return;
            }

            const CDL = (typeof ChartDataLabels !== 'undefined') ? ChartDataLabels : null;

            // --------- Power doughnut ----------
            if (opts.power) {
                const cfg = opts.power;
                const canvasId = cfg.id || 'powerchart';
                ReportCharts._destroyIfExists(canvasId);

                const plugins = [ReportCharts._sliceLabelPlugin];
                if (cfg.centerText !== undefined) {
                    plugins.push(ReportCharts._centerTextPlugin(cfg.centerText, { sizeFactor: 114, offsetX: cfg.offsetX || 60 }));
                }

                ReportCharts._instances[canvasId] = new Chart(document.getElementById(canvasId), {
                    type: 'doughnut',
                    data: {
                        labels: cfg.labels || [],
                        datasets: [{
                            data: cfg.data || [],
                            borderWidth: 1,
                            backgroundColor: cfg.backgroundColor || [],
                            borderColor: cfg.borderColor || [],
                            hoverBackgroundColor: cfg.hoverBackgroundColor || []
                        }]
                    },
                    options: {
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                position: 'left',
                                align: 'start',
                                labels: { padding: 10, boxWidth: 30 }
                            }
                        },
                        aspectRatio: cfg.aspectRatio || 1.6
                    },
                    plugins
                });
            }

            // --------- PowerToday (stacked + target line) ----------
            if (opts.powerToday) {
                const cfg = opts.powerToday;
                const canvasId = cfg.id || 'PowerToday';
                ReportCharts._destroyIfExists(canvasId);

                ReportCharts._instances[canvasId] = new Chart(document.getElementById(canvasId), {
                    type: 'bar',
                    data: { labels: cfg.labels || [], datasets: cfg.datasets || [] },
                    options: {
                        scales: { x: { stacked: true }, y: { stacked: true } },
                        plugins: { tooltip: { mode: 'index', intersect: false } },
                        maintainAspectRatio: false,
                        aspectRatio: cfg.aspectRatio || 3.0,
                        responsive: true
                    }
                });
            }

            // --------- BOM chart ----------
            if (opts.bom) {
                const cfg = opts.bom;
                const canvasId = cfg.id || 'bomchart';
                ReportCharts._destroyIfExists(canvasId);

                ReportCharts._instances[canvasId] = new Chart(document.getElementById(canvasId), {
                    type: 'bar',
                    data: {
                        labels: cfg.labels || [],
                        datasets: [{
                            data: cfg.data || [],
                            borderWidth: 1,
                            backgroundColor: cfg.backgroundColor || [],
                            borderColor: cfg.borderColor || [],
                            hoverBackgroundColor: cfg.hoverBackgroundColor || []
                        }]
                    },
                    options: {
                        scales: { y: { beginAtZero: true }, x: { grid: { offset: true } } },
                        plugins: { legend: { display: false } },
                        maintainAspectRatio: false,
                        aspectRatio: cfg.aspectRatio || 1.5
                    }
                });
            }

            // --------- Inventory (stacked) ----------
            if (opts.inventory) {
                const cfg = opts.inventory;
                const canvasId = cfg.id || 'inventoryposition';
                ReportCharts._destroyIfExists(canvasId);

                ReportCharts._instances[canvasId] = new Chart(document.getElementById(canvasId), {
                    type: 'bar',
                    data: { labels: cfg.labels || [], datasets: cfg.datasets || [] },
                    options: {
                        scales: { y: { beginAtZero: true }, x: { grid: { offset: true } } },
                        plugins: {
                            legend: { display: false },
                            tooltip: {
                                mode: 'index',
                                intersect: false,
                                callbacks: {
                                    footer: function (tooltipItems) {
                                        let sum = 0;
                                        tooltipItems.forEach(t => { sum += (t.parsed && t.parsed.y) ? t.parsed.y : 0; });
                                        return 'Total: ' + sum;
                                    }
                                }
                            }
                        },
                        maintainAspectRatio: false,
                        aspectRatio: cfg.aspectRatio || 1.4
                    }
                });
            }

            // --------- Downtime doughnut ----------
            if (opts.downtime) {
                const cfg = opts.downtime;
                const canvasId = cfg.id || 'downtimeschart';
                ReportCharts._destroyIfExists(canvasId);

                const plugin = ReportCharts._centerTextPlugin(cfg.centerText, { sizeFactor: cfg.sizeFactor || 100, offsetY: cfg.offsetY || 15 });

                ReportCharts._instances[canvasId] = new Chart(document.getElementById(canvasId), {
                    type: 'doughnut',
                    data: { labels: cfg.labels || [], datasets: [{ data: cfg.data || [], backgroundColor: cfg.backgroundColor, borderColor: cfg.borderColor, hoverBackgroundColor: cfg.hoverBackgroundColor }] },
                    options: {
                        maintainAspectRatio: false,
                        plugins: { legend: { position: 'top', align: 'start', labels: { padding: 10, boxWidth: 30 } } },
                        aspectRatio: cfg.aspectRatio || 1
                    },
                    plugins: [plugin]
                });
            }

            // --------- Efficiency donuts (multiple) ----------
            if (opts.efficiency) {
                const list = opts.efficiency;
                Object.keys(list).forEach(key => {
                    const cfg = list[key];
                    if (!cfg || !cfg.id) return;
                    const canvasId = cfg.id;
                    ReportCharts._destroyIfExists(canvasId);

                    const centerPlugin = ReportCharts._centerTextPlugin(() => (cfg.centerText || '') + '%', { sizeFactor: 60, offsetY: cfg.offsetY || 45, color: cfg.fontColor || '#000' });

                    ReportCharts._instances[canvasId] = new Chart(document.getElementById(canvasId), {
                        type: 'doughnut',
                        data: { labels: cfg.labels || [], datasets: [{ data: cfg.data || [], backgroundColor: cfg.backgroundColor, borderColor: cfg.borderColor, borderWidth: 1 }] },
                        options: {
                            plugins: { legend: { display: false } },
                            circumference: 180,
                            rotation: 270,
                            aspectRatio: cfg.aspectRatio || 2,
                            cutout: cfg.cutout || '60%'
                        },
                        plugins: [centerPlugin]
                    });
                });
            }

            // --------- RM Filling (line multi-series) ----------
            if (opts.rmFilling) {
                const cfg = opts.rmFilling;
                const canvasId = cfg.id || 'RMFilling';
                ReportCharts._destroyIfExists(canvasId);

                const datasets = [];
                const series = cfg.series || {};
                const colorMap = cfg.colorMap || {
                    Quartz: { border: 'rgba(255,192,203,1)', bg: 'rgba(255,192,203,0.1)' },
                    MillScale: { border: 'rgba(30,144,255,1)', bg: 'rgba(30,144,255,0.3)' },
                    Charcoal: { border: 'rgba(128,128,128,1)', bg: 'rgba(128,128,128,0.2)' },
                    LamCoke: { border: 'rgba(0,0,0,1)', bg: 'rgba(0,0,0,0.3)' },
                    WoodChip: { border: 'rgba(139,105,20,1)', bg: 'rgba(139,105,20,0.6)' }
                };

                Object.keys(series).forEach(k => {
                    const d = series[k];
                    const c = colorMap[k] || { border: 'rgba(0,0,0,1)', bg: 'rgba(0,0,0,0.1)' };
                    datasets.push({
                        label: k,
                        data: d || [],
                        borderColor: c.border,
                        borderWidth: cfg.borderWidth || 2,
                        tension: cfg.tension || 0.3,
                        fill: true,
                        backgroundColor: c.bg,
                        pointRadius: cfg.pointRadius || 2,
                        pointBorderColor: 'transparent',
                        pointBackgroundColor: 'transparent',
                        type: 'line'
                    });
                });

                ReportCharts._instances[canvasId] = new Chart(document.getElementById(canvasId), {
                    type: 'line',
                    data: { labels: cfg.labels || [], datasets },
                    options: {
                        plugins: { tooltip: { mode: 'index', intersect: false } },
                        aspectRatio: cfg.aspectRatio || 4,
                        maintainAspectRatio: cfg.maintainAspectRatio === undefined ? true : cfg.maintainAspectRatio,
                        responsive: true
                    }
                });
            }

            // --------- RM Fines ----------
            if (opts.rmFines) {
                const cfg = opts.rmFines;
                const canvasId = cfg.id || 'RMFines';
                ReportCharts._destroyIfExists(canvasId);

                ReportCharts._instances[canvasId] = new Chart(document.getElementById(canvasId), {
                    type: 'line',
                    data: { labels: cfg.labels || [], datasets: cfg.datasets || [] },
                    options: { plugins: { tooltip: { mode: 'index', intersect: false } }, aspectRatio: cfg.aspectRatio || 8, maintainAspectRatio: false, responsive: true }
                });
            }

            // --------- Production (stacked + target) ----------
            if (opts.production) {
                const cfg = opts.production;
                const canvasId = cfg.id || 'productionchart';
                ReportCharts._destroyIfExists(canvasId);

                ReportCharts._instances[canvasId] = new Chart(document.getElementById(canvasId), {
                    type: 'bar',
                    data: { labels: cfg.labels || [], datasets: cfg.datasets || [] },
                    options: {
                        scales: { x: { stacked: true }, y: { stacked: true } },
                        plugins: { tooltip: { mode: 'index', intersect: false } },
                        maintainAspectRatio: false,
                        responsive: true,
                        aspectRatio: cfg.aspectRatio || 4
                    }
                });
            }

            // --------- Production Cost charts (F1 & F2) ----------
            ['productionCostF1', 'productionCostF2'].forEach(key => {
                if (!opts[key]) return;
                const cfg = opts[key];
                const canvasId = cfg.id;
                if (!canvasId) return;
                ReportCharts._destroyIfExists(canvasId);

                const dataset = Object.assign({}, cfg.dataset || {});
                if (!dataset.datalabels) {
                    dataset.datalabels = {
                        anchor: 'end', align: 'bottom',
                        formatter: function (value, context) {
                            if (Array.isArray(cfg.productionValues)) return cfg.productionValues[context.dataIndex];
                            return value;
                        },
                        font: { weight: 'bold' }, color: '#000'
                    };
                }

                const plugins = [];
                if (CDL) plugins.push(ChartDataLabels);

                ReportCharts._instances[canvasId] = new Chart(document.getElementById(canvasId), {
                    type: 'bar',
                    data: { labels: cfg.labels || [], datasets: [dataset] },
                    options: Object.assign({
                        plugins: {
                            tooltip: {
                                mode: 'index',
                                intersect: false,
                                callbacks: {
                                    label: function (context) {
                                        const cost = context.raw;
                                        const idx = context.dataIndex;
                                        const prodQty = (Array.isArray(cfg.productionValues) ? parseFloat(cfg.productionValues[idx]) || 0 : 0);
                                        if (prodQty > 0) {
                                            return ['Per MT Cost: ' + cost.toLocaleString(), 'Total Production: ' + prodQty + ' MT'];
                                        } else {
                                            return ['Cost: ' + cost.toLocaleString(), 'Production: 0 MT'];
                                        }
                                    }
                                }
                            },
                            datalabels: { display: true }
                        },
                        aspectRatio: cfg.aspectRatio || 5,
                        maintainAspectRatio: false
                    }, cfg.options || {}),
                    plugins
                });
            });

            return ReportCharts._instances;
        } // init
    };

    // attach to window
    window.ReportCharts = ReportCharts;

    // -------------------- default sample data (client-side only) --------------------
    // Replace these arrays with your actual data or populate them dynamically
    const PowerConLabels = ['A', 'B', 'C', 'D'];
    const PowerConData = [20, 30, 25, 25];

    const PowerLabelData = ['08:00', '10:00', '12:00', '14:00'];
    const PowerTraget = [50, 50, 50, 50];
    const PowerFurnaceI = [10, 12, 14, 11];
    const PowerFurnaceII = [8, 9, 10, 9];
    const PowerGCP = [15, 12, 11, 13];
    const PowerAuxilary = [17, 17, 15, 17];

    const BOMConLabels = ['Quartz', 'Charcoal', 'MillScale', 'LamCoke', 'WoodChip', 'Other', 'Red'];
    const BOMConData = [120, 90, 60, 40, 70, 20, 30];

    const InventoryLabel = ['Site A', 'Site B', 'Site C'];
    const InventoryStock = [120, 90, 60];
    const InventoryQC = [5, 3, 2];
    const InventoryWIP = [12, 8, 5];

    const DownTimeLabels = ['Planned', 'Unplanned', 'Breakdown', 'Other'];
    const DownTimeData = [2.5, 1.0, 0.5, 0.25];
    const DownTimeCenterText = '4.25';

    const EfficiencyLabelData = ['Used', 'Idle'];
    const EfficiencyF1Data = [85, 15]; const EfficiencyF1 = 85;
    const EfficiencyF2Data = [72, 28]; const EfficiencyF2 = 72;
    const EfficiencyGCPData = [95, 5]; const EfficiencyGCP = 95;
    const EfficiencyRMHSData = [60, 40]; const EfficiencyRMHS = 60;

    const ConsLabelData = ['2025-09-01', '2025-09-02', '2025-09-03', '2025-09-04', '2025-09-05'];
    const ConsQuartzData = [22, 18, 12, 14, 16];
    const ConsMillScaleData = [10, 8, 11, 9, 10];
    const ConsCharCoalData = [5, 7, 6, 8, 4];
    const ConsLAMCData = [2, 3, 4, 3, 2];
    const ConsWoodChipData = [3, 2, 4, 3, 3];

    const ConsFinesLabelData = ['2025-09-01', '2025-09-02'];
    const ConsQuartzFinesData = [1, 2];
    const ConsCharcoalFinesData = [0.5, 0.6];
    const ConsQuartzWashingDustData = [0.2, 0.1];
    const ConsWoodChipFinesData = [0.3, 0.4];

    const ProductionLabelData = ['Day1', 'Day2', 'Day3', 'Day4'];
    const ProductionTargetData = [80, 80, 80, 80];
    const ProductionFurnaceIData = [40, 42, 38, 41];
    const ProductionFurnaceIIData = [30, 28, 32, 30];

    const CostFurnaceIData = [200000, 180000, 220000, 210000];
    const CostFurnaceIIData = [150000, 140000, 160000, 155000];
    const ProductionFurnaceIData_forLabels = ProductionFurnaceIData;
    const ProductionFurnaceIIData_forLabels = ProductionFurnaceIIData;

    const daysLeft = ['5', '3', '1', '7'];
    const disDate = '2025-09-10';

    // -------------------- build opts using the consts --------------------
    const opts = {
        power: {
            id: 'powerchart',
            labels: PowerConLabels,
            data: PowerConData,
            centerText: '75%',
            offsetX: 60,
            backgroundColor: ['rgba(255,0,0,0.6)', 'rgba(255,0,0,0.4)', 'rgba(100,239,90,0.6)', 'rgba(108,189,255,0.8)'],
            borderColor: ['rgba(255,0,0,1)', 'rgba(255,0,0,1)', 'rgba(100,239,90,1)', 'rgba(108,189,255,1)'],
            hoverBackgroundColor: ['rgba(255,0,0,1)', 'rgba(255,0,0,1)', 'rgba(100,239,90,1)', 'rgba(108,189,255,1)'],
            aspectRatio: 1.6
        },

        powerToday: {
            id: 'PowerToday',
            labels: PowerLabelData,
            datasets: [
                { type: 'line', label: 'Target', data: PowerTraget, backgroundColor: 'transparent', borderColor: 'rgba(255,0,0,1)', borderDash: [8, 5], borderWidth: 1 },
                { label: 'Furnace I', stack: 'Stack 0', data: PowerFurnaceI, borderColor: 'rgba(255,0,0,1)', backgroundColor: 'rgba(255,0,0,0.6)' },
                { label: 'Furnace II', stack: 'Stack 0', data: PowerFurnaceII, borderColor: 'rgba(255,0,0,0.6)', backgroundColor: 'rgba(255,0,0,0.4)' },
                { label: 'GCP', stack: 'Stack 0', data: PowerGCP, borderColor: 'rgba(100,239,90,1)', backgroundColor: 'rgba(60,179,113,0.4)' },
                { label: 'Auxilary', stack: 'Stack 0', data: PowerAuxilary, borderColor: 'rgba(108,189,255,1)', backgroundColor: 'rgba(108,189,255,0.8)' }
            ],
            aspectRatio: 3.0
        },

        bom: {
            id: 'bomchart',
            labels: BOMConLabels,
            data: BOMConData,
            backgroundColor: ['rgba(255,192,203,0.8)', 'rgba(0,0,0,0.4)', 'rgba(176,196,222,0.9)', 'rgba(0,0,0,0.6)', 'rgba(0,0,0,0.8)', 'rgba(139,105,20,0.6)', 'rgba(255,0,0,0.6)'],
            borderColor: ['rgba(255,192,203,1)', 'rgba(0,0,0,0.1)', 'rgba(176,196,222,1)', 'rgba(0,0,0,0.1)', 'rgba(0,0,0,0.1)', 'rgba(139,105,20,1)', 'rgba(255,0,0,0.9)'],
            hoverBackgroundColor: ['rgba(255,192,203,1)', 'rgba(0,0,0,0.1)', 'rgba(176,196,222,1)', 'rgba(0,0,0,0.1)', 'rgba(0,0,0,0.1)', 'rgba(139,105,20,1)', 'rgba(255,0,0,1)'],
            aspectRatio: 1.5
        },

        inventory: {
            id: 'inventoryposition',
            labels: InventoryLabel,
            datasets: [
                { label: 'Stock', stack: 'Stack 0', data: InventoryStock, borderColor: 'rgba(131,198,252,1)', backgroundColor: 'rgba(131,198,252,0.9)' },
                { label: 'QC', stack: 'Stack 0', data: InventoryQC, borderColor: 'rgba(176,203,243,0.9)', backgroundColor: 'rgba(176,203,243,0.7)' },
                { label: 'WIP', stack: 'Stack 0', data: InventoryWIP, borderColor: 'rgba(155,206,249,0.7)', backgroundColor: 'rgba(155,206,249,0.5)' }
            ],
            aspectRatio: 1.4
        },

        downtime: {
            id: 'downtimeschart',
            labels: DownTimeLabels,
            data: DownTimeData,
            centerText: DownTimeCenterText,
            backgroundColor: ['rgba(8,71,129,0.9)', 'rgba(17,98,170,0.7)', 'rgba(17,98,170,0.5)', 'rgba(36,156,255,0.5)'],
            borderColor: ['rgba(8,71,129,0.9)', 'rgba(17,98,170,0.7)', 'rgba(17,98,170,0.5)', 'rgba(36,156,255,0.5)'],
            hoverBackgroundColor: ['rgba(8,71,129,1)', 'rgba(17,98,170,1)', 'rgba(17,98,170,1)', 'rgba(36,156,255,1)'],
            aspectRatio: 1
        },

        efficiency: {
            f1: {
                id: 'effFurnaceI',
                labels: EfficiencyLabelData,
                data: EfficiencyF1Data,
                centerText: EfficiencyF1,
                backgroundColor: [(EfficiencyF1 >= 90 ? 'rgba(100,239,90,0.4)' : EfficiencyF1 >= 70 ? 'rgba(255,215,0,0.4)' : 'rgba(255,0,0,0.4)'), 'rgba(0,0,0,0.1)'],
                borderColor: [(EfficiencyF1 >= 90 ? 'rgba(100,239,90,1)' : EfficiencyF1 >= 70 ? 'rgba(255,215,0,1)' : 'rgba(255,0,0,1)'), 'rgba(0,0,0,0.1)'],
                fontColor: (EfficiencyF1 >= 90 ? 'rgba(0,128,0,1)' : EfficiencyF1 >= 70 ? 'rgba(255,215,0,1)' : 'rgba(255,0,0,1)'),
                aspectRatio: 2,
                cutout: '60%'
            },
            f2: {
                id: 'effFurnaceII',
                labels: EfficiencyLabelData,
                data: EfficiencyF2Data,
                centerText: EfficiencyF2,
                backgroundColor: [(EfficiencyF2 >= 90 ? 'rgba(100,239,90,0.4)' : EfficiencyF2 >= 70 ? 'rgba(255,215,0,0.4)' : 'rgba(255,0,0,0.4)'), 'rgba(0,0,0,0.1)'],
                borderColor: [(EfficiencyF2 >= 90 ? 'rgba(100,239,90,1)' : EfficiencyF2 >= 70 ? 'rgba(255,215,0,1)' : 'rgba(255,0,0,1)'), 'rgba(0,0,0,0.1)'],
                fontColor: (EfficiencyF2 >= 90 ? 'rgba(0,128,0,1)' : EfficiencyF2 >= 70 ? 'rgba(255,215,0,1)' : 'rgba(255,0,0,1)'),
                aspectRatio: 2,
                cutout: '60%'
            },
            gcp: {
                id: 'effGCP',
                labels: EfficiencyLabelData,
                data: EfficiencyGCPData,
                centerText: EfficiencyGCP,
                backgroundColor: [(EfficiencyGCP >= 90 ? 'rgba(100,239,90,0.4)' : EfficiencyGCP >= 70 ? 'rgba(255,215,0,0.4)' : 'rgba(255,0,0,0.4)'), 'rgba(0,0,0,0.1)'],
                borderColor: [(EfficiencyGCP >= 90 ? 'rgba(100,239,90,1)' : EfficiencyGCP >= 70 ? 'rgba(255,215,0,1)' : 'rgba(255,0,0,1)'), 'rgba(0,0,0,0.1)'],
                fontColor: (EfficiencyGCP >= 90 ? 'rgba(0,128,0,1)' : EfficiencyGCP >= 70 ? 'rgba(255,215,0,1)' : 'rgba(255,0,0,1)'),
                aspectRatio: 2,
                cutout: '60%'
            },
            rmhs: {
                id: 'effRMHS',
                labels: EfficiencyLabelData,
                data: EfficiencyRMHSData,
                centerText: EfficiencyRMHS,
                backgroundColor: [(EfficiencyRMHS >= 90 ? 'rgba(100,239,90,0.4)' : EfficiencyRMHS >= 70 ? 'rgba(255,215,0,0.4)' : 'rgba(255,0,0,0.4)'), 'rgba(0,0,0,0.1)'],
                borderColor: [(EfficiencyRMHS >= 90 ? 'rgba(100,239,90,1)' : EfficiencyRMHS >= 70 ? 'rgba(255,215,0,1)' : 'rgba(255,0,0,1)'), 'rgba(0,0,0,0.1)'],
                fontColor: (EfficiencyRMHS >= 90 ? 'rgba(0,128,0,1)' : EfficiencyRMHS >= 70 ? 'rgba(255,215,0,1)' : 'rgba(255,0,0,1)'),
                aspectRatio: 2,
                cutout: '60%'
            }
        },

        rmFilling: {
            id: 'RMFilling',
            labels: ConsLabelData,
            series: {
                Quartz: ConsQuartzData,
                MillScale: ConsMillScaleData,
                Charcoal: ConsCharCoalData,
                LamCoke: ConsLAMCData,
                WoodChip: ConsWoodChipData
            },
            aspectRatio: 4
        },

        rmFines: {
            id: 'RMFines',
            labels: ConsFinesLabelData,
            datasets: [
                { label: 'Quartz Fines', data: ConsQuartzFinesData, borderColor: 'rgba(255,192,203,1)', backgroundColor: 'rgba(255,192,203,0.8)' },
                { label: 'Charcoal Fines', data: ConsCharcoalFinesData, borderColor: 'rgba(128,128,128,1)', backgroundColor: 'rgba(128,128,128,0.6)' },
                { label: 'Quartz Washing Dust', data: ConsQuartzWashingDustData, borderColor: 'rgba(30,144,255,1)', backgroundColor: 'rgba(30,144,255,0.3)' },
                { label: 'Wood chips Fines', data: ConsWoodChipFinesData, borderColor: 'rgba(139,105,20,1)', backgroundColor: 'rgba(139,105,20,0.6)' }
            ],
            aspectRatio: 8
        },

        production: {
            id: 'productionchart',
            labels: ProductionLabelData,
            datasets: [
                { type: 'line', label: 'Target', data: ProductionTargetData, backgroundColor: 'transparent', borderColor: 'rgba(220,53,69,0.2)', tension: 0.3, borderWidth: 1 },
                { type: 'bar', label: 'Furnace I', data: ProductionFurnaceIData, borderColor: 'rgb(78, 113, 255)', backgroundColor: 'rgba(78, 113, 255,0.4)' },
                { type: 'bar', label: 'Furnace II', data: ProductionFurnaceIIData, borderColor: 'rgb(141, 216, 255)', backgroundColor: 'rgba(141, 216, 255,0.7)' }
            ],
            aspectRatio: 4
        },

        productionCostF1: {
            id: 'productionCostF1chart',
            labels: ProductionLabelData,
            dataset: {
                type: 'bar',
                label: 'Furnace I',
                data: CostFurnaceIData,
                backgroundColor: 'rgba(78,113,255,0.4)',
                borderColor: 'rgba(78,113,255,1)',
                hoverBackgroundColor: 'rgb(78,113,255,1)',
                borderWidth: 1
            },
            productionValues: ProductionFurnaceIData_forLabels,
            aspectRatio: 5
        },

        productionCostF2: {
            id: 'productionCostF2chart',
            labels: ProductionLabelData,
            dataset: {
                type: 'bar',
                label: 'Furnace II',
                data: CostFurnaceIIData,
                backgroundColor: 'rgba(141,216,255,0.7)',
                borderColor: 'rgba(141,216,255,1)',
                hoverBackgroundColor: 'rgba(141,216,255,1)',
                borderWidth: 1
            },
            productionValues: ProductionFurnaceIIData_forLabels,
            aspectRatio: 5
        }
    };

    // -------------------- initialize on DOMContentLoaded --------------------
    document.addEventListener('DOMContentLoaded', function () {
        ReportCharts.init(opts);
    });

})(window);
