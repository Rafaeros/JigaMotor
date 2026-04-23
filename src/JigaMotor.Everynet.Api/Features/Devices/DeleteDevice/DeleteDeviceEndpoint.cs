using JigaMotor.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace JigaMotor.Everynet.Api.Features.Devices.DeleteDevice
{
    public static class DeleteDeviceEndpoint
    {
        public static void MapDeleteDeviceEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapDelete("/{devEui}", async (
                [FromRoute] string devEui,
                [FromServices] DeleteDeviceUseCase useCase) =>
            {
                await useCase.ExecuteAsync(devEui);

                var apiResponse = ApiResponse<object>.Success(
                    data: null!,
                    message: "Dispositivo excluído com sucesso na Everynet."
                );

                return Results.Ok(apiResponse);
            })
            .WithName("DeleteDevice")
            .WithTags("Management API (Devices)");
        }
    }
}
