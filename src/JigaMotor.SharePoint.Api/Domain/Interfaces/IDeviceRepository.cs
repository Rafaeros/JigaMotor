using JigaMotor.SharePoint.Api.Domain.Entities;

namespace JigaMotor.SharePoint.Api.Domain.Interfaces
{
    public interface IDeviceRepository
    {

        Task<List<DeviceProductionRecord>> GetAllAsync();

        Task<DeviceProductionRecord?> GetByDevEuiAsync(string devEui);
        
        Task<DeviceProductionRecord?> GetBySerieAsync(string serie);

        Task<bool> ExistsByDevEuiAsync(string devEui);

        Task<bool> ExistsByKeysAsync(string devEui, string lora, string serie);

        Task<bool> ExistsByLoraOrSerieAsync(string lora, string serie);
        
        Task<int> GetMaxLoraIdAsync();
        
        Task<int> GetMaxDomainIdAsync();

        Task<DeviceProductionRecord> AddInitialAsync(DeviceProductionRecord device);

        Task<bool> UpdateHardwareTestsAsync(string identifier, HardwareTests tests);
        
        Task<bool> UpdatePowerConsumptionTestsAsync(string identifier, PowerConsumptionTests tests);

        Task<bool> UpdateProductionMetadataAsync(string identifier, ProductionMetadata metadata);

        Task<bool> DeleteByDevEuiAsync(string devEui);

        Task<bool> AddAttachmentAsync(string identifier, string fileName, byte[] content);

        Task<DeviceProductionRecord?> GetByIdentifierAsync(string identifier);

    }
}
