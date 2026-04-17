

namespace JigaMotor.SharePoint.Api.Features.Devices.CheckDevEuiExists
{
    public record CheckDevEuiExistsResponse(
        string DevEui,
        bool Exists,
        bool IsAvailable
    );
}
