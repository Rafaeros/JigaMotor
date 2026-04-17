using JigaMotor.SharePoint.Api.Domain.Entities;
using JigaMotor.SharePoint.Api.Domain.Interfaces;

namespace JigaMotor.SharePoint.Api.Features.Devices.GetAllDevices;

public class GetAllDevicesUseCase(IDeviceRepository deviceRepository)
{
    public async Task<List<DeviceProductionRecord>> GetAllAsync()
    {
        return await deviceRepository.GetAllAsync();
    }
}
