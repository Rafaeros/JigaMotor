using JigaMotor.Shared.Responses;
using JigaMotor.SharePoint.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;


namespace JigaMotor.SharePoint.Api.Middlewares
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Exception caught by Global Handler: {Message}", exception.Message);

            var (statusCode, responseEnvelope) = exception switch
            {
                SharePointBusinessException ex =>
                    (StatusCodes.Status400BadRequest, ApiResponse<object>.Warning(ex.Message)),

                DeviceNotFoundException ex =>
                    (StatusCodes.Status404NotFound, ApiResponse<object>.Error(ex.Message, StatusCodes.Status404NotFound)),

                _ =>
                    (StatusCodes.Status500InternalServerError, ApiResponse<object>.Error("An unexpected error occurred.", StatusCodes.Status500InternalServerError))
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(responseEnvelope, cancellationToken);

            return true;
        }
    }
}
