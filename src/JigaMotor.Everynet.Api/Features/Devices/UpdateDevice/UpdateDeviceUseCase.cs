using JigaMotor.Everynet.Api.Domain.Entities;
using JigaMotor.Everynet.Api.Domain.Interfaces;

namespace JigaMotor.Everynet.Api.Features.Devices.UpdateDevice
{
    public class UpdateDeviceUseCase(IEverynetRepository repository)
    {
        public async Task<EverynetDevice> ExecuteAsync(string devEui, UpdateDeviceRequest request)
        {
            var device = new EverynetDevice(
                devEui, 
                request.AppEui,
                request.Tags,
                request.Activation,
                request.Encryption,
                request.DevAddr,
                request.Nwkskey,
                request.Appskey,
                request.AppKey,
                request.DevClass,
                request.CountersSize,
                request.Adr,
                request.Band,
                null, // BlockDownlink
                null, // BlockUplink
                null, // CounterDown
                null, // CounterUp
                null, // CreatedAt
                null, // LastActivity
                null, // LastJoin
                null, // Locked
                null, // LorawanVersion
                null, // Multicast
                null, // StrictCounter
                null, // Geolocation
                null, // Rx1
                null  // Rx2
            );

            return await repository.UpdateDeviceAsync(devEui, device);
        }
    }
}
