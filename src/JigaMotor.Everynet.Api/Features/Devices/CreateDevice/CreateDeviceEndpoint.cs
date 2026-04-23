using FluentValidation;
using JigaMotor.Everynet.Api.Domain.Entities;
using JigaMotor.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace JigaMotor.Everynet.Api.Features.Devices.CreateDevice
{
    public static class CreateDeviceEndpoint
    {
        public static void MapCreateDeviceEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("", async (
                [FromBody] CreateDeviceRequest request,
                [FromServices] CreateDeviceUseCase useCase,
                [FromServices] IValidator<CreateDeviceRequest> validator) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var device = await useCase.ExecuteAsync(request);

                var apiResponse = ApiResponse<EverynetDevice>.Success(
                    data: device,
                    message: "Dispositivo criado com sucesso na Everynet."
                );

                return Results.Created($"/api/v1/devices/{device.DevEui}", apiResponse);
            })
            .WithName("CreateDevice")
            .WithTags("Management API (Devices)");
        }
    }
}
