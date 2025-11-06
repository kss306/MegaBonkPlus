using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx.Logging;
using MegaBonkPlusMod.API;

namespace MegaBonkPlusMod.Core;

public class HttpServer
{
    private readonly HttpListener _listener;
    private readonly ManualLogSource _logger;
    private readonly ApiRouter _router;
    private bool _isRunning;
    private readonly Assembly _pluginAssembly; // Um auf die Embedded Resources zuzugreifen

    public HttpServer(ManualLogSource logger, ApiRouter router)
    {
        _logger = logger;
        _router = router;
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:8080/");
        _pluginAssembly = Assembly.GetExecutingAssembly(); // Holt unsere eigene DLL
    }

    public void Start()
    {
        if (_isRunning) return;

        _isRunning = true;
        Task.Run(RunServerLoop);
        _logger.LogInfo("HttpServer gestartet auf http://localhost:8080/");
    }

    public void Stop()
    {
        _isRunning = false;
        _listener?.Stop();
        _listener?.Close();
    }

    private async void RunServerLoop()
    {
        try
        {
            _listener.Start();
            while (_isRunning)
            {
                var context = await _listener.GetContextAsync();

                // Übergib die Anfrage an den Router
                // Wenn der Router 'false' zurückgibt, war es keine API-Anfrage
                if (!_router.HandleApiRequest(context))
                    //...also versuchen wir, eine Frontend-Datei zu senden
                    HandleFrontendRequest(context);
            }
        }
        catch (Exception ex)
        {
            if (_isRunning) // Zeige Fehler nur an, wenn der Server laufen sollte
                _logger.LogError($"HttpServer-Loop-Fehler: {ex.Message}");
        }
    }

    // Diese Methode liest die HTML/JS/CSS-Dateien aus der DLL
    private void HandleFrontendRequest(HttpListenerContext context)
    {
        var urlPath = context.Request.Url.AbsolutePath;

        // Standard-Route auf index.html umleiten
        if (urlPath == "/") urlPath = "/index.html";

        // Konvertiert den Web-Pfad (z.B. /js/app.js) in den Ressourcennamen
        // (z.B. MeinSpielWebMod.Frontend.js.app.js)
        var resourcePath = "MegaBonkPlusMod.Frontend" + urlPath.Replace('/', '.');

        try
        {
            // Versuche, die Ressource aus der DLL zu laden
            using (var resourceStream = _pluginAssembly.GetManifestResourceStream(resourcePath))
            {
                if (resourceStream == null)
                {
                    _logger.LogWarning($"Frontend-Ressource nicht gefunden: {resourcePath}");
                    JsonResponse.Send(context, "404 Not Found", 404, "text/plain");
                    return;
                }

                // Sende die Datei
                context.Response.ContentType = GetMimeType(urlPath);
                resourceStream.CopyTo(context.Response.OutputStream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fehler beim Senden der Frontend-Datei: {ex.Message}");
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
        return "text/html"; // Standard
    }
}