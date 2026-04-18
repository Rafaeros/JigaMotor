using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using JigaMotor.Shared.Responses; // Não se esqueça deste using!

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

                var result = await useCase.ExecuteAsync(request);

                var apiResponse = ApiResponse<SendEmergencyOffResponse>.Success(
                    data: result,
                    message: "Comando de DESATIVAÇÃO de emergência enviado à Everynet com sucesso."
                );

                return Results.Ok(apiResponse);
            })
            .WithName("SendEmergencyOff")
            .WithTags("Devices");
        }
    }
}