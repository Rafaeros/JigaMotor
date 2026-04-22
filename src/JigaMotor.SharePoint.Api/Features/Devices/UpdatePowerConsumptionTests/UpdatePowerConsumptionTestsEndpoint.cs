using FluentValidation;
using JigaMotor.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace JigaMotor.SharePoint.Api.Features.Devices.UpdatePowerConsumptionTests
{
    public static class UpdatePowerConsumptionTestsEndpoint
    {
        public static void MapUpdatePowerConsumptionTests(this IEndpointRouteBuilder app)
        {
            app.MapPatch("/consumption", async (
                [FromBody] UpdatePowerConsumptionTestsRequest request,
                UpdatePowerConsumptionTestsUseCase useCase,
                IValidator<UpdatePowerConsumptionTestsRequest> validator) =>
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    return Results.ValidationProblem(validationResult.ToDictionary());

                var result = await useCase.ExecuteAsync(request);
                return result ? Results.Ok(ApiResponse<string>.Success(null, "Testes de consumo atualizados com sucesso.")) 
                              : Results.NotFound(ApiResponse<string>.Error("Dispositivo não encontrado.", 404));
            })
            .WithName("UpdatePowerConsumptionTests")
            .WithTags("Devices");
        }
    }
}
