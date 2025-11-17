using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SQLiteViewer.Mothods
{
    public class ConnetHtml
    {
        

        // 公共方法，外界通过这个方法传递路径并打开网页
        private static void CopyFile(string sourcePath, string destinationPath)
        {
            try
            {
                // 确保目标目录存在
                string directory = Path.GetDirectoryName(destinationPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.Copy(sourcePath, destinationPath, true); // true 表示覆盖已存在的文件
                Console.WriteLine("文件复制成功！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"复制文件时出错: {ex.Message}");
            }
        }

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
            string tempHtmlFile = Path.Combine(tempDir, "TempHtmlData\\DatabaseViewer.html");
            string tempScriptFile = Path.Combine(tempDir, "TempHtmlData\\script.js");
            string tempStyleFile = Path.Combine(tempDir, "TempHtmlData\\style.css");

            string sourceScriptPath = System.IO.Path.Combine(Environment.CurrentDirectory, "Assets\\HtmlData\\script.js");
            string sourceStylePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Assets\\HtmlData\\style.css");

            //CSS、js
            CopyFile(sourceScriptPath, tempScriptFile);
            CopyFile(sourceStylePath, tempStyleFile);


            // 读取原始HTML模板
            string htmlContent = GetHtmlContentWithData(filePath);

            // 使用同步方法写入文件
            File.WriteAllText(tempHtmlFile, htmlContent, Encoding.UTF8);

            return tempHtmlFile;
        }

        private string GetHtmlContentWithData(string filePath)
        {
            // 如果有现有的HTML文件，读取并替换数据
            string sourceHtmlPath = System.IO.Path.Combine(Environment.CurrentDirectory, "Assets\\HtmlData\\index.html");
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
            try
            {
                // 读取数据库文件内容并转换为 Base64
                byte[] fileBytes = File.ReadAllBytes(filePath);
                string base64Data = Convert.ToBase64String(fileBytes);

                // 转义路径中的特殊字符
                string escapedPath = filePath.Replace("\\", "\\\\").Replace("'", "\\'");

                // 注入脚本 - 同时传递文件路径和文件内容
                string injectionScript = $@"
        <script>
            // 自动设置数据
            window.fileData = '{escapedPath}';
            window.databaseBase64 = '{base64Data}';
            
            // 更新页面显示
            document.addEventListener('DOMContentLoaded', function() {{
                var pathElement = document.getElementById('filePath');
                if (pathElement) {{
                    pathElement.innerText = '{escapedPath}';
                }}
                
                // 自动加载数据库
                if (typeof loadDatabaseFromBase64 === 'function') {{
                    loadDatabaseFromBase64('{base64Data}');
                }}
            }});
        </script>
    ";

                // 在</body>前插入脚本
                return htmlContent.Replace("</body>", injectionScript + "</body>");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"读取文件失败: {ex.Message}");
                return htmlContent;
            }
        }

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
