using JigaMotor.Everynet.Api.Domain.Entities;

namespace JigaMotor.Everynet.Api.Domain.Interfaces
{
    public interface IEverynetRepository
    {
        Task<EverynetDevice?> GetDeviceByDevEuiAsync(string devEui);
        Task<string> QueueDownlinkAsync(DownlinkCommand command);
    }
}
