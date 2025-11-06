using System;
using System.Net;
using System.Text;

namespace MegaBonkPlusMod.API;

public static class JsonResponse
{
    // Sendet einen JSON-String (Standard)
    public static void Send(HttpListenerContext context, string jsonString)
    {
        Send(context, jsonString, 200, "application/json");
    }

    // Sendet eine beliebige Text-Antwort
    public static void Send(HttpListenerContext context, string content, int statusCode, string contentType)
    {
        try
        {
            var buffer = Encoding.UTF8.GetBytes(content);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.ContentType = contentType;
            context.Response.StatusCode = statusCode;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        catch (Exception)
        {
            // Ignoriere Fehler, wenn der Client die Verbindung bereits geschlossen hat
        }
        finally
        {
            context.Response.OutputStream.Close();
        }
    }
}