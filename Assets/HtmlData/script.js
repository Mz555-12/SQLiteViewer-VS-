// 全局变量
let db = null;
let currentTable = null;

// 初始化
document.addEventListener('DOMContentLoaded', function () {});


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

