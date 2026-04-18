using JigaMotor.Everynet.Api.Domain.Entities;
using JigaMotor.Everynet.Api.Domain.Interfaces;

namespace JigaMotor.Everynet.Api.Features.Devices.SendEmergencyOff
{
    public class SendEmergencyOffUseCase(IEverynetRepository everynetRepository)
    {
        public async Task<SendEmergencyOffResponse> ExecuteAsync(SendEmergencyOffRequest request)
        {
            var command = new DownlinkCommand(request.DevEui, request.PayloadBase64, request.Port, true);

            var messageId = await everynetRepository.QueueDownlinkAsync(command);

            return new SendEmergencyOffResponse(request.DevEui, messageId, DateTime.UtcNow);
        }
    }
}