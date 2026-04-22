using JigaMotor.Shared.Responses;
using JigaMotor.SharePoint.Api.Domain.Entities;

namespace JigaMotor.SharePoint.Api.Features.Devices.GetAllDevices;

public static class GetAllDevicesEndpoint
{
    public static void MapGetAllDevices(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (GetAllDevicesUseCase useCase) =>
        {
            var result = await useCase.GetAllAsync();
            var response = ApiResponse<List<DeviceProductionRecord>>.Success(result);
            return Results.Ok(response);
        })
        .WithName("GetAllDevices")
        .WithTags("Devices");
    }
}