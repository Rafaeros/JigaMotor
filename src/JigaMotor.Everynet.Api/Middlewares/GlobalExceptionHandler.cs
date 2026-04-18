using JigaMotor.Shared.Responses;
using Microsoft.AspNetCore.Diagnostics;
using JigaMotor.Everynet.Api.Exceptions;

namespace JigaMotor.Everynet.Api.Middlewares;


public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception caught by Global Handler: {Message}", exception.Message);
        var (statusCode, responseEnvelope) = exception switch
        {
            EverynetIntegrationException ex =>
                (StatusCodes.Status400BadRequest, ApiResponse<object>.Warning(ex.Message)),

            _ =>
                (StatusCodes.Status500InternalServerError, ApiResponse<object>.Error("Ocorreu um erro interno inesperado.", StatusCodes.Status500InternalServerError))
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(responseEnvelope, cancellationToken);

        return true;
    }
}