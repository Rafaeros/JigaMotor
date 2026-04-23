namespace JigaMotor.Everynet.Api.Features.Devices.ListDevices
{
    public record ListDevicesRequest(int Limit = 20, int Offset = 0, string? Q = null);
}
