/**
 * tabs.js — менеджер вкладок (ОБНОВЛЁННАЯ ВЕРСИЯ)
 * Поддерживает перерисовку графиков через публичные методы модулей
 */
document.addEventListener('DOMContentLoaded', () => {
    const tabBtns = document.querySelectorAll('.tab-btn');
    const tabContents = document.querySelectorAll('.tab-content');

    // Функция перерисовки графика для указанной вкладки
    function resizeChartForTab(tabId) {
        // Небольшая задержка, чтобы вкладка успела стать видимой
        setTimeout(() => {
            try {
                if (tabId === 'requests' && typeof window.requestsViz?.resizeChart === 'function') {
                    window.requestsViz.resizeChart();
                }
                if (tabId === 'processes' && typeof window.processesViz?.resizeChart === 'function') {
                    window.processesViz.resizeChart();
                }
            } catch (e) {
                console.warn(`Не удалось перерисовать график для ${tabId}:`, e);
            }
        }, 50);
    }

    tabBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            const tabId = btn.dataset.tab;
            
            // Деактивируем все кнопки и вкладки
            tabBtns.forEach(b => b.classList.remove('active'));
            tabContents.forEach(c => c.classList.remove('active'));
            
            // Активируем выбранное
            btn.classList.add('active');
            const targetTab = document.getElementById(`tab-${tabId}`);
            if (targetTab) {
                targetTab.classList.add('active');
            }
            
            // Перерисовываем график активной вкладки
            resizeChartForTab(tabId);
            
            console.log(`Вкладка "${tabId}" активирована`);
        });
    });

    // Опционально: перерисовка при изменении размера окна
    let resizeTimeout;
    window.addEventListener('resize', () => {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
            const activeTab = document.querySelector('.tab-content.active');
            if (activeTab) {
                const tabId = activeTab.id.replace('tab-', '');
                resizeChartForTab(tabId);
            }
        }, 150);
    });
});