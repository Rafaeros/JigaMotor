using JigaMotor.SharePoint.Api.Domain.Interfaces;

namespace JigaMotor.SharePoint.Api.Features.Devices.DeleteDeviceByDevEui
{
    public class DeleteDeviceByDevEuiUseCase(IDeviceRepository deviceRepository)
    {
        public async Task<bool> ExecuteAsync(string devEui)
        {
            return await deviceRepository.DeleteByDevEuiAsync(devEui);
        }
    }
}
