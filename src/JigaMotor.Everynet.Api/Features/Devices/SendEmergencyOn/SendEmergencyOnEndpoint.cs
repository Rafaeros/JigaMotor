using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace JigaMotor.Everynet.Api.Features.Devices.SendEmergencyOn
{
    public static class SendEmergencyOnEndpoint
    {
        public static void MapSendEmergencyOnEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/emergency-on", async (
                [FromBody] SendEmergencyOnRequest request,
                [FromServices] SendEmergencyOnUseCase useCase,
                [FromServices] IValidator<SendEmergencyOnRequest> validator) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var response = await useCase.ExecuteAsync(request);
                return Results.Ok(new { Message = "Command sent", Response = response });
            })
            .WithName("SendEmergencyOn")
            .WithTags("Devices");
        }
    }
}
