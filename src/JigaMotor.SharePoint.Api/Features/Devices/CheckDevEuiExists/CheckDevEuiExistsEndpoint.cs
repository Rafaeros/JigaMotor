using JigaMotor.Shared.Responses;

namespace JigaMotor.SharePoint.Api.Features.Devices.CheckDevEuiExists;

public static class CheckDevEuiExistsEndPoint
{
    public static void MapCheckDevEuiExists(this IEndpointRouteBuilder app)
    {
        app.MapGet("/check-exists/{devEui}", async (string devEui, CheckDevEuiExistsUseCase useCase) =>
        {
            var exists = await useCase.ExistsByDevEuiAsync(devEui);

            var result = new CheckDevEuiExistsResponse(
                DevEui: devEui,
                Exists: exists,
                IsAvailable: !exists
            );

            var response = ApiResponse<CheckDevEuiExistsResponse>.Success(result);

            return Results.Ok(response);
        })
        .WithName("CheckDevEuiExists")
        .WithTags("Devices");
    }

}
