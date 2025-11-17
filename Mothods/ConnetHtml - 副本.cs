using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SQLiteViewer.Mothod
{
    public class ConnetHtml
    {
        // 公共方法，外界通过这个方法传递路径并打开网页
        public void OpenHtmlWithData(string filePath)
        {
            // 1. 创建或复制HTML文件到临时目录
            string tempHtmlPath = CreateHtmlFileWithData(filePath);

            // 2. 用默认浏览器打开
            OpenInSystemBrowser(tempHtmlPath);
        }

        private string CreateHtmlFileWithData(string filePath)
        {
            // 获取临时目录
            string tempDir = Path.GetTempPath();
            string tempHtmlFile = Path.Combine(tempDir, "DatabaseViewer.html");

            // 读取原始HTML模板
            string htmlContent = GetHtmlContentWithData(filePath);

            // 使用同步方法写入文件
            File.WriteAllText(tempHtmlFile, htmlContent, Encoding.UTF8);

            return tempHtmlFile;
        }

        private string GetHtmlContentWithData(string filePath)
        {
            // 如果有现有的HTML文件，读取并替换数据
            string sourceHtmlPath = System.IO.Path.Combine(Environment.CurrentDirectory, "HtmlData\\index.html");
            if (!File.Exists(sourceHtmlPath))
            {
                MessageBox.Show("index.html文件没找到");
                return "";
            }
            string htmlContent = File.ReadAllText(sourceHtmlPath);
            return InjectDataIntoHtml(htmlContent, filePath);
        }

        private string InjectDataIntoHtml(string htmlContent, string filePath)
        {
            // 转义路径中的特殊字符
            string escapedPath = filePath.Replace("\\", "\\\\").Replace("'", "\\'");

            // 简化注入脚本
            string injectionScript = $@"
        <script>
            // 设置全局变量
            window.fileData = '{escapedPath}';
            
            // 如果函数已加载，立即调用
            if (typeof receiveDataFromWpf === 'function') {{
                receiveDataFromWpf('{escapedPath}');
            }}
        </script>
    ";

            // 在</body>前插入脚本
            return htmlContent.Replace("</body>", injectionScript + "</body>");
        }

        //private string InjectDataIntoHtml(string htmlContent, string filePath)
        //{
        //    // 转义路径中的特殊字符
        //    string escapedPath = filePath.Replace("\\", "\\\\").Replace("'", "\\'");

        //    // 方法1：替换占位符
        //    htmlContent = htmlContent.Replace("'{DATA_PLACEHOLDER}'", $"'{escapedPath}'");

        //    // 方法2：直接注入脚本
        //    string injectionScript = $@"
        //    <script>
        //        // 自动设置数据
        //        window.fileData = '{escapedPath}';
        //        if (typeof receiveDataFromWpf === 'function') {{
        //            receiveDataFromWpf('{escapedPath}');
        //        }}
        //        // 更新页面显示
        //        document.addEventListener('DOMContentLoaded', function() {{
        //            var pathElement = document.getElementById('filePath') || document.getElementById('receivedData');
        //            if (pathElement) {{
        //                pathElement.innerText = '{escapedPath}';
        //            }}
        //        }});
        //    </script>
        //";

        //    // 在</body>前插入脚本
        //    return htmlContent.Replace("</body>", injectionScript + "</body>");
        //}

        private void OpenInSystemBrowser(string filePath)
        {
            try
            {
                // 使用默认浏览器打开
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });

                Console.WriteLine($"已在浏览器中打开: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"打开浏览器失败: {ex.Message}");
                // 备选方案
                TryAlternativeBrowserOpen(filePath);
            }
        }

        private void TryAlternativeBrowserOpen(string filePath)
        {
            try
            {
                // 尝试使用chrome打开
                Process.Start("chrome.exe", filePath);
            }
            catch
            {
                try
                {
                    // 尝试使用edge打开
                    Process.Start("msedge.exe", filePath);
                }
                catch
                {
                    // 最后尝试使用explorer
                    Process.Start("explorer.exe", filePath);
                }
            }
        }
    }
}
