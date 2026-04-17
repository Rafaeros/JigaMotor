
namespace JigaMotor.SharePoint.Api.Features.Devices.GetDeviceByDevEui
{
    public static class GetDeviceByDevEuiEndpoint
    {
        public static void MapGetDeviceByDevEui(this IEndpointRouteBuilder app)
        {
            app.MapGet("/devices/{devEui}", async (string devEui, GetDeviceByDevEuiUseCase useCase) =>
            {
                var device = await useCase.ExecuteAsync(devEui);
                return device != null ? Results.Ok(device) : Results.NotFound();
            })
            .WithName("GetDeviceByDevEui")
            .WithTags("Devices");
        }
    }
}
