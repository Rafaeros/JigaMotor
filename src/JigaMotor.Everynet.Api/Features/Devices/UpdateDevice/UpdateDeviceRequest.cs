using JigaMotor.Everynet.Api.Domain.Entities;

namespace JigaMotor.Everynet.Api.Features.Devices.UpdateDevice
{
    public record UpdateDeviceRequest(
        string AppEui,
        string[] Tags,
        string Activation,
        string Encryption,
        string? DevAddr,
        string? Nwkskey,
        string? Appskey,
        string? AppKey,
        string DevClass,
        int CountersSize,
        Adr? Adr,
        string Band
    );
}
