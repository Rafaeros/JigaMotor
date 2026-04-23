using JigaMotor.Everynet.Api.Domain.Entities;
using JigaMotor.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace JigaMotor.Everynet.Api.Features.Devices.ListDevices
{
    public static class ListDevicesEndpoint
    {
        public static void MapListDevicesEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("", async (
                [AsParameters] ListDevicesRequest request,
                [FromServices] ListDevicesUseCase useCase) =>
            {
                var (devices, total) = await useCase.ExecuteAsync(request);

                var apiResponse = ApiResponse<IEnumerable<EverynetDevice>>.Success(
                    data: devices,
                    message: $"Listagem de dispositivos recuperada com sucesso. Total: {total}"
                );

                return Results.Ok(apiResponse);
            })
            .WithName("ListDevices")
            .WithTags("Management API (Devices)");
        }
    }
}
