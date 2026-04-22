namespace JigaMotor.SharePoint.Api.Features.Devices.CheckDeviceAvailability
{
    public record CheckDeviceAvailabilityResponse(
        string DevEui,
        string Lora,
        string Serie,
        bool IsAvailable
    );
}
