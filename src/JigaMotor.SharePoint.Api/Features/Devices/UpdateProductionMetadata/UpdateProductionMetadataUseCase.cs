using JigaMotor.SharePoint.Api.Domain.Entities;
using JigaMotor.SharePoint.Api.Domain.Interfaces;

namespace JigaMotor.SharePoint.Api.Features.Devices.UpdateProductionMetadata
{
    public class UpdateProductionMetadataUseCase(IDeviceRepository deviceRepository)
    {
        public async Task<bool> ExecuteAsync(UpdateProductionMetadataRequest request)
        {
            var metadata = new ProductionMetadata(
                request.TagStatus,
                request.RdpStatus,
                request.BoxNumber,
                request.ProductionOrder,
                DateTime.UtcNow
            );

            return await deviceRepository.UpdateProductionMetadataAsync(request.Serie, metadata);
        }
    }
}
