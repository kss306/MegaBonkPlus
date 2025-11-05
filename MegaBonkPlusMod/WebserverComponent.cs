using BepInEx.Logging;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Actors.Player;
using UnityEngine;
using System.Globalization;

namespace MegaBonkPlusMod
{
    public class WebserverComponent : MonoBehaviour
    {
        private static volatile string lastKnownPositionJson = "{\"x\":0,\"y\":0,\"z\":0}";

        private Transform playerTransform;
        private HttpListener listener;
        private bool isServerRunning = false;
        private ManualLogSource Logger;

        private void Awake()
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource("WebserverComponent");
            
            Logger.LogInfo("Webserver-Komponente ist erwacht!");
            Logger.LogInfo("Starte Webserver in einem neuen Task...");
            
            // Starte den Webserver im Hintergrund
            Task.Run(() => StartWebServer());
        }

        private void Update()
        {
            try
            {
                if (playerTransform == null)
                {
                    var playerObject = FindObjectOfType<MyPlayer>(); 
            
                    if (playerObject != null)
                    {
                        playerTransform = playerObject.transform;
                        Logger.LogInfo("Spieler-Transform (MyPlayer) erfolgreich gefunden/neu gefunden!");
                    }
                }
        
                // 2. Wenn wir den Spieler haben, lies seine Position
                if (playerTransform != null)
                {
                    Vector3 position = playerTransform.position; 
                    lastKnownPositionJson = string.Format(
                        CultureInfo.InvariantCulture, 
                        "{{\"x\":{0:F2},\"y\":{1:F2},\"z\":{2:F2}}}", 
                        position.x, 
                        position.y, 
                        position.z
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Fehler in Update(): {ex.Message}. Wahrscheinlich Szenenwechsel. Setze Spieler-Transform zurück.");
                playerTransform = null; 
            }
        }

        private void OnDestroy()
        {
            Logger.LogInfo("Stoppe Webserver...");
            isServerRunning = false;
            if (listener != null && listener.IsListening)
            {
                listener.Stop();
                listener.Close();
            }
        }
        
        private void StartWebServer()
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:8080/");
                listener.Start();
                isServerRunning = true;
                Logger.LogInfo("Webserver lauscht jetzt auf http://localhost:8080/");

                while (isServerRunning)
                {
                    HttpListenerContext context = listener.GetContext();
                    HandleRequest(context);
                }
            }
            catch (Exception ex)
            {
                if (isServerRunning)
                    Logger.LogError($"Webserver-Fehler: {ex.Message}");
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string responseString = "";
            string contentType = "text/plain";

            try
            {
                switch (request.Url.AbsolutePath)
                {
                    case "/":
                        responseString = frontendHtml;
                        contentType = "text/html; charset=utf-8";
                        break;
                    case "/api/position":
                        responseString = lastKnownPositionJson;
                        contentType = "application/json";
                        break;
                    default:
                        responseString = "404 - Nicht gefunden";
                        response.StatusCode = 404;
                        break;
                }

                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.ContentType = contentType;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Fehler bei der Anfragebehandlung: {ex.Message}");
            }
            finally
            {
                response.OutputStream.Close();
            }
        }

        private const string frontendHtml = @"
            <!DOCTYPE html>
            <html lang=""de"">
            <head>
                <meta charset=""UTF-8"">
                <title>Mod-Dashboard</title>
                <style>
                    body { font-family: Arial, sans-serif; background: #222; color: #eee; }
                    #app { padding: 20px; }
                    h1 { color: #00bcd4; }
                    pre { background: #333; padding: 10px; border-radius: 5px; font-size: 1.2em; }
                </style>
            </head>
            <body>
                <div id=""app"">
                    <h1>Live Spieler-Position</h1>
                    <pre id=""position-data"">Suche Spieler im Spiel...</pre>
                </div>
                
                <script>
                    async function fetchPosition() {
                        try {
                            const response = await fetch('/api/position');
                            if (!response.ok) throw new Error('Netzwerk-Antwort nicht ok');
                            const data = await response.json();
                            
                            document.getElementById('position-data').textContent = 
                                `X: ${data.x}\nY: ${data.y}\nZ: ${data.z}`;
                        } catch (error) {
                            document.getElementById('position-data').textContent = 'Fehler beim Laden der Daten.';
                            console.error('Fehler:', error);
                        }
                    }
                    setInterval(fetchPosition, 500);
                    fetchPosition();
                </script>
            </body>
            </html>
        ";
    }
}