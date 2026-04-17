namespace JigaMotor.Everynet.Api.Features.Devices.GetDeviceByDevEui
{
    public record GetDeviceByDevEuiResponse(
        string DevEui,
        string AppEui
    );
}
