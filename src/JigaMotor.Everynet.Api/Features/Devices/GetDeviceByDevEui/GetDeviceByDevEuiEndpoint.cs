using JigaMotor.Everynet.Api.Domain.Entities;
using JigaMotor.Shared.Responses;

namespace JigaMotor.Everynet.Api.Features.Devices.GetDeviceByDevEui
{
    public static class GetDeviceByDevEuiEndpoint
    {
        public static void MapGetDeviceByDevEuiEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{devEui}", async (string devEui, GetDeviceByDevEuiUseCase useCase) =>
            {
                var device = await useCase.ExecuteAsync(devEui);
                
                var apiResponse = ApiResponse<EverynetDevice>.Success(
                    data: device!,
                    message: "Dispositivo recuperado com sucesso."
                );

                return Results.Ok(apiResponse);
            })
            .WithName("GetDeviceByDevEui")
            .WithTags("Management API (Devices)");
        }
    }
}
