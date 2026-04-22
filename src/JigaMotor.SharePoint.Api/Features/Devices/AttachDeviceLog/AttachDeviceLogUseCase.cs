using JigaMotor.SharePoint.Api.Domain.Interfaces;

namespace JigaMotor.SharePoint.Api.Features.Devices.AttachDeviceLog
{
    public class AttachDeviceLogUseCase(IDeviceRepository deviceRepository)
    {
        public async Task<bool> ExecuteAsync(AttachDeviceLogRequest request)
        {
            var device = await deviceRepository.GetByIdentifierAsync(request.Identifier);
            
            if (device == null)
            {
                return false;
            }

            var cleanBase64 = request.LogBase64;
            if (cleanBase64.Contains(","))
                cleanBase64 = cleanBase64.Split(',')[1];

            var fileName = $"{device.Serie}.log";
            var content = Convert.FromBase64String(cleanBase64);

            return await deviceRepository.AddAttachmentAsync(request.Identifier, fileName, content);
        }
    }
}
