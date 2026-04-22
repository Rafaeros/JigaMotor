using JigaMotor.SharePoint.Api.Domain.Interfaces;

namespace JigaMotor.SharePoint.Api.Features.Devices.CheckDeviceAvailability
{
    public class CheckDeviceAvailabilityUseCase(IDeviceRepository deviceRepository)
    {
        public async Task<bool> IsAvailableAsync(CheckDeviceAvailabilityRequest request)
        {
            var exists = await deviceRepository.ExistsByKeysAsync(request.DevEui, request.Lora, request.Serie);
            return !exists;
        }
    }
}
