(function() {
    const frame = document.getElementById('viewFrame');
    const radioTable = document.querySelector('input[value="table"]');
    const radioChart = document.querySelector('input[value="chart"]');

    // 存储键名
    const STORAGE_KEY = 'sqliteViewer_lastViewMode';

    // ========== 心跳机制 ==========
    let heartbeatInterval = null;
    const HEARTBEAT_INTERVAL_MS = 3000; // 每3秒

    function startHeartbeat() {
        if (heartbeatInterval) clearInterval(heartbeatInterval);
        console.log('[主页面] 心跳已启动');
        heartbeatInterval = setInterval(() => {
            fetch('/heartbeat')
                .then(() => console.log('[主页面] 心跳发送成功'))
                .catch(err => console.warn('[主页面] 心跳失败:', err));
        }, HEARTBEAT_INTERVAL_MS);
    }

    function stopHeartbeat() {
        if (heartbeatInterval) {
            clearInterval(heartbeatInterval);
            heartbeatInterval = null;
            console.log('[主页面] 心跳已停止');
        }
    }

    window.addEventListener('beforeunload', stopHeartbeat);
    // ===============================

    // 创建加载遮罩（复用现有 .loading 样式）
    function createLoadingOverlay() {
        const overlay = document.createElement('div');
        overlay.className = 'loading-overlay';
        overlay.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(255,255,255,0.8);
            display: flex;
            justify-content: center;
            align-items: center;
            z-index: 9999;
            backdrop-filter: blur(2px);
        `;
        const loadingDiv = document.createElement('div');
        loadingDiv.className = 'loading';
        loadingDiv.style.display = 'block';
        loadingDiv.innerHTML = `
            <div class="spinner"></div>
            <p style="margin-top: 15px; color: #4a6fa5; font-weight: 500;">正在加载视图...</p>
        `;
        overlay.appendChild(loadingDiv);
        return overlay;
    }

    function showLoading() {
        let overlay = document.querySelector('.loading-overlay');
        if (!overlay) {
            overlay = createLoadingOverlay();
            document.body.appendChild(overlay);
        }
        overlay.style.display = 'flex';
        // 强制浏览器立即重绘遮罩
        overlay.offsetHeight;
    }

    function hideLoading() {
        const overlay = document.querySelector('.loading-overlay');
        if (overlay) overlay.style.display = 'none';
    }

    // 实际执行地址切换
    function performSwitch(targetUrl) {
        if (frame.src && frame.src.includes(targetUrl)) {
            frame.src = 'about:blank';
            setTimeout(() => {
                frame.src = targetUrl;
            }, 0);
        } else {
            frame.src = targetUrl;
        }
    }

    // 切换视图（立即显示动画，并保存记忆）
    function switchView(view) {
        showLoading();
        if (view === 'table') radioTable.checked = true;
        else radioChart.checked = true;
        localStorage.setItem(STORAGE_KEY, view);
        const targetUrl = view === 'table' ? 'TableView/table.html' : 'ChartView/chart.html';
        requestAnimationFrame(() => {
            performSwitch(targetUrl);
        });
    }

    frame.addEventListener('load', hideLoading);

    radioTable.addEventListener('change', () => {
        if (radioTable.checked) switchView('table');
    });
    radioChart.addEventListener('change', () => {
        if (radioChart.checked) switchView('chart');
    });

    function restoreLastView() {
        const lastView = localStorage.getItem(STORAGE_KEY);
        if (lastView === 'chart') {
            radioChart.checked = true;
            switchView('chart');
        } else {
            radioTable.checked = true;
            switchView('table');
        }
        // 页面初始化完成后立即启动心跳
        startHeartbeat();
    }

    restoreLastView();
})();