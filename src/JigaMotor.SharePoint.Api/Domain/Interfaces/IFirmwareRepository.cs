using JigaMotor.SharePoint.Api.Domain.Entities;

namespace JigaMotor.SharePoint.Api.Domain.Interfaces;

public interface IFirmwareRepository
{
    Task<List<Firmware>> GetByModelAsync(string model);
    Task<List<Firmware>> GetAllActiveAsync();
}
