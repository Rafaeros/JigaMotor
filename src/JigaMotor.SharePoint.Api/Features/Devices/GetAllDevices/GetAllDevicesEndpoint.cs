namespace JigaMotor.SharePoint.Api.Features.Devices.GetAllDevices;

public static class GetAllDevicesEndpoint
{
    public static void MapGetAllDevices(this IEndpointRouteBuilder app)
    {
        app.MapGet("/devices", async (GetAllDevicesUseCase useCase) =>
        {
            var result = await useCase.GetAllAsync();
            return Results.Ok(result);
        })
        .WithName("GetAllDevices")
        .WithTags("SharePoint");
    }
}