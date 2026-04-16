

namespace JigaMotor.SharePoint.Api.Features.Device.CheckDevEuiExists
{
    public record CheckDevEuiExistsResponse(
        string DevEui,
        bool Exists,
        bool IsAvailable
    );
}
