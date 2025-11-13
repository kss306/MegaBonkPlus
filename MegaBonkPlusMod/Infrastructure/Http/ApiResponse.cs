using System.Text.Json.Serialization;

namespace MegaBonkPlusMod.Infrastructure.Http;

public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        [JsonPropertyName("message")]
        public string Message { get; set; }
        
        [JsonPropertyName("data")]
        public T Data { get; set; }
        
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }
        
        [JsonPropertyName("error")]
        public string Error { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = 200,
                Error = null
            };
        }

        public static ApiResponse<T> Created(T data, string message = "Resource created")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = 201,
                Error = null
            };
        }

        public static ApiResponse<T> BadRequest(string error, string message = "Bad request")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                StatusCode = 400,
                Error = error
            };
        }

        public static ApiResponse<T> NotFound(string message = "Resource not found")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                StatusCode = 404,
                Error = "NOT_FOUND"
            };
        }

        public static ApiResponse<T> ServerError(string error, string message = "Internal server error")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                StatusCode = 500,
                Error = error
            };
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Ok(string message = "Success")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                StatusCode = 200,
                Error = null
            };
        }
    }