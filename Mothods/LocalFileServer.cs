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

    public LocalFileServer(string filePath)
    {
        _filePath = filePath;
        var port = GetFreePort();
        _url = $"http://localhost:{port}/";
        _listener = new HttpListener();
        _listener.Prefixes.Add(_url);
    }

    public string GetFileUrl() => _url + "db";

    public void Start()
    {
        _isRunning = true;
        _serverThread = new Thread(Listen);
        _serverThread.Start();
        Console.WriteLine($"服务器已启动: {_url}");
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

        response.AddHeader("Access-Control-Allow-Origin", "*");
        response.AddHeader("Access-Control-Allow-Methods", "GET, OPTIONS");

        if (request.HttpMethod == "OPTIONS")
        {
            response.StatusCode = 200;
            response.Close();
            return;
        }

        string path = request.Url.AbsolutePath;

        if (path == "/db")
        {
            ServeDatabaseFile(response);
        }
        else if (path == "/" || path == "/index.html")
        {
            string html = GetInjectedHtml();
            ServeString(response, html, "text/html");
        }
        else if (path == "/script.js")
        {
            byte[] jsBytes = ResourceHelper.ReadEmbeddedResourceAsBytes("SQLiteViewer.Assets.HtmlData.script.js");
            ServeBytes(response, jsBytes, "application/javascript");
        }
        else if (path == "/style.css")
        {
            byte[] cssBytes = ResourceHelper.ReadEmbeddedResourceAsBytes("SQLiteViewer.Assets.HtmlData.style.css");
            ServeBytes(response, cssBytes, "text/css");
        }
        else
        {
            response.StatusCode = 404;
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
            // 后备：返回原始 HTML（不带注入）
            return ResourceHelper.ReadEmbeddedResource("SQLiteViewer.Assets.HtmlData.index.html");
        }
        return _injectedHtml;
    }

    public void Dispose()
    {
        _isRunning = false;
        _listener?.Stop();
        _listener?.Close();
        _serverThread?.Join(1000);
    }
}