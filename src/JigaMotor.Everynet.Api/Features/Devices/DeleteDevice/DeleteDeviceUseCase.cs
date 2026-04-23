using JigaMotor.Everynet.Api.Domain.Interfaces;

namespace JigaMotor.Everynet.Api.Features.Devices.DeleteDevice
{
    public class DeleteDeviceUseCase(IEverynetRepository repository)
    {
        public async Task ExecuteAsync(string devEui)
        {
            await repository.DeleteDeviceAsync(devEui);
        }
    }
}
