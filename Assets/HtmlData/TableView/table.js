(function() {
    let db = null;
    let currentTable = null;

    const loadingEl = document.getElementById('loading');
    const tableListEl = document.getElementById('tableList');
    const containerEl = document.getElementById('tableContainer');

    function showLoading(show) {
        loadingEl.style.display = show ? 'block' : 'none';
    }
    

    function displayTableList() {
        if (!db) return;
        tableListEl.innerHTML = '';
        try {
            const result = db.exec(`SELECT name, sql FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name`);
            if (result.length === 0) {
                tableListEl.innerHTML = '<p>数据库中未找到用户表格</p>';
                return;
            }
            const tables = result[0].values.map(row => ({ name: row[0], sql: row[1] }));
            tables.forEach(table => {
                const btn = document.createElement('button');
                btn.className = 'table-btn';
                btn.textContent = table.name;
                btn.title = table.sql || '';
                btn.addEventListener('click', (e) => displayTable(table.name, e));
                tableListEl.appendChild(btn);
            });
            if (tables.length > 0) {
                displayTable(tables[0].name);
                tableListEl.children[0]?.classList.add('active');
            }
        } catch (e) {
            tableListEl.innerHTML = `<p>错误: ${e.message}</p>`;
        }
    }

    function displayTable(tableName, event) {
        if (!db) return;
        if (event && event.target) {
            document.querySelectorAll('.table-btn').forEach(b => b.classList.remove('active'));
            event.target.classList.add('active');
        } else {
            document.querySelectorAll('.table-btn').forEach(b => {
                b.classList.toggle('active', b.textContent === tableName);
            });
        }
        currentTable = tableName;
        try {
            const result = db.exec(`SELECT * FROM "${tableName}"`);
            if (result.length === 0) {
                containerEl.innerHTML = '<p>表格为空</p>';
                return;
            }
            const cols = result[0].columns;
            const rows = result[0].values;
            let html = '<table><thead><tr>';
            cols.forEach(c => html += `<th>${c}</th>`);
            html += '</tr></thead><tbody>';
            rows.forEach(row => {
                html += '<tr>';
                row.forEach(cell => {
                    let val = cell === null ? '<em>NULL</em>' : String(cell).replace(/[&<>"']/g, m => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#039;'}[m]));
                    html += `<td>${val}</td>`;
                });
                html += '</tr>';
            });
            html += '</tbody></table>';
            containerEl.innerHTML = html;
        } catch (e) {
            containerEl.innerHTML = `<p>显示表格出错: ${e.message}</p>`;
        }
    }

    // ✅ 直接从父窗口读取数据库 URL，不再依赖 postMessage 握手
    function loadDatabase() {
        const url = window.parent.databaseUrl;
        if (!url) {
            containerEl.innerHTML = '<p>未获取到数据库 URL</p>';
            return;
        }
        showLoading(true);
        fetch(url)
            .then(r => { if(!r.ok) throw new Error(`HTTP ${r.status}`); return r.arrayBuffer(); })
            .then(buf => initSqlJs({ locateFile: f => `https://cdnjs.cloudflare.com/ajax/libs/sql.js/1.8.0/${f}` })
                .then(SQL => {
                    db = new SQL.Database(new Uint8Array(buf));
                    displayTableList();
                    showLoading(false);
                })
            )
            .catch(e => {
                showLoading(false);
                containerEl.innerHTML = `<p>加载数据库失败: ${e.message}</p>`;
            });
    }

    // 页面启动时直接加载
    loadDatabase();

    // 可选：向父窗口发送 ready 信号（便于调试）
    window.parent.postMessage({ type: 'ready' }, '*');
})();