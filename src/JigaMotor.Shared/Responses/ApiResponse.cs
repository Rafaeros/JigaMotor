
namespace JigaMotor.Shared.Responses
{
    public class ApiResponse<T>
    {
        public T? Data { get; init; }
        public string Message { get; init; }
        public string Type { get; init; }
        public int StatusCode { get; init; }
        public DateTime Timestamp { get; init; }

        private ApiResponse(T? data, string message, ResponseType type, int statusCode)
        {
            Data = data;
            Message = message;
            Type = type.ToString().ToLowerInvariant();
            StatusCode = statusCode;
            Timestamp = DateTime.UtcNow;
        }

        public static ApiResponse<T> Success(T? data, string message = "Operação realizada com sucesso!")
        {
            return new ApiResponse<T>(data, message, ResponseType.Success, 200);
        }

        public static ApiResponse<T> Created(T? data, string message = "Cadastrado com sucesso!")
        {
            return new ApiResponse<T>(data, message, ResponseType.Success, 201);
        }

        public static ApiResponse<T> Info(string message)
        {
            return new ApiResponse<T>(default, message, ResponseType.Info, 200);
        }

        public static ApiResponse<T> Warning(string message)
        {
            return new ApiResponse<T>(default, message, ResponseType.Warning, 400);
        }

        public static ApiResponse<T> Error(string message, int statusCode = 500)
        {
            return new ApiResponse<T>(default, message, ResponseType.Error, statusCode);
        }


    }
}
