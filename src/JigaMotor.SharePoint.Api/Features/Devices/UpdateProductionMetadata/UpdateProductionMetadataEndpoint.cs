using FluentValidation;
using JigaMotor.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace JigaMotor.SharePoint.Api.Features.Devices.UpdateProductionMetadata
{
    public static class UpdateProductionMetadataEndpoint
    {
        public static void MapUpdateProductionMetadata(this IEndpointRouteBuilder app)
        {
            app.MapPatch("/metadata", async (
                [FromBody] UpdateProductionMetadataRequest request,
                [FromServices] IValidator<UpdateProductionMetadataRequest> validator,
                [FromServices] UpdateProductionMetadataUseCase useCase) =>
            {
                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }

                try
                {
                    var updated = await useCase.ExecuteAsync(request);

                    if (!updated)
                    {
                        return Results.NotFound(ApiResponse<string>.Error($"Dispositivo com série '{request.Serie}' não encontrado.", 404));
                    }

                    return Results.Ok(ApiResponse<string>.Success(null, "Metadados de produção atualizados com sucesso."));
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(ApiResponse<string>.Error(ex.Message, 400));
                }
            })
            .WithName("UpdateProductionMetadata")
            .WithTags("Devices");
        }
    }
}
