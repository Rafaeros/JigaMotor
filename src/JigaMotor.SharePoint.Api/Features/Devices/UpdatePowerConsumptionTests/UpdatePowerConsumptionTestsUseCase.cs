using JigaMotor.SharePoint.Api.Domain.Entities;
using JigaMotor.SharePoint.Api.Domain.Interfaces;

namespace JigaMotor.SharePoint.Api.Features.Devices.UpdatePowerConsumptionTests
{
    public class UpdatePowerConsumptionTestsUseCase(IDeviceRepository deviceRepository)
    {
        public async Task<bool> ExecuteAsync(UpdatePowerConsumptionTestsRequest request)
        {
            var tests = new PowerConsumptionTests(
                request.LrwEmOnStatus,
                request.AdvEmOnStatus,
                request.PacketEmOnStatus,
                request.LrwEmOffStatus,
                request.AdvEmOffStatus,
                request.PacketEmOffStatus
            );

            return await deviceRepository.UpdatePowerConsumptionTestsAsync(request.Identifier, tests);
        }
    }
}
