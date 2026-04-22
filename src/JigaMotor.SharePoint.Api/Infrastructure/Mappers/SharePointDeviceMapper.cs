using JigaMotor.SharePoint.Api.Domain.Entities;
using Microsoft.Graph.Models;

namespace JigaMotor.SharePoint.Api.Infrastructure.Mappers;

public static class SharePointDeviceMapper
{
    public static DeviceProductionRecord ToDomain(ListItem item)
    {
        string GetField(string name)
        {
            if (item.Fields?.AdditionalData != null && item.Fields.AdditionalData.TryGetValue(name, out var value))
            {
                return value?.ToString()?.Trim() ?? string.Empty;
            }
            return string.Empty;
        }

        DateTime? GetDate(string name)
        {
            var strValue = GetField(name);
            if (DateTime.TryParse(strValue, out var date)) return date;
            return null;
        }

        var network = new NetworkKeys(
            LoraId: GetField("LORA"),
            DevEui: GetField("DEVEUI"),
            AppEui: GetField("APPEUI"),
            DevAddr: GetField("DEVADDR"),
            NwkSKey: GetField("NWSKEY"),
            AppSKey: GetField("APPSKEY")
        );

        var tests = new HardwareTests(
            P2pStatus: GetField("P2P"),
            AdcStatus: GetField("ADC"),
            EverynetStatus: GetField("EVERYNET"),
            OtpStatus: GetField("OTP"),
            FirmwareStatus: GetField("FW")
        );

        var consumption = new PowerConsumptionTests(
            LrwEmOnStatus: GetField("LRWEMON"),
            AdvEmOnStatus: GetField("ADVEMON"),
            PacketEmOnStatus: GetField("PACKETEMON"),
            LrwEmOffStatus: GetField("LRWEMOFF"),
            AdvEmOffStatus: GetField("ADVEMOFF"),
            PacketEmOffStatus: GetField("PACKETEMOFF")
        );

        var metadata = new ProductionMetadata(
            TagStatus: GetField("TAG"),
            RdpStatus: GetField("RDP"),
            BoxNumber: GetField("Caixa"),
            ProductionOrder: GetField("OrdemProducao"),
            PunchDate: GetDate("DataBatida")
        );

        _ = int.TryParse(GetField("N"), out int domainId);

        return new DeviceProductionRecord(
            Id: domainId,
            Serie: GetField("SERIE"),
            BatchNumber: GetField("LOTE"),
            FirmwareVersion: GetField("FIRMWARE"),
            MemoryVersion: GetField("MEMORY"),
            Bluetooth: GetField("BLUETOOTH"),
            RecordDate: GetDate("DATA") ?? DateTime.MinValue,
            Model: GetField("MODELO"),
            Network: network,
            Tests: tests,
            Consumption: consumption,
            Metadata: metadata
        );
    }

    public static Dictionary<string, object> ToSharePoint(DeviceProductionRecord device)
    {
        var data = new Dictionary<string, object>
        {
            { "LORA", device.Network?.LoraId ?? string.Empty },
            { "DEVEUI", device.Network?.DevEui ?? string.Empty },
            { "APPEUI", device.Network?.AppEui ?? string.Empty },
            { "DEVADDR", device.Network?.DevAddr ?? string.Empty },
            { "NWSKEY", device.Network?.NwkSKey ?? string.Empty },
            { "APPSKEY", device.Network?.AppSKey ?? string.Empty },
            { "SERIE", device.Serie ?? string.Empty },
            { "BLUETOOTH", device.Bluetooth ?? string.Empty },
            { "LOTE", device.BatchNumber ?? string.Empty },
            { "N", device.Id },
            { "FIRMWARE", device.FirmwareVersion ?? string.Empty },
            { "MEMORY", device.MemoryVersion ?? string.Empty },
            { "MODELO", device.Model ?? string.Empty }
        };

        if (device.RecordDate.HasValue && device.RecordDate.Value != DateTime.MinValue)
        {
            data.Add("DATA", device.RecordDate.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }

        return data;
    }
}