using JigaMotor.Shared.Responses;
using JigaMotor.SharePoint.Api.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace JigaMotor.SharePoint.Api.Features.Firmwares.GetFirmwaresByModel;

public static class GetFirmwaresByModelEndpoint
{
    public static void MapGetFirmwaresByModel(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{model}", async ([FromRoute] string model, GetFirmwaresByModelUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync(model);
            var response = ApiResponse<List<Firmware>>.Success(result);
            return Results.Ok(response);
        })
        .WithName("GetFirmwaresByModel")
        .WithTags("Firmwares");
    }
}
