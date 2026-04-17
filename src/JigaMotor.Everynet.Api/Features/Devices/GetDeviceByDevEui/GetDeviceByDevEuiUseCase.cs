using JigaMotor.Everynet.Api.Domain.Entities;
using JigaMotor.Everynet.Api.Domain.Interfaces;

namespace JigaMotor.Everynet.Api.Features.Devices.GetDeviceByDevEui

{
    public class GetDeviceByDevEuiUseCase(IEverynetRepository everynetRepository)
    {
        public async Task<EverynetDevice?> ExecuteAsync(string devEui)
        {
            var device = await everynetRepository.GetDeviceByDevEuiAsync(devEui)
                ?? throw new Exception($"Device with DevEui {devEui} not found.");

            return device;
        }
    }
}
