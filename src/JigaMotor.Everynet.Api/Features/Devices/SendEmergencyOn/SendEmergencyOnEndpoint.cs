using FluentValidation;
using JigaMotor.Shared.Responses;
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

                var result = await useCase.ExecuteAsync(request);

                var apiResponse = ApiResponse<SendEmergencyOnResponse>.Success(
                    data: result,
                    message: "Comando de ATIVAÇÃO de emergência enviado à Everynet com sucesso."
                );

                return Results.Ok(apiResponse);
            })
            .WithName("SendEmergencyOn")
            .WithTags("Data API (Emergency)");
        }
    }
}
