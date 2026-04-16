using JigaMotor.SharePoint.Api.Domain.Interfaces;

namespace JigaMotor.SharePoint.Api.Features.Device.CheckDevEuiExists
{
    public class CheckDevEuiExistsUseCase(IDeviceRepository deviceRepository)
    {
        public Task<bool> ExistsByDevEuiAsync(string devEui)
        {
            return deviceRepository.ExistsByDevEuiAsync(devEui);
        }
    }
}
