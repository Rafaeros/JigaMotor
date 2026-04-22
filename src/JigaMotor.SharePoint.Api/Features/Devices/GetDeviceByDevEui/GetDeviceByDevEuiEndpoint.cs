using JigaMotor.Shared.Responses;
using JigaMotor.SharePoint.Api.Domain.Entities;

namespace JigaMotor.SharePoint.Api.Features.Devices.GetDeviceByDevEui
{
    public static class GetDeviceByDevEuiEndpoint
    {
        public static void MapGetDeviceByDevEui(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{devEui}", async (string devEui, GetDeviceByDevEuiUseCase useCase) =>
            {
                var device = await useCase.ExecuteAsync(devEui);
                var response = ApiResponse<DeviceProductionRecord>.Success(device);
                return device != null ? Results.Ok(response) 
                                      : Results.NotFound(ApiResponse<string>.Error($"Dispositivo com DevEui '{devEui}' não encontrado.", 404));
            })
            .WithName("GetDeviceByDevEui")
            .WithTags("Devices");
        }
    }
}
