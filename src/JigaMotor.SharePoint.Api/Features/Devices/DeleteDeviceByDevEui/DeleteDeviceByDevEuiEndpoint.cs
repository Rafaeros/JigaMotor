using JigaMotor.Shared.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace JigaMotor.SharePoint.Api.Features.Devices.DeleteDeviceByDevEui
{
    public static class DeleteDeviceByDevEuiEndpoint
    {
        public static void MapDeleteDeviceByDevEui(this IEndpointRouteBuilder app)
        {
            app.MapDelete("/{devEui}", async (
                string devEui,
                DeleteDeviceByDevEuiUseCase useCase) =>
            {
                var deleted = await useCase.ExecuteAsync(devEui);

                if (!deleted)
                {
                    return Results.NotFound(ApiResponse<string>.Error($"Dispositivo com DevEui '{devEui}' não encontrado.", 404));
                }

                return Results.Ok(ApiResponse<string>.Success(null, "Dispositivo removido com sucesso."));
            })
            .WithName("DeleteDeviceByDevEui")
            .WithTags("Devices");
        }
    }
}
