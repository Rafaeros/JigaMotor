using JigaMotor.SharePoint.Api.Domain.Entities;

namespace JigaMotor.SharePoint.Api.Domain.Interfaces
{
    public interface IDeviceRepository
    {
        Task<List<DeviceProductionRecord>> GetAllAsync();
        Task<bool> ExistsByDevEuiAsync(string devEui);
    }
}
