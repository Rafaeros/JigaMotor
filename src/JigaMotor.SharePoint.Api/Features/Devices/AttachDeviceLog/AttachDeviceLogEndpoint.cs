using FluentValidation;
using JigaMotor.Shared.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace JigaMotor.SharePoint.Api.Features.Devices.AttachDeviceLog
{
    public static class AttachDeviceLogEndpoint
    {
        public static void MapAttachDeviceLog(this IEndpointRouteBuilder app)
        {
            app.MapPost("/attach-log", async (
                [FromBody] AttachDeviceLogRequest request,
                [FromServices] IValidator<AttachDeviceLogRequest> validator,
                [FromServices] AttachDeviceLogUseCase useCase) =>
            {
                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var success = await useCase.ExecuteAsync(request);

                if (!success)
                {
                    return Results.NotFound(ApiResponse<string>.Error($"Dispositivo '{request.Identifier}' não encontrado.", 404));
                }

                return Results.Ok(ApiResponse<string>.Success(null, "Log anexado com sucesso."));
            })
            .WithName("AttachDeviceLog")
            .WithTags("Devices");
        }
    }
}
