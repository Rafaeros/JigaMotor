

namespace JigaMotor.SharePoint.Api.Features.Device.CheckDevEuiExists;

public static class CheckDevEuiExistsEndPoint
{
    public static void MapCheckDevEuiExists(this IEndpointRouteBuilder app)
    {
        app.MapGet("/devices/{devEui}", async (string devEui, CheckDevEuiExistsUseCase useCase) =>
        {
            var exists = await useCase.ExistsByDevEuiAsync(devEui);

            var response = new CheckDevEuiExistsResponse(
                DevEui: devEui,
                Exists: exists,
                IsAvailable: !exists
            );

            return Results.Ok(response);
        })
        .WithName("CheckDevEuiExists")
        .WithTags("SharePoint");
}

}
