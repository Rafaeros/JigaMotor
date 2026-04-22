using FluentValidation;
using JigaMotor.Shared.Responses;
using JigaMotor.SharePoint.Api.Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace JigaMotor.SharePoint.Api.Features.Devices.CreateInitialDevice
{
    public static class CreateInitialDeviceEndpoint
    {
        public static void MapCreateInitialDevice(this IEndpointRouteBuilder app)
        {
            app.MapPost("/", async (
                [FromBody] CreateInitialDeviceRequest request,
                [FromServices] IValidator<CreateInitialDeviceRequest> validator,
                [FromServices] CreateInitialDeviceUseCase useCase) =>
            {
                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                try
                {
                    var createdDevice = await useCase.ExecuteAsync(request);
                    var response = ApiResponse<DeviceProductionRecord>.Created(createdDevice);
                    return Results.Created($"/devices/check-exists/{createdDevice.Network?.DevEui}", response);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(ApiResponse<string>.Error(ex.Message, 400));
                }
            })
            .WithName("CreateInitialDevice")
            .WithTags("Devices");
        }
    }
}
