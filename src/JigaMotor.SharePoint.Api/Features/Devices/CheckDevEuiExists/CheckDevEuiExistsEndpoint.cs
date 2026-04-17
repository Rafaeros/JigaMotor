using JigaMotor.Shared.Responses;

namespace JigaMotor.SharePoint.Api.Features.Devices.CheckDevEuiExists;

public static class CheckDevEuiExistsEndPoint
{
    public static void MapCheckDevEuiExists(this IEndpointRouteBuilder app)
    {
        app.MapGet("/devices/check-exists/{devEui}", async (string devEui, CheckDevEuiExistsUseCase useCase) =>
        {
            var exists = await useCase.ExistsByDevEuiAsync(devEui);

            var response = new CheckDevEuiExistsResponse(
                DevEui: devEui,
                Exists: exists,
                IsAvailable: !exists
            );

            var apiResponse = ApiResponse<CheckDevEuiExistsResponse>.Success(response);

            return Results.Ok(apiResponse);
        })
        .WithName("CheckDevEuiExists")
        .WithTags("Devices");
    }

}
