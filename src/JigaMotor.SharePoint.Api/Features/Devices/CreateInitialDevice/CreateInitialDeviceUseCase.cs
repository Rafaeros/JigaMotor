using JigaMotor.SharePoint.Api.Domain.Entities;
using JigaMotor.SharePoint.Api.Domain.Interfaces;

namespace JigaMotor.SharePoint.Api.Features.Devices.CreateInitialDevice
{
    public class CreateInitialDeviceUseCase(IDeviceRepository deviceRepository)
    {
        public async Task<DeviceProductionRecord> ExecuteAsync(CreateInitialDeviceRequest request)
        {
            var devEuiExists = await deviceRepository.ExistsByDevEuiAsync(request.DevEui);
            if (devEuiExists)
            {
                throw new InvalidOperationException($"O DevEui '{request.DevEui}' já foi gravado.");
            }

            var currentMaxLoraId = await deviceRepository.GetMaxLoraIdAsync();
            var nextLoraId = currentMaxLoraId + 1;
            var nextSerie = $"108{nextLoraId}";
            while (await deviceRepository.ExistsByLoraOrSerieAsync(nextLoraId.ToString(), nextSerie))
            {
                nextLoraId++;
                nextSerie = $"108{nextLoraId}";
            }

            var currentMaxId = await deviceRepository.GetMaxDomainIdAsync();
            var nextId = currentMaxId + 1;

            var batchNumber = $"{nextLoraId:X}{DateTime.Now:MMyy}";

            var deviceToCreate = new DeviceProductionRecord(
                Id: nextId,
                Serie: nextSerie,
                BatchNumber: batchNumber,
                FirmwareVersion: request.Firmware,
                MemoryVersion: request.Memory,
                Bluetooth: request.Bluetooth,
                RecordDate: DateTime.Now,
                Model: request.Model,
                Network: new NetworkKeys(
                    LoraId: nextLoraId.ToString(),
                    DevEui: request.DevEui,
                    AppEui: request.AppEui,
                    DevAddr: request.DevAddr,
                    NwkSKey: request.NwsKey,
                    AppSKey: request.AppSKey
                ),
                Tests: null,
                Consumption: null,
                Metadata: null
            );

            return await deviceRepository.AddInitialAsync(deviceToCreate);
        }
    }
}
