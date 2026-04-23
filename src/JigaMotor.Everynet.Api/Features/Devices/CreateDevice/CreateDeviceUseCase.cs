using JigaMotor.Everynet.Api.Domain.Entities;
using JigaMotor.Everynet.Api.Domain.Interfaces;

namespace JigaMotor.Everynet.Api.Features.Devices.CreateDevice
{
    public class CreateDeviceUseCase(IEverynetRepository repository)
    {
        public async Task<EverynetDevice> ExecuteAsync(CreateDeviceRequest request)
        {
            var activation = string.IsNullOrWhiteSpace(request.Activation) ? "ABP" : request.Activation;
            var encryption = string.IsNullOrWhiteSpace(request.Encryption) ? "NS" : request.Encryption;
            var devClass = string.IsNullOrWhiteSpace(request.DevClass) ? "A" : request.DevClass;
            var band = string.IsNullOrWhiteSpace(request.Band) ? "LA915-928A" : request.Band;

            var adr = request.Adr;
            if (adr == null)
            {
                adr = new Adr(null, null, "off", null, null);
            }
            else if (string.IsNullOrWhiteSpace(adr.Mode))
            {
                adr = adr with { Mode = "off" };
            }

            var device = new EverynetDevice(
                request.DevEui,
                request.AppEui,
                request.Tags ?? [],
                activation,
                encryption,
                request.DevAddr,
                request.Nwkskey,
                request.Appskey,
                request.AppKey,
                devClass,
                request.CountersSize ?? 4,
                adr,
                band,
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

            return await repository.CreateDeviceAsync(device);
        }
    }
}
