using JigaMotor.SharePoint.Api.Domain.Entities;
using JigaMotor.SharePoint.Api.Domain.Interfaces;

namespace JigaMotor.SharePoint.Api.Features.Devices.UpdateHardwareTests
{
    public class UpdateHardwareTestsUseCase(IDeviceRepository deviceRepository)
    {
        public async Task<bool> ExecuteAsync(UpdateHardwareTestsRequest request)
        {
            var tests = new HardwareTests(
                request.P2pStatus,
                request.AdcStatus,
                request.EverynetStatus,
                request.OtpStatus,
                request.FirmwareStatus
            );

            return await deviceRepository.UpdateHardwareTestsAsync(request.Identifier, tests);
        }
    }
}
