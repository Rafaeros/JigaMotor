using JigaMotor.Everynet.Api.Domain.Entities;
using JigaMotor.Everynet.Api.Domain.Interfaces;

namespace JigaMotor.Everynet.Api.Features.Devices.SendEmergencyOff
{
    public class SendEmergencyOffUseCase(IEverynetRepository everynetRepository)
    {
        public async Task<string> ExecuteAsync(SendEmergencyOffRequest request)
        {
            var command = new DownlinkCommand(request.DevEui, request.PayloadBase64, request.Port, true);
            return await everynetRepository.QueueDownlinkAsync(command);
        }
    }
}
