using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using MegaBonkPlusMod.Infrastructure.Http.Attributes;
using MegaBonkPlusMod.Infrastructure.Http.Controllers;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.Infrastructure.Http;

public class ControllerRouter
{
    private readonly Dictionary<string, (MethodInfo method, object controller)> _getRoutes = new();
    private readonly Dictionary<string, (MethodInfo method, object controller)> _postRoutes = new();

    public void RegisterControllers(params object[] controllers)
    {
        ModLogger.LogDebug("Registering API controllers...");
        foreach (var controller in controllers)
        {
            RegisterController(controller);
        }
    }

    private void RegisterController(object controller)
    {
        var type = controller.GetType();
        var controllerAttr = type.GetCustomAttribute<ApiControllerAttribute>();
        var basePath = controllerAttr?.BasePath ?? "";

        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            var getAttribute = method.GetCustomAttribute<HttpGetAttribute>();
            if (getAttribute != null)
            {
                var fullPath = CombinePaths(basePath, getAttribute.Route);
                _getRoutes[fullPath] = (method, controller);
                ModLogger.LogDebug($"  [GET] {fullPath} → {type.Name}.{method.Name}");
            }

            var postAttribute = method.GetCustomAttribute<HttpPostAttribute>();
            if (postAttribute != null)
            {
                var fullPath = CombinePaths(basePath, postAttribute.Route);
                _postRoutes[fullPath] = (method, controller);
                ModLogger.LogDebug($"  [POST] {fullPath} → {type.Name}.{method.Name}");
            }
        }
    }

    public bool HandleRequest(HttpListenerContext context)
    {
        var path = context.Request.Url.AbsolutePath;
        var method = context.Request.HttpMethod;

        try
        {
            if (method == "GET" && _getRoutes.TryGetValue(path, out var getRoute))
            {
                InvokeController(context, getRoute.method, getRoute.controller, null);
                return true;
            }

            if (method == "POST" && _postRoutes.TryGetValue(path, out var postRoute))
            {
                var payload = ReadJsonPayload(context);
                InvokeController(context, postRoute.method, postRoute.controller, payload);
                return true;
            }
        }
        catch (Exception ex)
        {
            ModLogger.LogDebug($"Error handling controller request: {ex.Message}\n{ex.StackTrace}");
            SendErrorResponse(context, ex);
            return true;
        }

        return false;
    }

    private void InvokeController(HttpListenerContext context, MethodInfo method, object controller,
        JsonElement? payload)
    {
        try
        {
            object result;

            var parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                result = method.Invoke(controller, null);
            }
            else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(JsonElement))
            {
                result = method.Invoke(controller, new object[] { payload ?? default(JsonElement) });
            }
            else
            {
                throw new InvalidOperationException($"Unsupported method signature: {method.Name}");
            }

            if (result != null && controller is ApiControllerBase controllerBase)
            {
                var responseType = result.GetType();
                var sendMethod = typeof(ApiControllerBase)
                    .GetMethod("SendResponse", BindingFlags.NonPublic | BindingFlags.Instance)
                    .MakeGenericMethod(responseType.GetGenericArguments().FirstOrDefault() ?? typeof(object));

                sendMethod.Invoke(controllerBase, new[] { context, result });
            }
        }
            catch (TargetInvocationException ex)
            {
                var innerException = ex.InnerException ?? ex;
                ModLogger.LogDebug($"Error invoking controller method: {innerException.Message}");
                SendErrorResponse(context, innerException);
            }
            catch (Exception ex)
            {
                ModLogger.LogDebug($"Error invoking controller method: {ex.Message}");
                SendErrorResponse(context, ex);
            }
        }

    private JsonElement? ReadJsonPayload(HttpListenerContext context)
    {
        try
        {
            using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
            var json = reader.ReadToEnd();
            
            if (string.IsNullOrWhiteSpace(json))
                return null;
            
            return JsonDocument.Parse(json).RootElement;
        }
        catch (Exception ex)
        {
            ModLogger.LogDebug($"Error reading JSON payload: {ex.Message}");
            return null;
        }
    }

    private void SendErrorResponse(HttpListenerContext context, Exception ex)
    {
        try
        {
            var response = ApiResponse.ServerError(ex.Message);
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.Close();
        }
        catch (Exception innerEx)
        {
            ModLogger.LogDebug($"Error sending error response: {innerEx.Message}");
            try
            {
                context.Response.StatusCode = 500;
                context.Response.Close();
            }
            catch { /* Give up */ }
        }
    }

    private string CombinePaths(string basePath, string route)
    {
        if (string.IsNullOrEmpty(basePath)) return route;
        if (string.IsNullOrEmpty(route)) return basePath;
            
        basePath = basePath.TrimEnd('/');
        route = route.TrimStart('/');
            
        return $"{basePath}/{route}";
    }
}