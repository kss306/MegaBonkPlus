using System;
using System.Net;
using System.Text;

namespace MegaBonkPlusMod.API;

public static class JsonResponse
{
    public static void Send(HttpListenerContext context, string jsonString)
    {
        Send(context, jsonString, 200, "application/json");
    }

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
            // ignore if client closed connection
        }
        finally
        {
            context.Response.OutputStream.Close();
        }
    }
}