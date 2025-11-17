// 全局变量
let db = null;
let currentTable = null;

// 初始化
document.addEventListener('DOMContentLoaded', function () {
    // 文件输入事件
    // document.getElementById('dbFile').addEventListener('change', handleFileSelect);

    // 样式定制事件
    document.getElementById('fontFamily').addEventListener('change', updateStyles);
    document.getElementById('fontSize').addEventListener('change', updateStyles);
    document.getElementById('backgroundColor').addEventListener('change', updateStyles);
    document.getElementById('tableHeaderBg').addEventListener('change', updateStyles);
    document.getElementById('tableBorderColor').addEventListener('change', updateStyles);

});

// 处理文件选择
function handleFileSelect(event) {
    const file = event.target.files[0];
    if (!file) return;

    const status = document.getElementById('fileStatus');
    status.textContent = `正在加载文件: ${file.name} (${(file.size / 1024).toFixed(2)} KB)`;
    status.className = 'status';
    status.style.display = 'block';

    // 显示加载指示器
    document.getElementById('loading').style.display = 'block';

    // 检查文件大小
    if (file.size === 0) {
        status.textContent = '错误：文件为空';
        status.className = 'status error';
        document.getElementById('loading').style.display = 'none';
        return;
    }

    const reader = new FileReader();
    reader.onload = function () {
        try {
            console.log('文件读取成功，大小:', reader.result.byteLength, 'bytes');

            // 检查文件头是否为SQLite格式
            const dbData = new Uint8Array(reader.result);
            if (dbData.length < 16) {
                throw new Error('文件太小，不是有效的数据库文件');
            }

            // 检查SQLite文件头
            const header = String.fromCharCode.apply(null, dbData.slice(0, 16));
            if (header !== 'SQLite format 3\0') {
                console.warn('文件头不匹配:', header);
                // 继续尝试，有些数据库可能有不同的头
            }

            // 初始化 SQL.js
            initSqlJs({
                locateFile: file => `https://cdnjs.cloudflare.com/ajax/libs/sql.js/1.8.0/${file}`
            }).then(SQL => {
                console.log('SQL.js 加载成功');

                try {
                    // 加载数据库
                    db = new SQL.Database(dbData);
                    console.log('数据库加载成功');

                    // 显示表格列表
                    displayTableList();

                    status.textContent = `成功加载数据库: ${file.name}`;
                    status.className = 'status success';
                    document.getElementById('loading').style.display = 'none';

                } catch (dbError) {
                    console.error('数据库操作错误:', dbError);
                    status.textContent = '数据库操作失败: ' + dbError.message;
                    status.className = 'status error';
                    document.getElementById('loading').style.display = 'none';
                }

            }).catch(error => {
                console.error('SQL.js 初始化错误:', error);
                status.textContent = 'SQL.js 加载失败: ' + error.message;
                status.className = 'status error';
                document.getElementById('loading').style.display = 'none';
            });

        } catch (error) {
            console.error('文件处理错误:', error);
            status.textContent = '文件处理失败: ' + error.message;
            status.className = 'status error';
            document.getElementById('loading').style.display = 'none';
        }
    };

    reader.onerror = function () {
        console.error('文件读取错误:', reader.error);
        status.textContent = '读取文件失败: ' + reader.error;
        status.className = 'status error';
        document.getElementById('loading').style.display = 'none';
    };

    reader.readAsArrayBuffer(file);
}

// 显示表格列表
function displayTableList() {
    console.log('displayTableList被调用，db:', db);

    if (!db) {
        console.error('数据库未加载');
        return;
    }

    const tableList = document.getElementById('tableList');
    tableList.innerHTML = '';

    try {
        console.log('开始执行SQL查询获取表格列表...');
        // 获取所有表格
        const result = db.exec(`
            SELECT name, sql 
            FROM sqlite_master 
            WHERE type='table' AND name NOT LIKE 'sqlite_%'
            ORDER BY name
        `);

        console.log('获取表格列表结果:', result);

        if (result.length === 0) {
            console.log('未找到用户表格');
            tableList.innerHTML = '<p>数据库中未找到用户表格</p>';
            return;
        }

        const tables = result[0].values.map(row => ({
            name: row[0],
            sql: row[1]
        }));

        console.log('找到表格:', tables);

        tables.forEach(table => {
            console.log('创建表格按钮:', table.name);
            const button = document.createElement('button');
            button.className = 'table-btn';
            button.textContent = table.name;
            button.title = table.sql || '无创建语句';
            button.addEventListener('click', function (event) {
                console.log('点击表格按钮:', table.name);
                displayTable(table.name, event);
            });
            tableList.appendChild(button);
        });

        // 默认显示第一个表格（不传递事件对象）
        if (tables.length > 0) {
            console.log('默认显示第一个表格:', tables[0].name);
            displayTable(tables[0].name);
            // 手动设置第一个按钮为active
            if (tableList.children.length > 0) {
                tableList.children[0].classList.add('active');
            }
        }

    } catch (error) {
        console.error('获取表格列表错误:', error);
        console.error('错误堆栈:', error.stack);
        tableList.innerHTML = `<p>获取表格列表时出错: ${error.message}</p>`;
    }
}

// 显示表格数据
function displayTable(tableName, event) {
    console.log('displayTable被调用，表名:', tableName, '事件:', event);

    if (!db || !tableName) {
        console.error('数据库未加载或表名为空', { db: db, tableName: tableName });
        return;
    }

    // 更新活动按钮（如果有事件对象）
    if (event && event.target) {
        console.log('更新活动按钮:', event.target.textContent);
        const buttons = document.querySelectorAll('.table-btn');
        buttons.forEach(btn => btn.classList.remove('active'));
        event.target.classList.add('active');
    } else {
        console.log('通过表名更新活动按钮:', tableName);
        // 如果没有事件对象，通过表名找到对应的按钮
        const buttons = document.querySelectorAll('.table-btn');
        buttons.forEach(btn => {
            if (btn.textContent === tableName) {
                btn.classList.add('active');
            } else {
                btn.classList.remove('active');
            }
        });
    }

    currentTable = tableName;

    try {
        console.log('开始查询表格数据:', tableName);
        // 获取表格数据
        const result = db.exec(`SELECT * FROM ${tableName}`);
        console.log('查询结果:', result);

        if (result.length === 0) {
            console.log('表格为空:', tableName);
            document.getElementById('tableContainer').innerHTML = '<p>表格为空</p>';
            return;
        }

        const columns = result[0].columns;
        const values = result[0].values;
        console.log('表格列:', columns);
        console.log('表格数据行数:', values.length);

        // 创建表格
        let tableHTML = '<table>';

        // 表头
        tableHTML += '<thead><tr>';
        columns.forEach(col => {
            tableHTML += `<th>${col}</th>`;
        });
        tableHTML += '</tr></thead>';

        // 表体
        tableHTML += '<tbody>';
        values.forEach((row, index) => {
            tableHTML += '<tr>';
            row.forEach(cell => {
                // 处理空值和特殊字符
                let displayValue = cell;
                if (cell === null || cell === undefined) {
                    displayValue = '<em>NULL</em>';
                } else {
                    // 转义HTML特殊字符
                    displayValue = String(cell)
                        .replace(/&/g, '&amp;')
                        .replace(/</g, '&lt;')
                        .replace(/>/g, '&gt;')
                        .replace(/"/g, '&quot;')
                        .replace(/'/g, '&#039;');
                }
                tableHTML += `<td>${displayValue}</td>`;
            });
            tableHTML += '</tr>';
        });
        tableHTML += '</tbody></table>';

        console.log('将表格HTML插入到容器');
        document.getElementById('tableContainer').innerHTML = tableHTML;
        console.log('表格显示完成');

    } catch (error) {
        console.error('显示表格错误:', error);
        console.error('错误堆栈:', error.stack);
        document.getElementById('tableContainer').innerHTML = `<p>显示表格时出错: ${error.message}</p>`;
    }
}

// 更新样式
function updateStyles() {
    const root = document.documentElement;

    // 获取用户选择的样式值
    const fontFamily = document.getElementById('fontFamily').value;
    const fontSize = document.getElementById('fontSize').value;
    const backgroundColor = document.getElementById('backgroundColor').value;
    const tableHeaderBg = document.getElementById('tableHeaderBg').value;
    const tableBorderColor = document.getElementById('tableBorderColor').value;

    // 应用样式
    root.style.setProperty('--font-family', fontFamily);
    root.style.setProperty('--font-size', fontSize);
    root.style.setProperty('--background-color', backgroundColor);
    root.style.setProperty('--table-header-bg', tableHeaderBg);
    root.style.setProperty('--table-border-color', tableBorderColor);

    // 更新页面背景
    document.body.style.backgroundColor = backgroundColor;
}