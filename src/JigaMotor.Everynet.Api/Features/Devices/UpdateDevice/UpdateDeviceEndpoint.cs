using FluentValidation;
using JigaMotor.Everynet.Api.Domain.Entities;
using JigaMotor.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace JigaMotor.Everynet.Api.Features.Devices.UpdateDevice
{
    public static class UpdateDeviceEndpoint
    {
        public static void MapUpdateDeviceEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPatch("/{devEui}", async (
                [FromRoute] string devEui,
                [FromBody] UpdateDeviceRequest request,
                [FromServices] UpdateDeviceUseCase useCase,
                [FromServices] IValidator<UpdateDeviceRequest> validator) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var device = await useCase.ExecuteAsync(devEui, request);

                var apiResponse = ApiResponse<EverynetDevice>.Success(
                    data: device,
                    message: "Dispositivo atualizado com sucesso na Everynet."
                );

                return Results.Ok(apiResponse);
            })
            .WithName("UpdateDevice")
            .WithTags("Management API (Devices)");
        }
    }
}
