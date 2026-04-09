/**
 * processes.js - ВИЗУАЛИЗАЦИЯ ПРОЦЕССОВ (ФИКС ВЫСОТЫ)
 * Все параметры ищутся внутри вкладки #tab-processes с префиксом proc-
 */
const processesViz = (function() {
    'use strict';
    
    // === НАСТРОЙКИ ОТОБРАЖЕНИЯ ===
    const ROW_HEIGHT = 30;           // Высота одной строки (было 15 → теперь 40)
    const BAR_THICKNESS_RATIO = 0.75; // Бар занимает 75% от высоты строки
    const MIN_VISIBLE_ROWS = 5;       // Минимум строк для комфортного вида
    const MAX_CHART_HEIGHT = 800;     // Макс. высота перед включением скролла
    const SCROLL_THRESHOLD = 15;      // После этого кол-ва процессов включаем скролл
    
    const COLORS = [
        'rgba(66, 133, 244, 0.75)',
        'rgba(52, 168, 83, 0.75)',
        'rgba(251, 188, 5, 0.75)',
        'rgba(244, 67, 54, 0.75)',
        'rgba(156, 39, 176, 0.75)',
        'rgba(0, 188, 212, 0.75)',
        'rgba(156, 39, 176, 0.75)',
        'rgba(33, 150, 243, 0.75)'
    ];
    
    let chart = null;

    // Поиск элемента ВНУТРИ вкладки #tab-processes
    function getElInTab(selector) {
        const tab = document.querySelector('#tab-processes');
        return tab ? tab.querySelector(selector) : null;
    }

    // Поиск глобального элемента
    function getEl(id) {
        return document.getElementById(id);
    }

    async function fetchData() {
        const tab = document.querySelector('#tab-processes');
        const btn = tab?.querySelector('#buildProcessesBtn');
        
        if (btn) {
            btn.disabled = true;
            btn.textContent = 'Загрузка...';
        }

        try {
            const getValue = (id) => getElInTab(id)?.value || '';
            
            const params = new URLSearchParams({
                arrivalLambda: getValue('#proc-arrivalLambda'),
                memLambda: getValue('#proc-memLambda'),
                cpuLambda: getValue('#proc-cpuLambda'),
                arrivalTime: getValue('#proc-arrivalTime'),
                testTime: getValue('#proc-testTime'),
                rngSeed: getValue('#proc-rngSeed'),
                maxMem: getValue('#proc-maxMem'),
                cpuQuant: getValue('#proc-cpuQuant')
            });

            const res = await fetch('/api/getProcesses?' + params.toString());
            if (!res.ok) throw new Error('HTTP ' + res.status);
            
            const data = await res.json();
            
            // Обновляем статистику
            const logCountEl = getElInTab('#logCount');
            const processCountEl = getElInTab('#processCount');
            const intervalCountEl = getElInTab('#intervalCount');
            
            if (logCountEl) logCountEl.textContent = data.logCount ?? '—';
            if (processCountEl) processCountEl.textContent = data.processCount ?? '—';
            if (intervalCountEl) intervalCountEl.textContent = (data.intervals?.length ?? 0).toString();

            renderTable(data.intervals);
            
            const testTime = parseFloat(getValue('#proc-testTime'));
            renderChart(data.intervals, testTime);
            
        } catch(e) {
            console.error('processes.js ошибка:', e);
            alert('Ошибка: ' + e.message);
        } finally {
            if (btn) {
                btn.disabled = false;
                btn.textContent = 'Построить процессы';
            }
        }
    }

    function renderTable(intervals) {
        const table = getEl('processesTable');
        if (!table) return;
        const tbody = table.querySelector('tbody');
        if (!tbody) return;
        
        tbody.innerHTML = '';
        
        if (!intervals || intervals.length === 0) {
            tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;color:#777">Нет данных</td></tr>';
            return;
        }

        intervals.sort(function(a, b) {
            if (a.processId !== b.processId) return a.processId - b.processId;
            return a.startTime - b.startTime;
        });
        
        intervals.forEach(function(item, idx) {
            const row = document.createElement('tr');
            row.style.background = (idx % 2 === 0) ? '#f8f9fa' : 'white';
            row.innerHTML = 
                '<td><strong>#' + item.processId + '</strong></td>' +
                '<td>' + (item.startTime ?? 0).toFixed(2) + '</td>' +
                '<td>' + (item.endTime ?? 0).toFixed(2) + '</td>' +
                '<td>' + ((item.endTime ?? 0) - (item.startTime ?? 0)).toFixed(2) + '</td>';
            tbody.appendChild(row);
        });
    }

    function renderChart(intervals, testTime) {
        // Уничтожаем старый график
        if (chart) {
            try { chart.destroy(); } catch(e) {}
            chart = null;
        }
        
        const canvas = getEl('processesChart');
        const container = canvas?.parentElement;
        if (!canvas || !container) {
            console.warn('processes.js: canvas или контейнер не найден');
            return;
        }

        // Восстанавливаем canvas в контейнере
        if (!canvas.parentElement) {
            container.innerHTML = '';
            container.appendChild(canvas);
        }

        if (!intervals || intervals.length === 0) {
            container.innerHTML = '<div style="padding:30px;text-align:center;color:#777">Нет данных</div>';
            // Сбрасываем стили контейнера
            container.style.height = '';
            container.style.maxHeight = '';
            container.style.overflowY = '';
            return;
        }

        // === Группировка интервалов по processId ===
        const processMap = {};
        for (let i = 0; i < intervals.length; i++) {
            const iv = intervals[i];
            const pid = iv.processId;
            if (!processMap[pid]) processMap[pid] = [];
            processMap[pid].push(iv);
        }

        // Сортируем ID процессов численно
        const processIds = Object.keys(processMap).map(Number).sort((a, b) => a - b);
        const numProcesses = processIds.length;

        // Метки для оси Y
        const labels = processIds.map(pid => '#' + pid);

        // Формируем данные для Chart.js
        const chartDataArray = [];
        const bgColors = [];
        const borderColors = [];

        for (let p = 0; p < processIds.length; p++) {
            const pid = processIds[p];
            const color = COLORS[p % COLORS.length];
            const processIntervals = processMap[pid];
            
            for (let j = 0; j < processIntervals.length; j++) {
                const iv = processIntervals[j];
                chartDataArray.push({
                    x: [iv.startTime ?? 0, iv.endTime ?? 0],
                    y: labels[p],
                    pid: pid,
                    start: iv.startTime ?? 0,
                    end: iv.endTime ?? 0
                });
                bgColors.push(color);
                borderColors.push(color.replace('0.75', '1'));
            }
        }

        // === РАСЧЁТ ВЫСОТЫ ГРАФИКА (ИСПРАВЛЕНО) ===
        // Берём максимум: фактическое кол-во процессов ИЛИ минимальное для красоты
        const rowsForHeight = Math.max(numProcesses, MIN_VISIBLE_ROWS);
        const calculatedHeight = rowsForHeight * ROW_HEIGHT + 80; // +80px на отступы
        
        // Ограничиваем максимальную высоту
        const chartHeight = Math.min(calculatedHeight, MAX_CHART_HEIGHT);
        
        // Настраиваем контейнер
        container.style.height = chartHeight + 'px';
        container.style.maxHeight = MAX_CHART_HEIGHT + 'px';
        
        // Включаем вертикальный скролл, если процессов много
        if (numProcesses > SCROLL_THRESHOLD) {
            container.style.overflowY = 'auto';
            container.style.overflowX = 'hidden';
        } else {
            container.style.overflowY = 'hidden';
            container.style.overflowX = 'hidden';
        }
        
        // Настраиваем canvas
        canvas.height = chartHeight;
        canvas.style.height = chartHeight + 'px';
        canvas.style.width = '100%';
        canvas.style.display = 'block';

        // === Создание графика ===
        chart = new Chart(canvas, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Процессы',
                    data: chartDataArray,
                    backgroundColor: bgColors,
                    borderColor: borderColors,
                    borderWidth: 1,
                    // Бары теперь пропорциональны высоте строки
                    barThickness: ROW_HEIGHT * BAR_THICKNESS_RATIO,
                    borderRadius: 4,
                    borderSkipped: false
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            title: function(context) {
                                const raw = context[0]?.raw;
                                return raw?.pid !== undefined ? 'Процесс #' + raw.pid : '';
                            },
                            label: function(context) {
                                const raw = context.raw;
                                if (!raw || !raw.x) return [];
                                const start = raw.x[0];
                                const end = raw.x[1];
                                return [
                                    'Начало: ' + start.toFixed(2),
                                    'Конец: ' + end.toFixed(2),
                                    'Длительность: ' + (end - start).toFixed(2)
                                ];
                            }
                        }
                    },
                    title: {
                        display: true,
                        text: 'Gantt-диаграмма (' + numProcesses + ' процессов)',
                        font: { size: 14, weight: 'bold' },
                        padding: { top: 10, bottom: 20 }
                    }
                },
                scales: {
                    x: {
                        type: 'linear',
                        min: 0,
                        max: testTime * 1.05,
                        title: { 
                            display: true, 
                            text: 'Время (тики)',
                            font: { size: 11 }
                        },
                        grid: { color: 'rgba(0,0,0,0.05)' },
                        ticks: { callback: v => Math.round(v) }
                    },
                    y: {
                        grid: { color: '#f0f0f0', offset: false },
                        ticks: {
                            font: { size: 12 }, // Чуть крупнее шрифт
                            autoSkip: false,
                            maxRotation: 0,
                            minRotation: 0,
                            padding: 8
                        }
                    }
                },
                animation: { duration: 0 },
                layout: {
                    padding: { top: 15, bottom: 15, left: 10, right: 15 }
                }
            }
        });
    }

    // === ПУБЛИЧНЫЙ МЕТОД ДЛЯ TABS.JS ===
    function resizeChart() {
        if (chart) {
            try { chart.resize(); } catch(e) {}
        }
    }

    // === ИНИЦИАЛИЗАЦИЯ ===
    function init() {
        const tab = document.querySelector('#tab-processes');
        const btn = tab?.querySelector('#buildProcessesBtn');
        
        if (btn) {
            const newBtn = btn.cloneNode(true);
            btn.parentNode.replaceChild(newBtn, btn);
            
            newBtn.addEventListener('click', function(e) {
                e.preventDefault();
                fetchData();
            });
            console.log('processes.js: кнопка подключена');
        } else {
            console.warn('processes.js: кнопка #buildProcessesBtn не найдена');
        }
    }

    // Запуск
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Экспорт
    return { 
        fetchData: fetchData,
        resizeChart: resizeChart
    };
})();

window.processesViz = processesViz;