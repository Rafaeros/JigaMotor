using FluentValidation;
using JigaMotor.Shared.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace JigaMotor.SharePoint.Api.Features.Devices.UpdateHardwareTests
{
    public static class UpdateHardwareTestsEndpoint
    {
        public static void MapUpdateHardwareTests(this IEndpointRouteBuilder app)
        {
            app.MapPatch("/tests", async (
                [FromBody] UpdateHardwareTestsRequest request,
                [FromServices] IValidator<UpdateHardwareTestsRequest> validator,
                [FromServices] UpdateHardwareTestsUseCase useCase) =>
            {
                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                var updated = await useCase.ExecuteAsync(request);

                if (!updated)
                {
                    return Results.NotFound(ApiResponse<string>.Error($"Dispositivo '{request.Identifier}' não encontrado.", 404));
                }

                return Results.Ok(ApiResponse<string>.Success(null, "Testes de hardware atualizados com sucesso."));
            })
            .WithName("UpdateHardwareTests")
            .WithTags("Devices");
        }
    }
}
