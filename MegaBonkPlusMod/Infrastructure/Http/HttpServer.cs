using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using MegaBonkPlusMod.Core;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.Infrastructure.Http
{
    public class HttpServer
    {
        private readonly HttpListener _listener;
        private readonly ControllerRouter _controllerRouter;
        private bool _isRunning;
        private readonly Assembly _pluginAssembly;

        public HttpServer(ControllerRouter controllerRouter)
        {
            _controllerRouter = controllerRouter;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{ModConfig.WebServerPort.Value}/");
            _pluginAssembly = Assembly.GetExecutingAssembly();
        }

        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            Task.Run(RunServerLoop);
            ModLogger.LogDebug($"HttpServer start requested at http://localhost:{ModConfig.WebServerPort.Value}/");
        }

        public void Stop()
        {
            _isRunning = false;
            try
            {
                _listener?.Stop();
                _listener?.Close();
            }
            catch (Exception)
            {
                /* Ignore */
            }
        }

        private async void RunServerLoop()
        {
            try
            {
                try
                {
                    _listener.Start();
                    ModLogger.LogInfo($"HttpServer listening on http://localhost:{ModConfig.WebServerPort.Value}/");
                }
                catch (HttpListenerException ex)
                {
                    _isRunning = false;
                    ModLogger.LogError($"HttpServer failed to start: {ex.Message}");
                    return;
                }

                while (_isRunning)
                {
                    HttpListenerContext context = await _listener.GetContextAsync();
                    ModLogger.LogHttp($"[{context.Request.HttpMethod}] {context.Request.Url.AbsolutePath}");

                    try
                    {
                        if (_controllerRouter.HandleRequest(context))
                        {
                            ModLogger.LogHttp($"[HttpServer] Controller handled {context.Request.Url.AbsolutePath}");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        ModLogger.LogError($"[HttpServer] Controller crashed for {context.Request.Url.AbsolutePath}: {ex}");
                        context.Response.StatusCode = 500;
                        context.Response.Close();
                        continue;
                    }

                    if (context.Request.HttpMethod == "GET")
                    {
                        try
                        {
                            HandleEmbeddedRequest(context);
                        }
                        catch (Exception ex)
                        {
                            ModLogger.LogError($"[HttpServer] Static file handler crashed for {context.Request.Url.AbsolutePath}: {ex}");
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 404;
                        context.Response.Close();
                    }

                    ModLogger.LogHttp($"[HttpServer] Finished request {context.Request.Url.AbsolutePath}");
                }
            }
            catch (Exception ex)
            {
                if (_isRunning && ex is not ObjectDisposedException)
                    ModLogger.LogTrace($"HttpServer loop error: {ex}");
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
                        context.Response.StatusCode = 404;
                        context.Response.Close();
                        return;
                    }

                    context.Response.ContentType = GetMimeType(urlPath);
                    resourceStream.CopyTo(context.Response.OutputStream);
                }
            }
            catch (Exception ex)
            {
                ModLogger.LogTrace($"Error serving file '{resourcePath}': {ex}");
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
            finally
            {
                if (context.Response.OutputStream.CanWrite)
                {
                    context.Response.OutputStream.Close();
                }

                if (context.Response is not null && context.Response.StatusCode != 404)
                {
                    context.Response.Close();
                }

                ModLogger.LogTrace($"Response sent for {context.Request.Url.AbsolutePath}");
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