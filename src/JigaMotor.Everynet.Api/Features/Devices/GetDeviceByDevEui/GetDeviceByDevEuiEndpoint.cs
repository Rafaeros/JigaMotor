namespace JigaMotor.Everynet.Api.Features.Devices.GetDeviceByDevEui
{
    public static class GetDeviceByDevEuiEndpoint
    {
        public static void MapGetDeviceByDevEuiEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{devEui}", async (string devEui, GetDeviceByDevEuiUseCase useCase) =>
            {
                var device = await useCase.ExecuteAsync(devEui);
                return device is not null ? Results.Ok(device) : Results.NotFound();
            })
            .WithName("GetDeviceByDevEui")
            .WithTags("Devices");
        }
    }
}
