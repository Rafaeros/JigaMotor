using JigaMotor.Everynet.Api.Domain.Entities;

namespace JigaMotor.Everynet.Api.Domain.Interfaces
{
    public interface IEverynetRepository
    {
        Task<EverynetDevice?> GetDeviceByDevEuiAsync(string devEui);
        Task<string> QueueDownlinkAsync(DownlinkCommand command);
        Task<EverynetDevice> CreateDeviceAsync(EverynetDevice device);
        Task<(IEnumerable<EverynetDevice> Devices, int Total)> ListDevicesAsync(int limit, int offset, string? query);
        Task<EverynetDevice> UpdateDeviceAsync(string devEui, EverynetDevice device);
        Task DeleteDeviceAsync(string devEui);
    }
}
