using JigaMotor.Everynet.Api.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace JigaMotor.Everynet.Api.Features.Devices.CreateDevice
{
    public record CreateDeviceRequest(
        [Required] string DevEui,
        [Required] string AppEui,
        [Required] string[] Tags,
        [DefaultValue("ABP")] string? Activation,
        [DefaultValue("NS")] string? Encryption,
        [Required] string DevAddr,
        [Required] string Nwkskey,
        [Required] string Appskey,
        string? AppKey,
        [DefaultValue("A")] string? DevClass,
        [DefaultValue(4)] int? CountersSize,
        Adr? Adr,
        [DefaultValue("LA915-928A")] string? Band
    );
}
