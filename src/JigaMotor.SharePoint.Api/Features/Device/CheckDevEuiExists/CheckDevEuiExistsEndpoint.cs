

namespace JigaMotor.SharePoint.Api.Features.Device.CheckDevEuiExists;

public static class CheckDevEuiExistsEndPoint
{
    public static void MapCheckDevEuiExists(this IEndpointRouteBuilder app)
    {
        app.MapGet("/devices/{devEui}", async (string devEui, CheckDevEuiExistsUseCase useCase) =>
        {
            var result = await useCase.ExistsByDevEuiAsync(devEui);
            return result ? Results.Ok("Dispositivo já existe com esse DevEui")
                          : Results.NotFound("Nenhum dispositivo encontrado com esse DevEui");
        })
        .WithName("CheckDevEuiExists")
        .WithTags("SharePoint");
}

}
