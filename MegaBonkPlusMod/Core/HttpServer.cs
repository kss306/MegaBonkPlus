using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx.Logging;
using MegaBonkPlusMod.API;

namespace MegaBonkPlusMod.Core
{
    public class HttpServer
    {
        private readonly HttpListener _listener;
        private readonly ManualLogSource _logger;
        private readonly ApiRouter _router;
        private bool _isRunning;
        private readonly Assembly _pluginAssembly; 

        public HttpServer(ManualLogSource logger, ApiRouter router)
        {
            _logger = logger;
            _router = router;
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:8080/");
            _pluginAssembly = Assembly.GetExecutingAssembly(); 
        }

        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            Task.Run(RunServerLoop);
            _logger.LogInfo("HttpServer started at http://localhost:8080/");
        }

        public void Stop()
        {
            _isRunning = false;
            try
            {
                _listener?.Stop();
                _listener?.Close();
            }
            catch (Exception) { /* Ignore if closed */ }
        }

        private async void RunServerLoop()
        {
            try
            {
                _listener.Start();
                while (_isRunning)
                {
                    HttpListenerContext context = await _listener.GetContextAsync();
                    
                    if (context.Request.HttpMethod == "POST")
                    {
                        _router.HandleApiPostRequest(context);
                    }
                    else if (context.Request.HttpMethod == "GET")
                    {
                        if (!_router.HandleApiGetRequest(context))
                        {
                            HandleEmbeddedRequest(context);
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 200;
                        context.Response.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                if (_isRunning && ex is not ObjectDisposedException) 
                    _logger.LogError($"HttpServer-Loop-Error: {ex.Message}");
            }
        }
        
        private void HandleEmbeddedRequest(HttpListenerContext context)
        {
            var urlPath = context.Request.Url.AbsolutePath;
            
            if (urlPath == "/") urlPath = "/index.html";
            
            var resourcePath = "MegaBonkPlusMod.Frontend" + urlPath.Replace('/', '.');

            try
            {
                using (var resourceStream = _pluginAssembly.GetManifestResourceStream(resourcePath))
                {
                    if (resourceStream == null)
                    {
                        _logger.LogWarning($"Frontend-Ressource not found: {resourcePath}");
                        JsonResponse.Send(context, "404 Not Found", 404, "text/plain");
                        return;
                    }

                    context.Response.ContentType = GetMimeType(urlPath);
                    resourceStream.CopyTo(context.Response.OutputStream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending Fronted-File: {ex.Message}");
                JsonResponse.Send(context, "500 Internal Error", 500, "text/plain");
            }
            finally
            {
                context.Response.OutputStream.Close();
            }
        }

        private string GetMimeType(string path)
        {
            if (path.EndsWith(".js")) return "application/javascript";
            if (path.EndsWith(".css")) return "text/css";
            if (path.EndsWith(".html")) return "text/html";
            if (path.EndsWith(".png")) return "image/png";
            if (path.EndsWith(".jpg") || path.EndsWith(".jpeg")) return "image/jpeg";
            return "application/octet-stream";
        }
    }
}