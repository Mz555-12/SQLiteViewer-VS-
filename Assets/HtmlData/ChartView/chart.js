(function () {
    let db = null;
    let chartInstance = null;
    let currentTable = null;

    const tableListEl = document.getElementById('chartTableList');
    const loadingEl = document.getElementById('chartLoading');
    const canvas = document.getElementById('dataChart');
    const msgEl = document.getElementById('chartMessage');

    function showLoading(show) {
        loadingEl.style.display = show ? 'block' : 'none';
    }

    function displayTableButtons() {
        if (!db) return;
        tableListEl.innerHTML = '';
        try {
            const result = db.exec(`SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name`);
            if (result.length === 0) {
                tableListEl.innerHTML = '<p>没有表格</p>';
                return;
            }
            const tables = result[0].values.map(r => r[0]);
            tables.forEach(name => {
                const btn = document.createElement('button');
                btn.className = 'table-btn';
                btn.textContent = name;
                btn.addEventListener('click', () => {
                    currentTable = name;
                    document.querySelectorAll('.table-btn').forEach(b => b.classList.remove('active'));
                    btn.classList.add('active');
                    renderChart(name);
                });
                tableListEl.appendChild(btn);
            });
            if (tables.length > 0) {
                currentTable = tables[0];
                tableListEl.children[0]?.classList.add('active');
                renderChart(tables[0]);
            }
        } catch (e) {
            tableListEl.innerHTML = `<p>错误: ${e.message}</p>`;
        }
    }

    function renderChart(tableName) {
        if (!db) return;
        if (chartInstance) {
            chartInstance.destroy();
            chartInstance = null;
        }
        try {
            const result = db.exec(`SELECT * FROM "${tableName}"`);
            if (result.length === 0 || result[0].values.length === 0) {
                msgEl.textContent = '表格无数据';
                canvas.style.display = 'none';
                return;
            }
            const cols = result[0].columns;
            const rows = result[0].values;
            if (cols.length < 2) {
                msgEl.textContent = '至少需要两列';
                canvas.style.display = 'none';
                return;
            }
            const labels = rows.map(r => String(r[0] ?? ''));
            const data = rows.map(r => {
                const v = r[1];
                if (v === null || v === undefined) return 0;
                const n = parseFloat(v);
                return isNaN(n) ? 0 : n;
            });
            canvas.style.display = 'block';
            msgEl.textContent = '';
            const ctx = canvas.getContext('2d');
            chartInstance = new Chart(ctx, {
                type: 'line',
                data: {
                    labels,
                    datasets: [{
                        label: `${tableName} - ${cols[1]}`,
                        data,
                        borderColor: '#4a6fa5',
                        backgroundColor: 'rgba(74,111,165,0.1)',
                        tension: 0.2,
                        fill: true
                    }]
                },
                options: {
                    scales: {
                        x: {
                            title: {
                                display: true,
                                text: cols[0]   // 横坐标标题 = 第一列的列名
                            }
                        },
                        y: {
                            title: {
                                display: true,
                                text: cols[1]   // 纵坐标标题 = 第二列的列名
                            },
                            beginAtZero: false
                        }
                    }
                }
            });
        } catch (e) {
            msgEl.textContent = `渲染图表出错: ${e.message}`;
            canvas.style.display = 'none';
        }
    }

    // ✅ 直接从父窗口读取数据库 URL
    function loadDatabase() {
        const url = window.parent.databaseUrl;
        if (!url) {
            msgEl.textContent = '未获取到数据库 URL';
            return;
        }
        showLoading(true);
        fetch(url)
            .then(r => { if (!r.ok) throw new Error(`HTTP ${r.status}`); return r.arrayBuffer(); })
            .then(buf => initSqlJs({ locateFile: f => `https://cdnjs.cloudflare.com/ajax/libs/sql.js/1.8.0/${f}` })
                .then(SQL => {
                    db = new SQL.Database(new Uint8Array(buf));
                    displayTableButtons();
                    showLoading(false);
                })
            )
            .catch(e => {
                showLoading(false);
                msgEl.textContent = `加载数据库失败: ${e.message}`;
            });
    }

    loadDatabase();
    window.parent.postMessage({ type: 'ready' }, '*');
})();