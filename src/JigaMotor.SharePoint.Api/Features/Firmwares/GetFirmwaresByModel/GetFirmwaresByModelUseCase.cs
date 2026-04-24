using JigaMotor.SharePoint.Api.Domain.Entities;
using JigaMotor.SharePoint.Api.Domain.Interfaces;

namespace JigaMotor.SharePoint.Api.Features.Firmwares.GetFirmwaresByModel;

public class GetFirmwaresByModelUseCase(IFirmwareRepository repository)
{
    public async Task<List<Firmware>> ExecuteAsync(string model)
    {
        var firmwares = await repository.GetByModelAsync(model);
        
        // Filter active ones and ensure both Teste and Producao are included if they exist
        return firmwares
            .Where(f => f.IsActive)
            .OrderBy(f => f.Purpose)
            .ToList();
    }
}
