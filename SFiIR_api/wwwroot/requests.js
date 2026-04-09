/**
 * requests.js — минимальная защищённая версия
 */
(() => {
    let chart = null;
    const ROW_HEIGHT = 24;

    // ВСЕ обращения к DOM — только внутри функций, никогда при загрузке модуля!
    const getEl = {
        btn: () => document.getElementById('buildRequestsBtn'),
        canvas: () => document.getElementById('requestsChart'),
        container: () => document.querySelector('#tab-requests .chart-wrapper'),
        tbody: () => document.querySelector('#requestsTable tbody'),
        countInfo: () => document.getElementById('requestsCountInfo'),
        input: (id) => document.getElementById(id)
    };

    function setLoading(isLoading) {
        const btn = getEl.btn();
        if (!btn) return; // Просто выходим, если кнопки нет
        btn.disabled = isLoading;
        btn.textContent = isLoading ? 'Загрузка...' : 'Построить заявки';
    }

    async function fetchData() {
        // Ищем инпуты ВНУТРИ вкладки #tab-requests
        const tab = document.querySelector('#tab-requests');
        if (!tab) {
            console.error('Не найдена вкладка #tab-requests');
            return;
        }

        setLoading(true);

        try {
            const params = new URLSearchParams({
                arrivalLambda: tab.querySelector('#req-arrivalLambda').value,
                memLambda: tab.querySelector('#req-memLambda').value,
                cpuLambda: tab.querySelector('#req-cpuLambda').value,
                testTime: tab.querySelector('#req-testTime').value,
                rngSeed: tab.querySelector('#req-rngSeed').value
            });

            const res = await fetch(`/api/getRequests?${params}`);
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            
            const data = await res.json();
            renderTable(data);
            renderChart(data);
        } catch (e) {
            console.error('Ошибка:', e);
            alert(e.message);
            clear();
        } finally {
            setLoading(false);
        }
    }

    function renderTable(data) {
        const tbody = getEl.tbody();
        if (!tbody) return;
        tbody.innerHTML = '';

        if (!data?.length) {
            tbody.innerHTML = '<tr><td colspan="4" style="text-align:center;color:#777">Нет данных</td></tr>';
            const info = getEl.countInfo();
            if (info) info.textContent = '';
            return;
        }

        const info = getEl.countInfo();
        if (info) info.textContent = `Всего заявок: ${data.length}`;

        const frag = document.createDocumentFragment();
        data.forEach(r => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td><strong>${r.id ?? '—'}</strong></td>
                <td>${(r.arrivalTime ?? 0).toFixed(2)}</td>
                <td>${(r.memUsage ?? 0).toFixed(2)}</td>
                <td>${(r.cpuTime ?? 0).toFixed(2)}</td>
            `;
            frag.appendChild(row);
        });
        tbody.appendChild(frag);
    }

    function renderChart(data) {
        clearChart();
        const canvas = getEl.canvas();
        const container = getEl.container();
        if (!canvas || !container) {
            console.warn('Canvas не найден');
            return;
        }

        if (!data?.length) {
            container.innerHTML = '<div style="padding:20px;text-align:center;color:#777">Нет данных</div>';
            setTimeout(() => {
                container.innerHTML = '<canvas id="requestsChart"></canvas>';
            }, 0);
            return;
        }

        const height = Math.min(data.length * ROW_HEIGHT + 60, 600);
        canvas.height = height;
        canvas.style.height = height + 'px';

        const chartData = {
            labels: data.map(r => `#${r.id}`),
            datasets: [{
                label: 'CPU',
                data: data.map((r, i) => ({
                    x: [r.arrivalTime ?? 0, (r.arrivalTime ?? 0) + (r.cpuTime ?? 0)],
                    y: i
                })),
                backgroundColor: 'rgba(66, 133, 244, 0.75)',
                borderColor: 'rgba(66, 133, 244, 1)',
                borderWidth: 1,
                barThickness: ROW_HEIGHT * 0.5,
                borderRadius: 2
            }]
        };

        chart = new Chart(canvas, {
            type: 'bar',
            data: chartData,
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            title: ctx => `Заявка #${data[ctx[0]?.dataIndex]?.id ?? '?'}`,
                            label: ctx => {
                                const r = ctx.raw;
                                const dur = ((r.x?.[1] ?? 0) - (r.x?.[0] ?? 0)).toFixed(2);
                                return [`Старт: ${(r.x?.[0] ?? 0).toFixed(2)}`, `Длительность: ${dur}`];
                            }
                        }
                    },
                    title: {
                        display: true,
                        text: `Временная шкала (${data.length} заявок)`,
                        font: { size: 13, weight: 'bold' }
                    }
                },
                scales: {
                    x: {
                        type: 'linear',
                        title: { display: true, text: 'Время' },
                        grid: { color: 'rgba(0,0,0,0.04)' },
                        ticks: { callback: v => Math.round(v) }
                    },
                    y: {
                        grid: { color: '#f5f5f5' },
                        reverse: true,
                        ticks: {
                            callback: v => `#${data[v]?.id ?? ''}`,
                            autoSkip: false,
                            font: { size: 10 }
                        }
                    }
                },
                animation: { duration: 0 }
            }
        });
    }

    function clearChart() {
        if (chart) {
            try { chart.destroy(); } catch(e) {}
            chart = null;
        }
        const c = getEl.canvas();
        if (c) { c.style.height = ''; c.style.width = ''; }
    }

    function clear() {
        const tb = getEl.tbody();
        if (tb) tb.innerHTML = '';
        const ci = getEl.countInfo();
        if (ci) ci.textContent = '';
        clearChart();
    }

    // === ИНИЦИАЛИЗАЦИЯ ===
    function init() {
        const btn = getEl.btn();
        if (btn) {
            // Удаляем старые обработчики через clone
            const newBtn = btn.cloneNode(true);
            btn.parentNode.replaceChild(newBtn, btn);
            newBtn.addEventListener('click', (e) => {
                e.preventDefault();
                fetchData();
            });
            console.log('requests.js: кнопка подключена');
        } else {
            console.warn('requests.js: кнопка #buildRequestsBtn не найдена');
        }
    }

    // Ждём DOM, затем инициализируемся
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Экспорт для отладки
    window.requestsViz = { fetchData, clearChart };

    function init() {
    const tab = document.querySelector('#tab-requests');
    const btn = tab?.querySelector('#buildRequestsBtn');
    if (btn) {
        const newBtn = btn.cloneNode(true);
        btn.parentNode.replaceChild(newBtn, btn);
        newBtn.addEventListener('click', (e) => {
            e.preventDefault();
            fetchData();
        });
        console.log('requests.js: кнопка подключена');
    } else {
        console.warn('requests.js: кнопка #buildRequestsBtn не найдена');
    }
}
})();