using JigaMotor.Everynet.Api.Domain.Entities;
using JigaMotor.Everynet.Api.Domain.Interfaces;

namespace JigaMotor.Everynet.Api.Features.Devices.ListDevices
{
    public class ListDevicesUseCase(IEverynetRepository repository)
    {
        public async Task<(IEnumerable<EverynetDevice> Devices, int Total)> ExecuteAsync(ListDevicesRequest request)
        {
            return await repository.ListDevicesAsync(request.Limit, request.Offset, request.Q);
        }
    }
}
