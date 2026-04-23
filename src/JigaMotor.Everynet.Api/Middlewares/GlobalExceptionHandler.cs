using JigaMotor.Shared.Responses;
using Microsoft.AspNetCore.Diagnostics;
using JigaMotor.Everynet.Api.Exceptions;

namespace JigaMotor.Everynet.Api.Middlewares;


public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, responseEnvelope) = exception switch
        {
            EverynetDeviceAlreadyExistsException ex =>
                (StatusCodes.Status409Conflict, ApiResponse<object>.Warning(ex.Message)),

            EverynetIntegrationException ex =>
                (StatusCodes.Status400BadRequest, ApiResponse<object>.Warning(ex.Message)),

            _ =>
                (StatusCodes.Status500InternalServerError, ApiResponse<object>.Error("Ocorreu um erro interno inesperado.", StatusCodes.Status500InternalServerError))
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            logger.LogWarning("Business exception: {Message}", exception.Message);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(responseEnvelope, cancellationToken);

        return true;
    }
}