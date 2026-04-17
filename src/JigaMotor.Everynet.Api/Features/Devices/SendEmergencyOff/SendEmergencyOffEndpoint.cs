using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace JigaMotor.Everynet.Api.Features.Devices.SendEmergencyOff
{
    public static class SendEmergencyOffEndpoint
    {
        public static void MapSendEmergencyOffEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/emergency-off", async (
                [FromBody] SendEmergencyOffRequest request,
                [FromServices] SendEmergencyOffUseCase useCase,
                [FromServices] IValidator<SendEmergencyOffRequest> validator) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var response = await useCase.ExecuteAsync(request);
                return Results.Ok(new { Message = "Command sent", Response = response });
            })
            .WithName("SendEmergencyOff")
            .WithTags("Devices");
        }
    }
}
