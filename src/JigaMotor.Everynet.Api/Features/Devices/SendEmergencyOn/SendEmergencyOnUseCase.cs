using JigaMotor.Everynet.Api.Domain.Entities;
using JigaMotor.Everynet.Api.Domain.Interfaces;

namespace JigaMotor.Everynet.Api.Features.Devices.SendEmergencyOn
{
    public class SendEmergencyOnUseCase(IEverynetRepository everynetRepository)
    {
        public async Task<string> ExecuteAsync(SendEmergencyOnRequest request)
        {
            var command = new DownlinkCommand(request.DevEui, request.PayloadBase64, request.Port, true);
            return await everynetRepository.QueueDownlinkAsync(command);
        }
    }
}
