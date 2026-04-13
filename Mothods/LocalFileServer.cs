using SQLiteViewer.Base;
using SQLiteViewer.Mothods;  // 引用 ResourceHelper 所在命名空间
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class LocalFileServer : IDisposable
{
    private HttpListener _listener;
    private string _filePath;
    private string _url;
    private Thread _serverThread;
    private bool _isRunning;
    private string _injectedHtml;

    // 新增字段：心跳检测
    private DateTime _lastHeartbeat;
    private Timer _idleTimer;
    private readonly object _heartbeatLock = new object();
    private const int IdleTimeoutSeconds = 10;  // 秒无心跳则停止

    private bool _isDisposed = false;

    public LocalFileServer(string filePath)
    {
        _filePath = filePath;
        var port = GetFreePort();
        _url = $"http://localhost:{port}/";
        _listener = new HttpListener();
        _listener.Prefixes.Add(_url);

        _lastHeartbeat = DateTime.UtcNow;
        _idleTimer = new Timer(CheckIdle, null, TimeSpan.FromSeconds(IdleTimeoutSeconds), TimeSpan.FromSeconds(IdleTimeoutSeconds));
    }

    private void CheckIdle(object state)
    {
        lock (_heartbeatLock)
        {
            if (!_isRunning) return;
            if ((DateTime.UtcNow - _lastHeartbeat).TotalSeconds > IdleTimeoutSeconds)
            {
                Console.WriteLine($"端口 {_url} 空闲超时，自动停止。");
                Stop();
            }
        }
    }

    public string GetFileUrl() => _url + "db";

    public void Start()
    {
        _isRunning = true;
        _serverThread = new Thread(Listen);
        _serverThread.Start();
        Console.WriteLine($"服务器已启动: {_url}");
    }

    private void Stop()
    {
        if (!_isRunning) return;
        _isRunning = false;
        _listener?.Stop();
        _listener?.Close();
        _idleTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        Console.WriteLine($"服务器已停止: {_url}");
    }

    private void Listen()
    {
        try
        {
            _listener.Start();
            while (_isRunning)
            {
                var context = _listener.GetContext();
                ProcessRequest(context);
            }
        }
        catch (HttpListenerException ex) when (ex.ErrorCode == 995)
        {
            Console.WriteLine("HTTP 服务器已停止");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"HTTP 服务器异常: {ex.Message}");
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        // 设置 CORS 头
        response.AddHeader("Access-Control-Allow-Origin", "*");
        response.AddHeader("Access-Control-Allow-Methods", "GET, OPTIONS");

        if (request.HttpMethod == "OPTIONS")
        {
            response.StatusCode = 200;
            response.Close();
            return;
        }

        string path = request.Url.AbsolutePath;

        // 心跳端点
        if (path == "/heartbeat")
        {
            lock (_heartbeatLock)
            {
                _lastHeartbeat = DateTime.UtcNow;
            }
            response.StatusCode = 204; // No Content
            response.Close();
            return;
        }

        // 数据库文件端点
        if (path == "/db")
        {
            ServeDatabaseFile(response);
            return;
        }

        // 主页面
        if (path == "/" || path == "/index.html")
        {
            string html = GetInjectedHtml();
            ServeString(response, html, "text/html");
            return;
        }

        // 静态资源（script.js / style.css）
        if (path == "/script.js")
        {
            byte[] jsBytes = ResourceHelper.ReadEmbeddedResourceAsBytes("SQLiteViewer.Assets.HtmlData.script.js");
            ServeBytes(response, jsBytes, "application/javascript");
            return;
        }

        if (path == "/style.css")
        {
            byte[] cssBytes = ResourceHelper.ReadEmbeddedResourceAsBytes("SQLiteViewer.Assets.HtmlData.style.css");
            ServeBytes(response, cssBytes, "text/css");
            return;
        }

        // 其他嵌入资源映射
        string resourceName = "SQLiteViewer.Assets.HtmlData" + path.Replace("/", ".");
        try
        {
            if (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                string content = ResourceHelper.ReadEmbeddedResource(resourceName);
                ServeString(response, content, "text/html");
            }
            else if (path.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
            {
                byte[] data = ResourceHelper.ReadEmbeddedResourceAsBytes(resourceName);
                ServeBytes(response, data, "application/javascript");
            }
            else if (path.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            {
                byte[] data = ResourceHelper.ReadEmbeddedResourceAsBytes(resourceName);
                ServeBytes(response, data, "text/css");
            }
            else
            {
                response.StatusCode = 404;
                response.Close();
            }
        }
        catch (FileNotFoundException)
        {
            response.StatusCode = 404;
            response.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"提供资源 '{path}' 失败: {ex.Message}");
            response.StatusCode = 500;
            byte[] errorBytes = Encoding.UTF8.GetBytes("Internal Server Error");
            response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
            response.Close();
        }
    }

    private void ServeDatabaseFile(HttpListenerResponse response)
    {
        try
        {
            var fileInfo = new FileInfo(_filePath);
            response.ContentLength64 = fileInfo.Length;
            response.ContentType = "application/x-sqlite3";
            response.AddHeader("Content-Disposition", "attachment; filename=database.db");

            using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fs.CopyTo(response.OutputStream);
            }
            response.StatusCode = 200;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"读取数据库文件失败: {_filePath}, 错误: {ex.Message}");
            response.StatusCode = 500;
            byte[] errorBytes = Encoding.UTF8.GetBytes("Error: " + ex.Message);
            response.OutputStream.Write(errorBytes, 0, errorBytes.Length);
        }
        response.Close();
    }

    private void ServeString(HttpListenerResponse response, string content, string contentType)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(content);
        response.ContentLength64 = buffer.Length;
        response.ContentType = contentType;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.StatusCode = 200;
        response.Close();
    }

    private void ServeBytes(HttpListenerResponse response, byte[] data, string contentType)
    {
        response.ContentLength64 = data.Length;
        response.ContentType = contentType;
        response.OutputStream.Write(data, 0, data.Length);
        response.StatusCode = 200;
        response.Close();
    }

    private static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    public void SetInjectedHtml(string html)
    {
        _injectedHtml = html;
    }

    private string GetInjectedHtml()
    {
        if (_injectedHtml == null)
        {
            return ResourceHelper.ReadEmbeddedResource("SQLiteViewer.Assets.HtmlData.index.html");
        }
        return _injectedHtml;
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            Stop();
            _idleTimer?.Dispose();
            _serverThread?.Join(1000);
        }
    }
}