using JigaMotor.SharePoint.Api.Domain.Entities;
using JigaMotor.SharePoint.Api.Domain.Interfaces;


namespace JigaMotor.SharePoint.Api.Features.Devices.GetDeviceByDevEui
{
    public class GetDeviceByDevEuiUseCase(IDeviceRepository deviceRepository)
    {
        private readonly IDeviceRepository _deviceRepository = deviceRepository;
        public async Task<DeviceProductionRecord?> ExecuteAsync(string devEui)
        {
            return await _deviceRepository.GetByDevEuiAsync(devEui);
        }
    }
}
