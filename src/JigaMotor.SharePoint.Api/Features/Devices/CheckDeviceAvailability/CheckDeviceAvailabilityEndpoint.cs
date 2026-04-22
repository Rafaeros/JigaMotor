using FluentValidation;
using JigaMotor.Shared.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

namespace JigaMotor.SharePoint.Api.Features.Devices.CheckDeviceAvailability
{
    public static class CheckDeviceAvailabilityEndpoint
    {
        public static void MapCheckDeviceAvailability(this IEndpointRouteBuilder app)
        {
            app.MapPost("/check-availability", async (
                [FromBody] CheckDeviceAvailabilityRequest request,
                [FromServices] IValidator<CheckDeviceAvailabilityRequest> validator,
                [FromServices] CheckDeviceAvailabilityUseCase useCase) =>
            {
                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var isAvailable = await useCase.IsAvailableAsync(request);

                var result = new CheckDeviceAvailabilityResponse(
                    DevEui: request.DevEui,
                    Lora: request.Lora,
                    Serie: request.Serie,
                    IsAvailable: isAvailable
                );

                var response = ApiResponse<CheckDeviceAvailabilityResponse>.Success(result);

                return Results.Ok(response);
            })
            .WithName("CheckDeviceAvailability")
            .WithTags("Devices");
        }
    }
}
