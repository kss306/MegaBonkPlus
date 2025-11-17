using System;
using System.Net;
using System.Text;
using System.Text.Json;
using MegaBonkPlusMod.Utils;

namespace MegaBonkPlusMod.Infrastructure.Http.Controllers;

public abstract class ApiControllerBase
{
    protected void SendResponse<T>(HttpListenerContext context, ApiResponse<T> response)
    {
        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = "application/json";

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var bytes = Encoding.UTF8.GetBytes(json);
        context.Response.ContentLength64 = bytes.Length;
        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        try
        {
            ModLogger.LogTrace($"[HttpServer] Response sent for {context.Request.Url.AbsolutePath}");
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"[HttpServer] Failed to send response for {context.Request.Url.AbsolutePath}: {ex}");
            throw;
        }
        finally
        {
            context.Response.OutputStream.Close();
            context.Response.Close();
        }
    }

    protected ApiResponse<T> Ok<T>(T data, string message = "Success")
    {
        return ApiResponse<T>.Ok(data, message);
    }

    protected ApiResponse Ok(string message = "Success")
    {
        return ApiResponse.Ok(message);
    }

    protected ApiResponse<T> BadRequest<T>(string error)
    {
        return ApiResponse<T>.BadRequest(error);
    }

    protected ApiResponse BadRequest(string error)
    {
        return new ApiResponse { Success = false, Message = "Bad request", Error = error, StatusCode = 400 };
    }

    protected ApiResponse<T> NotFound<T>(string message = "Not found")
    {
        return ApiResponse<T>.NotFound(message);
    }

    protected ApiResponse<T> ServerError<T>(string error)
    {
        return ApiResponse<T>.ServerError(error);
    }

    protected ApiResponse ServerError(string error)
    {
        return new ApiResponse { Success = false, Message = "Internal server error", Error = error, StatusCode = 500 };
    }
}