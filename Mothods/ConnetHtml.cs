using SQLiteViewer.Base;
using SQLiteViewer.Mothods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace SQLiteViewer.Mothods
{
    public class ConnetHtml : IDisposable
    {
        private LocalFileServer _fileServer;

        // 静态列表，跟踪所有活跃的服务器实例
        private static readonly List<LocalFileServer> _activeServers = new List<LocalFileServer>();
        private static readonly object _serverListLock = new object();

        public void OpenHtmlWithData(string filePath)
        {
            

            var server = new LocalFileServer(filePath);

            lock (_serverListLock)
            {
                _activeServers.Add(server);
            }

            string dbUrl = server.GetFileUrl();
            string htmlContent = GetHtmlContentWithData(dbUrl);
            server.SetInjectedHtml(htmlContent);
            server.Start();

            string baseUrl = server.GetFileUrl().Replace("/db", "/");
            OpenInSystemBrowser(baseUrl);
        }

        // 静态方法：释放所有活动服务器（供主窗口关闭时调用）
        public static void DisposeAllServers()
        {
            lock (_serverListLock)
            {
                foreach (var server in _activeServers.ToArray())
                {
                    server.Dispose();
                }
                _activeServers.Clear();
            }
        }

        // 当单个 ConnetHtml 实例被释放时，将其持有的服务器从列表中移除并释放
        public void Dispose()
        {
            if (_fileServer != null)
            {
                lock (_serverListLock)
                {
                    _activeServers.Remove(_fileServer);
                }
                _fileServer.Dispose();
                _fileServer = null;
            }
        }

        // 以下方法保持不变
        private string GetHtmlContentWithData(string dbUrl)
        {
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
    }
}