using JigaMotor.Shared.Models;
using JigaMotor.Shared.Interfaces;

namespace JigaMotor.SharePoint.Api.Features.Read
{
    public class ReadUseCase(IDeviceRepository deviceRepository)
    {
        public async Task<DeviceListResponse> GetAllAsync()
        {
            return new DeviceListResponse(await deviceRepository.GetAllAsync());
        }
    }
}
