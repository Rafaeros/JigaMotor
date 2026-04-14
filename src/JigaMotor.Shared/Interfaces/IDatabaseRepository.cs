using JigaMotor.Shared.Models;


namespace JigaMotor.Shared.Interfaces
{
    public interface IDeviceRepository
    {
        Task<List<DeviceItem>> GetAllAsync();

    }
}
