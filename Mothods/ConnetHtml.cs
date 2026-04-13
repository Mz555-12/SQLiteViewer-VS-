using SQLiteViewer.Base;
using SQLiteViewer.Mothods;  // 确保引用 ResourceHelper 所在命名空间
using System;
using System.Diagnostics;
using System.Windows;

namespace SQLiteViewer.Mothods
{
    public class ConnetHtml : IDisposable
    {
        private LocalFileServer _fileServer;

        public void OpenHtmlWithData(string filePath)
        {
            _fileServer = new LocalFileServer(filePath);

            // 生成带数据库 URL 的 HTML 字符串（直接从嵌入资源读取）
            string dbUrl = _fileServer.GetFileUrl();
            string htmlContent = GetHtmlContentWithData(dbUrl);

            // 将注入后的 HTML 设置到服务器
            _fileServer.SetInjectedHtml(htmlContent);

            _fileServer.Start();

            // 打开浏览器访问根路径
            string baseUrl = _fileServer.GetFileUrl().Replace("/db", "/");
            OpenInSystemBrowser(baseUrl);
        }

        private string GetHtmlContentWithData(string dbUrl)
        {
            // 从嵌入资源读取原始 index.html
            string originalHtml = ResourceHelper.ReadEmbeddedResource("SQLiteViewer.Assets.HtmlData.index.html");
            return InjectUrlIntoHtml(originalHtml, dbUrl);
        }

        private string InjectUrlIntoHtml(string htmlContent, string dbUrl)
        {
            string injectionScript = $@"
<script>
    window.databaseUrl = '{dbUrl}';
    document.addEventListener('DOMContentLoaded', function() {{
        if (typeof loadDatabaseFromUrl === 'function') {{
            loadDatabaseFromUrl('{dbUrl}');
        }}
    }});
</script>
";
            return htmlContent.Replace("</body>", injectionScript + "</body>");
        }

        private void OpenInSystemBrowser(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                Console.WriteLine($"已在浏览器中打开: {url}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"打开浏览器失败: {ex.Message}");
                TryAlternativeBrowserOpen(url);
            }
        }

        private void TryAlternativeBrowserOpen(string url)
        {
            try
            {
                Process.Start("chrome.exe", url);
            }
            catch
            {
                try
                {
                    Process.Start("msedge.exe", url);
                }
                catch
                {
                    Process.Start("explorer.exe", url);
                }
            }
        }

        public void Dispose()
        {
            _fileServer?.Dispose();
        }
    }
}